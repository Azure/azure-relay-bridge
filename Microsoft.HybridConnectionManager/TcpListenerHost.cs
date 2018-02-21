// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    sealed class TcpListenerHost
    {
        readonly Dictionary<string, TcpListenerBridge> listenerBridges = new Dictionary<string, TcpListenerBridge>();
        private readonly TcpListenerSettingsCollection connectionInfoCollection;

        public TcpListenerHost(TcpListenerSettingsCollection connectionInfoCollection)
        {
            this.connectionInfoCollection = connectionInfoCollection;
        }

        public void Start(string[] args)
        {
            EventSource.Log.HybridConnectionClientServiceStarting();

        }

        public void Stop()
        {
            EventSource.Log.HybridConnectionClientServiceStopping();
            this.StopEndpoints();
        }

        void StartEndpoint(TcpListenerSetting setting)
        {
            var activity = new EventTraceActivity();
            Uri hybridConnectionUri = null;
            TcpListenerBridge tcpListenerBridge = null;

            try
            {
                IPHostEntry localHostEntry = Dns.GetHostEntry(Dns.GetHostName());

                // Resolve the host name. Whether this is in the hosts file or in some 
                // form of DNS server shouldn't matter for us here (means we do not touch 
                // the hosts file in this process), but the address MUST resolve to a local 
                // endpoint or to a loopback endpoint
                IPHostEntry hostEntry = Dns.GetHostEntry(setting.ListenHostName);
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
                    tcpListenerBridge = TcpListenerBridge.FromConnectionString(setting.RelayConnectionString);
                    tcpListenerBridge.Run(new IPEndPoint(bindToAddress, setting.ListenPort));
                    this.listenerBridges.Add(setting.Key, tcpListenerBridge);
                    EventSource.Log.HybridConnectionClientStarted(activity,
                        hybridConnectionUri.AbsoluteUri);
                }
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        void StartEndpoints(TcpListenerSettingsCollection tcpListenerSettings)
        {
            foreach (var tcpListenerSetting in tcpListenerSettings.Values)
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