// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NETFRAMEWORK
namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.Azure.Relay;

    sealed class SocketRemoteForwardBridge : IDisposable
    {
        private readonly Config config;
        readonly RelayConnectionStringBuilder connectionString;
        static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
        readonly string targetServer;
        HybridConnectionListener listener;
        CancellationTokenSource shuttingDown = new CancellationTokenSource();
        EventTraceActivity activity = BridgeEventSource.NewActivity("RemoteForwardBridge");
        static Random rnd = new Random();

        internal SocketRemoteForwardBridge(Config config, RelayConnectionStringBuilder connectionString, string targetSocket)
        {
            this.config = config;
            this.connectionString = connectionString;
            this.targetServer = targetSocket;
        }

        public event EventHandler Connecting;
        public event EventHandler Offline;
        public event EventHandler Online;

        public bool IsOnline
        {
            get { return this.listener.IsOnline; }
        }

        public Exception LastError
        {
            get { return this.listener.LastError; }
        }

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
                    ConnectionPumpLoopAsync(hybridConnectionStream).Fork(this);
                }
                catch (Exception e)
                {
                    BridgeEventSource.Log.HandledExceptionAsWarning(activity, e);
                }
            }
        }

        async Task ConnectionPumpLoopAsync(Microsoft.Azure.Relay.HybridConnectionStream hybridConnectionStream)
        {
            try
            {
                using (hybridConnectionStream)
                {
                    hybridConnectionStream.WriteTimeout = 60000;

                    // read and write 4-byte header
                    // we don't do anything with this version preamble just yet; it really 
                    // is insurance for when we might have to break protocol.
                    byte[] versionPreamble = new byte[4];
                    for (int read = 0; read < versionPreamble.Length; read += await hybridConnectionStream.ReadAsync(versionPreamble, read, versionPreamble.Length - read)) ;
                    versionPreamble = new byte[] { 1, 0, 0, 0 };
                    await hybridConnectionStream.WriteAsync(versionPreamble, 0, versionPreamble.Length);
                        
                    
                    using (Socket socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP))
                    {   
                        socket.SendBufferSize = socket.ReceiveBufferSize = 65536;
                        socket.SendTimeout = 60000;
                        await socket.ConnectAsync(new UnixDomainSocketEndPoint(targetServer));
                        var tcpstream = new NetworkStream(socket);

                        CancellationTokenSource socketAbort = new CancellationTokenSource();
                        await Task.WhenAll(
                            StreamPump.RunAsync(hybridConnectionStream, tcpstream,
                                () => socket.Shutdown(SocketShutdown.Send), socketAbort.Token)
                                .ContinueWith((t) => socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted),
                            StreamPump.RunAsync(tcpstream, hybridConnectionStream, () => hybridConnectionStream.Shutdown(), socketAbort.Token))
                                .ContinueWith((t) => socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted);


                        using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1)))
                        {
                            await hybridConnectionStream.ShutdownAsync(cts.Token);
                            await hybridConnectionStream.CloseAsync(cts.Token);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                BridgeEventSource.Log.HandledExceptionAsWarning(activity, e);
            }
        }

    }
}
#endif