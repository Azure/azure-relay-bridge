// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Relay.Bridge.Configuration;

    sealed class RemoteForwardBridge : IDisposable
    {
        private readonly Config config;
        readonly RelayConnectionStringBuilder connectionString;

        readonly IDictionary<string, IRemoteForwarder> remoteForwarders;

        static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
        HybridConnectionListener listener;
        readonly CancellationTokenSource shuttingDown = new CancellationTokenSource();
        readonly EventTraceActivity activity = BridgeEventSource.NewActivity("RemoteForwardBridge");
        static Random rnd = new Random();

        internal RemoteForwardBridge(Config config, RelayConnectionStringBuilder connectionString, IDictionary<string,IRemoteForwarder> remoteForwarders)
        {
            this.config = config;
            this.connectionString = connectionString;
            this.remoteForwarders = remoteForwarders;
        }

        public event EventHandler Connecting;
        public event EventHandler Offline;
        public event EventHandler Online;

        public bool IsOnline => this.listener.IsOnline;

        public Exception LastError => this.listener.LastError;

        bool IsOpen { get; set; }

        public async void Close()
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException();
            }

            shuttingDown.Cancel(); ;

            this.IsOpen = false;
            await this.listener.CloseAsync(TimeSpan.FromSeconds(5));
        }

        public void Dispose()
        {
            this.IsOpen = false;
            this.listener.CloseAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Opens this PortBridgeServerProxy instance and listens for new connections coming through Service Bus.
        /// </summary>
        /// <exception cref="System.Security.SecurityException">Throws a SecurityException if Group Policy prohibits Resource Publishing.</exception>
        public async Task Open()
        {
            if (this.IsOpen)
            {
                throw new InvalidOperationException();
            }

            this.listener = new HybridConnectionListener(connectionString.ToString());
            this.listener.Online += (s, e) => { Online?.Invoke(this, e); };
            this.listener.Offline += (s, e) => { Offline?.Invoke(this, e); };
            this.listener.Connecting += (s, e) => { Connecting?.Invoke(this, e); };

            await listener.OpenAsync(shuttingDown.Token);
            this.IsOpen = true;

            AcceptLoopAsync().Fork(this);
        }

        async Task AcceptLoopAsync()
        {
            while (!shuttingDown.IsCancellationRequested)
            {
                try
                {
                    var hybridConnectionStream = await listener.AcceptConnectionAsync();
                    if (hybridConnectionStream == null)
                    {
                        // we only get null if trhe listener is shutting down
                        break;
                    }
                    HandleRelayConnectionAsync(hybridConnectionStream).Fork(this);
                }
                catch (Exception e)
                {
                    BridgeEventSource.Log.HandledExceptionAsWarning(activity, e);
                }
            }
        }

        async Task HandleRelayConnectionAsync(HybridConnectionStream hybridConnectionStream)
        {
            try
            {
                using (hybridConnectionStream)
                {
                    string portName = null;

                    hybridConnectionStream.WriteTimeout = 60000;

                    // read and write 4-byte header
                    // we don't do anything with this version preamble just yet; it really 
                    // is insurance for when we might have to break protocol.
                    var versionPreamble = new byte[3];
                    for (int read = 0; read < versionPreamble.Length;)
                    {
                        var r = await hybridConnectionStream.ReadAsync(versionPreamble, read,
                            versionPreamble.Length - read);
                        if (r == 0)
                        {
                            await hybridConnectionStream.ShutdownAsync(CancellationToken.None);
                            return;
                        }

                        read += r;
                    }

                    // version 1.0 and stream mode (0)
                    if (versionPreamble[0] == 1 && versionPreamble[1] == 0 &&
                        (versionPreamble[2] == 0 || versionPreamble[2] == 1))
                    {
                        // For version 1.0, the version preamble is followed by a single byte 
                        // length indicator and then that number of bytes with of UTF-8 encoded
                        // port-name string.
                        var portNameBuffer = new byte[256];
                        var r = await hybridConnectionStream.ReadAsync(portNameBuffer, 0, 1);
                        if (r == 0)
                        {
                            await hybridConnectionStream.ShutdownAsync(CancellationToken.None);
                            return;
                        }

                        for (int read = 0; read < portNameBuffer[0];)
                        {
                            r = await hybridConnectionStream.ReadAsync(portNameBuffer, read + 1,
                                portNameBuffer[0] - read);
                            if (r == 0)
                            {
                                await hybridConnectionStream.ShutdownAsync(CancellationToken.None);
                                return;
                            }

                            read += r;
                        }

                        portName = Encoding.UTF8.GetString(portNameBuffer, 1, portNameBuffer[0]);
                    }
                    else
                    {
                        // if we don't understand the version, we write a 0.0 version preamble back and shut down the connection
                        versionPreamble = new byte[] { 0, 0, 0 };
                        await hybridConnectionStream.WriteAsync(versionPreamble, 0, versionPreamble.Length);
                        await CloseConnection(hybridConnectionStream);
                        return;
                    }

                    if (remoteForwarders.TryGetValue(portName, out var forwarder))
                    {
                        if (forwarder is UdpRemoteForwarder && versionPreamble[2] != 1)
                        {
                            // bad datagram indicator
                            versionPreamble = new byte[] { 0, 0, 1 };
                            await hybridConnectionStream.WriteAsync(versionPreamble, 0, versionPreamble.Length);
                            await CloseConnection(hybridConnectionStream);
                            return;
                        }
                        else if (!(forwarder is UdpRemoteForwarder) && versionPreamble[2] == 1)
                        {
                            // mismatch
                            versionPreamble = new byte[] { 0, 0, 255 };
                            await hybridConnectionStream.WriteAsync(versionPreamble, 0, versionPreamble.Length);
                            await CloseConnection(hybridConnectionStream);
                            return;
                        }

                        // write out 1.0 and handle the stream
                        versionPreamble = new byte[] { 1, 0, versionPreamble[2] };
                        await hybridConnectionStream.WriteAsync(versionPreamble, 0, versionPreamble.Length);
                        await forwarder.HandleConnectionAsync(hybridConnectionStream);
                        await CloseConnection(hybridConnectionStream);
                    }
                    else
                    {
                        await CloseConnection(hybridConnectionStream);
                    }
                }
            }
            catch (Exception e)
            {
                BridgeEventSource.Log.HandledExceptionAsWarning(activity, e);
            }
        }

        static async Task CloseConnection(HybridConnectionStream hybridConnectionStream)
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1)))
            {
                await hybridConnectionStream.ShutdownAsync(cts.Token);
                await hybridConnectionStream.CloseAsync(cts.Token);
            }
        }
    }
}