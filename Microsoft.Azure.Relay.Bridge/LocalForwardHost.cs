// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Microsoft.Azure.Relay;
    using Microsoft.Azure.Relay.Bridge.Configuration;

    sealed class LocalForwardHost
    {
        readonly Dictionary<string, TcpLocalForwardBridge> listenerBridges = new Dictionary<string, TcpLocalForwardBridge>();
        private Config config;

        public LocalForwardHost(Config config)
        {
            this.config = config;
        }

        public void Start()
        {
            EventSource.Log.HybridConnectionClientServiceStarting();
            this.StartEndpoints(this.config.LocalForward);
        }

        public void Stop()
        {
            EventSource.Log.HybridConnectionClientServiceStopping();
            this.StopEndpoints();
        }

        void StartEndpoint(LocalForward localForward)
        {
            var activity = new EventTraceActivity();
            Uri hybridConnectionUri = null;
            TcpLocalForwardBridge tcpListenerBridge = null;

            var rcbs = localForward.RelayConnectionStringBuilder ?? new RelayConnectionStringBuilder(config.AzureRelayConnectionString);
            rcbs.EntityPath = localForward.RelayName;
            hybridConnectionUri = new Uri(rcbs.Endpoint, rcbs.EntityPath);

            try
            {
                IPHostEntry localHostEntry = Dns.GetHostEntry(Dns.GetHostName());

                // Resolve the host name. Whether this is in the hosts file or in some 
                // form of DNS server shouldn't matter for us here (means we do not touch 
                // the hosts file in this process), but the address MUST resolve to a local 
                // endpoint or to a loopback endpoint

                IPAddress bindToAddress;
                Random rnd = new Random();
                if (!IPAddress.TryParse(localForward.BindAddress, out bindToAddress))
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(localForward.BindAddress);
                    bindToAddress = hostEntry.AddressList[rnd.Next(hostEntry.AddressList.Length)];
                }

                if (bindToAddress != null)
                {
                    tcpListenerBridge = TcpLocalForwardBridge.FromConnectionString(rcbs);
                    tcpListenerBridge.Run(new IPEndPoint(bindToAddress, localForward.BindPort));
                    this.listenerBridges.Add(hybridConnectionUri.AbsoluteUri, tcpListenerBridge);
                    EventSource.Log.HybridConnectionClientStarted(activity,
                        hybridConnectionUri.AbsoluteUri);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        internal void UpdateConfig(Config config)
        {
            this.config = config;

            // stopping the listeners will actually not cut existing
            // connections.

            StopEndpoints();
            StartEndpoints(config.LocalForward);
        }

        void StartEndpoints(IEnumerable<LocalForward> localForwardSettings)
        {
            foreach (var tcpListenerSetting in localForwardSettings)
            {
                this.StartEndpoint(tcpListenerSetting);
            }
        }

        void StopEndpoint(TcpLocalForwardBridge tcpLocalForwardBridge)
        {
            tcpLocalForwardBridge.Close();
        }

        void StopEndpoints()
        {
            foreach (var bridge in this.listenerBridges.Values)
            {
                this.StopEndpoint(bridge);
            }
        }

    }
}