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
        private EventTraceActivity activity = BridgeEventSource.NewActivity("LocalForwardHost");

        public LocalForwardHost(Config config)
        {
            this.config = config;
        }

        public void Start()
        {
            activity.DiagnosticsActivity.Start();
            BridgeEventSource.Log.LocalForwardHostStarting(activity);
            this.StartEndpoints(this.config.LocalForward);
            BridgeEventSource.Log.LocalForwardHostStart(activity);
        }

        public void Stop()
        {
            try
            {
                BridgeEventSource.Log.LocalForwardHostStopping(activity);
                this.StopEndpoints();
                BridgeEventSource.Log.LocalForwardHostStop(activity);
            }
            finally
            {
                activity.DiagnosticsActivity.Stop();
            }
        }

        void StartEndpoint(LocalForward localForward)
        {
            var startActivity = BridgeEventSource.NewActivity("LocalForwardBridgeStart", activity);
            Uri hybridConnectionUri = null;
            TcpLocalForwardBridge tcpListenerBridge = null;

            BridgeEventSource.Log.LocalForwardBridgeStarting(startActivity, localForward);

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
                }
                BridgeEventSource.Log.LocalForwardBridgeStart(startActivity, bindToAddress, localForward);
            }
            catch (Exception e)
            {
                BridgeEventSource.Log.LocalForwardBridgeStartFailure(startActivity, localForward, e);
                if ( !config.ExitOnForwardFailure.HasValue ||
                     config.ExitOnForwardFailure.Value)
                {
                    throw;
                }
            }

        }

        internal void UpdateConfig(Config config)
        {
            EventTraceActivity updateActivity = BridgeEventSource.NewActivity("UpdateConfig", activity);
            BridgeEventSource.Log.LocalForwardConfigUpdating(updateActivity, config, this.config);
            this.config = config;

            // stopping the listeners will actually not cut existing
            // connections.

            StopEndpoints();
            StartEndpoints(config.LocalForward);

            BridgeEventSource.Log.LocalForwardConfigUpdated(updateActivity);
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
            EventTraceActivity stopActivity = BridgeEventSource.NewActivity("LocalForwardBridgeStop", activity);
            try
            {
                BridgeEventSource.Log.LocalForwardBridgeStopping(stopActivity, tcpLocalForwardBridge);
                tcpLocalForwardBridge.Close();
                BridgeEventSource.Log.LocalForwardBridgeStop(stopActivity, tcpLocalForwardBridge);
            }
            catch (Exception e)
            {
                BridgeEventSource.Log.LocalForwardBridgeStopFailure(stopActivity, tcpLocalForwardBridge, e);
                if ( Fx.IsFatal(e))
                {
                    throw;
                }
            }
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