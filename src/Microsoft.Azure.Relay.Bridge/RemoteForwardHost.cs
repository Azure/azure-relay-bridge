// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Relay;
    using Microsoft.Azure.Relay.Bridge.Configuration;

    sealed class RemoteForwardHost
    {
        readonly Dictionary<string, TcpRemoteForwardBridge> clientBridges =
            new Dictionary<string, TcpRemoteForwardBridge>();
        private Config config;
        private EventTraceActivity activity = new EventTraceActivity();

        public RemoteForwardHost(Config config)
        {
            this.config = config;
        }

        public void Start()
        {
            try
            {
                BridgeEventSource.Log.RemoteForwardHostStarting(this.activity);
                StartEndpoints(config.RemoteForward);
                BridgeEventSource.Log.RemoteForwardHostStarted(this.activity);
            }
            catch (Exception e)
            {
                BridgeEventSource.Log.RemoteForwardHostFailedToStart(activity, e);
            }
        }

        public void Stop()
        {
            BridgeEventSource.Log.RemoteForwardHostStopping(activity);
            this.StopEndpoints();
            BridgeEventSource.Log.RemoteForwardHostStopped(activity);
        }
                                                             

        void StopEndpoint(TcpRemoteForwardBridge tcpClientBridge)
        {
            EventTraceActivity epa = new EventTraceActivity(activity);
            try
            {
                BridgeEventSource.Log.RemoteForwardBridgeStopping(epa, tcpClientBridge);
                tcpClientBridge.Close();
                BridgeEventSource.Log.RemoteForwardBridgeStopped(epa, tcpClientBridge);
            }
            catch (Exception exception)
            {
                BridgeEventSource.Log.RemoteForwardBridgeFailedToStop(epa, exception);
                if (Fx.IsFatal(exception))
                {
                    throw;
                }                                                            
            }
        }

        internal void UpdateConfig(Config config)
        {
            BridgeEventSource.Log.RemoteForwardConfigUpdating(activity, config, this.config);
            this.config = config;

            // stopping the listeners will actually not cut existing
            // connections.
            StopEndpoints();
            StartEndpoints(config.RemoteForward);

            BridgeEventSource.Log.RemoteForwardConfigUpdated(activity);
        }

        void StartEndpoint(RemoteForward remoteForward)
        {
            Uri hybridConnectionUri = null;
            TcpRemoteForwardBridge tcpRemoteForwardBridge = null;
            EventTraceActivity epa = new EventTraceActivity(activity);

            BridgeEventSource.Log.RemoteForwardBridgeStarting(epa, this, remoteForward);

            var rcbs = remoteForward.RelayConnectionStringBuilder ?? new RelayConnectionStringBuilder(this.config.AzureRelayConnectionString);
            rcbs.EntityPath = remoteForward.RelayName;
            hybridConnectionUri = new Uri(rcbs.Endpoint, rcbs.EntityPath);

            try
            {
                tcpRemoteForwardBridge = new TcpRemoteForwardBridge(rcbs,
                    remoteForward.Host, remoteForward.HostPort);
                tcpRemoteForwardBridge.Online += (s,e)=>
                {
                    NotifyOnline(hybridConnectionUri, remoteForward);
                    BridgeEventSource.Log.RemoteForwardBridgeOnline(epa, hybridConnectionUri, tcpRemoteForwardBridge);
                };
                tcpRemoteForwardBridge.Offline += (s, e) =>
                {
                    NotifyOffline(hybridConnectionUri, remoteForward);
                    BridgeEventSource.Log.RemoteForwardBridgeOffline(epa, hybridConnectionUri, tcpRemoteForwardBridge);
                };
                tcpRemoteForwardBridge.Connecting += (s, e) =>
                {
                    NotifyConnecting(hybridConnectionUri, remoteForward);
                    BridgeEventSource.Log.RemoteForwardBridgeConnecting(epa, hybridConnectionUri, tcpRemoteForwardBridge);
                };
                tcpRemoteForwardBridge.Open().Wait();

                this.clientBridges.Add(hybridConnectionUri.AbsoluteUri, tcpRemoteForwardBridge);

                BridgeEventSource.Log.RemoteForwardBridgeStarted(epa, hybridConnectionUri.AbsoluteUri);
            }
            catch (Exception exception)
            {
                BridgeEventSource.Log.RemoteForwardBridgeFailedToStart(epa, hybridConnectionUri, exception);
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                try
                {
                    if (tcpRemoteForwardBridge != null)
                    {
                        tcpRemoteForwardBridge.Dispose();
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    BridgeEventSource.Log.HandledExceptionAsWarning(this, e);
                }

                if ( !this.config.ExitOnForwardFailure.HasValue ||
                     this.config.ExitOnForwardFailure.Value)
                {
                    throw;
                }
            }
        }

        private void NotifyConnecting(Uri hybridConnectionUri, RemoteForward remoteForward)
        {
            throw new NotImplementedException();
        }

        private void NotifyOffline(Uri hybridConnectionUri, RemoteForward remoteForward)
        {
            throw new NotImplementedException();
        }

        private void NotifyOnline(Uri hybridConnectionUri, RemoteForward remoteForward)
        {
            throw new NotImplementedException();
        }

        void StartEndpoints(IEnumerable<RemoteForward> tcpClientSettings)
        {
            foreach (var tcpClientSetting in tcpClientSettings)
            {
                this.StartEndpoint(tcpClientSetting);
            }
        }

        void StopEndpoints()
        {
            foreach (var bridge in this.clientBridges.Values)
            {
                StopEndpoint(bridge);
            }
        }
    }
}