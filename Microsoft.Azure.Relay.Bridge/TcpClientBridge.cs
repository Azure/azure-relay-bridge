// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Relay;
    using Microsoft.Win32;

    sealed class TcpClientBridge : IDisposable
    {
        readonly RelayConnectionStringBuilder connectionString;
        static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
        readonly int targetPort;
        readonly string targetServer;
        Microsoft.Azure.Relay.HybridConnectionListener listener;
        CancellationTokenSource shuttingDown = new CancellationTokenSource();

        internal TcpClientBridge(RelayConnectionStringBuilder connectionString, string targetServer, int targetPort)
        {
            this.connectionString = connectionString;
            this.targetServer = targetServer;
            this.targetPort = targetPort;
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

            shuttingDown.Cancel();            ;

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
#if WINDOWS
            this.EnforceSecurityPolicy(); // Open a v2 connection.
#endif
            this.listener = new HybridConnectionListener(connectionString.ToString());

            this.listener.Online += (s, e) => { Online?.Invoke(s, e); };
            this.listener.Offline += (s, e) => { Offline?.Invoke(s, e); };
            this.listener.Connecting += (s, e) => { Connecting?.Invoke(s, e); };

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
                    EventSource.Log.HybridConnectionManagerTrace(null, e.ToString());
                }
            }
        }

        async Task ConnectionPumpLoopAsync(Microsoft.Azure.Relay.HybridConnectionStream hybridConnectionStream)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(targetServer, targetPort);
                    await Task.WhenAll(
                        StreamPump.RunAsync(hybridConnectionStream, client.GetStream(), shuttingDown.Token), 
                        StreamPump.RunAsync(client.GetStream(), hybridConnectionStream, shuttingDown.Token));
                }
            }
            catch (Exception e)
            {
                EventSource.Log.HybridConnectionManagerTrace(null, e.ToString());
            }

            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1)))
            {
                await hybridConnectionStream.CloseAsync(cts.Token);
            }
        }

#if WINDOWS
        void EnforceSecurityPolicy()
        {
            const string windowsAzureBizTalkServicesPolicyKey =
                @"SOFTWARE\Policies\Microsoft\Azure\HybridConnections\1.0";
            const string resourcePublishingEnabled = "AllowHybridConnections";
            const string resourcePublishingAllowedServers = "AllowedHybridConnectionsServers";

            using (var settingsKey = Registry.LocalMachine.OpenSubKey(windowsAzureBizTalkServicesPolicyKey))
            {
                var resourcePublishingValue = settingsKey?.GetValue(resourcePublishingEnabled);
                if (resourcePublishingValue != null)
                {
                    if ((int)resourcePublishingValue == 0)
                    {
                        throw new SecurityException("Publishing not allowed per Group Policy");
                    }

                    string[] allowedServers = (string[])settingsKey.GetValue(resourcePublishingAllowedServers);
                    if (allowedServers == null || allowedServers.Length <= 0)
                    {
                        return;
                    }
                    string hostAndPort = this.targetServer + ":" + this.targetPort;
                    bool endpointIsAllowed = false;
                    foreach (var allowedServer in allowedServers)
                    {
                        if (string.Equals(hostAndPort, allowedServer, StringComparison.OrdinalIgnoreCase))
                        {
                            endpointIsAllowed = true;
                            break;
                        }
                    }

                    if (!endpointIsAllowed)
                    {
                        throw new SecurityException("Publishing not allowed per Group Policy");
                    }
                }
            }
        }
#endif
    }
}