// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Microsoft.Azure.Relay;
    using Microsoft.HybridConnectionManager.Configuration;

    sealed class TcpListenerHost
    {
        readonly Dictionary<string, TcpListenerBridge> listenerBridges = new Dictionary<string, TcpListenerBridge>();
        private readonly IEnumerable<ConnectionListener> connectionInfoCollection;

        public TcpListenerHost(IEnumerable<ConnectionListener> connectionInfoCollection)
        {
            this.connectionInfoCollection = connectionInfoCollection;
        }

        public void Start()
        {
            EventSource.Log.HybridConnectionClientServiceStarting();

        }

        public void Stop()
        {
            EventSource.Log.HybridConnectionClientServiceStopping();
            this.StopEndpoints();
        }

        void StartEndpoint(ConnectionListener setting)
        {
            var activity = new EventTraceActivity();
            Uri hybridConnectionUri = null;
            TcpListenerBridge tcpListenerBridge = null;

            var rcbs = new RelayConnectionStringBuilder(setting.ConnectionString);
            hybridConnectionUri = rcbs.Endpoint;
            try
            {
                IPHostEntry localHostEntry = Dns.GetHostEntry(Dns.GetHostName());

                // Resolve the host name. Whether this is in the hosts file or in some 
                // form of DNS server shouldn't matter for us here (means we do not touch 
                // the hosts file in this process), but the address MUST resolve to a local 
                // endpoint or to a loopback endpoint
                IPHostEntry hostEntry = Dns.GetHostEntry(setting.HostName);
                IPAddress bindToAddress = null;
                foreach (var address in hostEntry.AddressList)
                {
                    if (IPAddress.IsLoopback(address))
                    {
                        // we bind to the first loopback address we find here
                        bindToAddress = address;
                        break;
                    }
                    if (localHostEntry.AddressList.FirstOrDefault((e) => e.Equals(address)) != default(IPAddress))
                    {
                        // if the address is the public address of this machine, we bind to that
                        bindToAddress = address;
                        break;
                    }
                }
                if (bindToAddress != null)
                {
                    tcpListenerBridge = TcpListenerBridge.FromConnectionString(setting.ConnectionString);
                    tcpListenerBridge.Run(new IPEndPoint(bindToAddress, setting.Port));
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

        internal void UpdateConfig(List<ConnectionListener> listeners)
        {
            
        }

        void StartEndpoints(IEnumerable<ConnectionListener> tcpListenerSettings)
        {
            foreach (var tcpListenerSetting in tcpListenerSettings)
            {
                this.StartEndpoint(tcpListenerSetting);
            }
        }

        void StopEndpoint(TcpListenerBridge tcpListenerBridge)
        {
            tcpListenerBridge.Close();
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