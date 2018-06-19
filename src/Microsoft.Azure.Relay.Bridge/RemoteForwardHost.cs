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
        private EventTraceActivity activity = BridgeEventSource.NewActivity("RemoteForwardHost");

        public RemoteForwardHost(Config config)
        {
            this.config = config;
        }

        public void Start()
        {
            try
            {
                this.activity.DiagnosticsActivity.Start();
                BridgeEventSource.Log.RemoteForwardHostStarting(this.activity);
                StartEndpoints(config.RemoteForward);
                BridgeEventSource.Log.RemoteForwardHostStart(this.activity);
            }
            catch (Exception exception)
            {
                BridgeEventSource.Log.RemoteForwardHostStartFailure(activity, exception);
                this.activity.DiagnosticsActivity.Stop();
            }
        }

        public void Stop()
        {
            try
            {
                BridgeEventSource.Log.RemoteForwardHostStopping(activity);
                this.StopEndpoints();
                BridgeEventSource.Log.RemoteForwardHostStop(activity);
            }
            finally
            {
                this.activity.DiagnosticsActivity.Stop();
            }                                                        
        }
                                                             

        void StopEndpoint(TcpRemoteForwardBridge tcpClientBridge)
        {
            EventTraceActivity stopActivity = BridgeEventSource.NewActivity("RemoteForwardBridgeStop", activity);
            try
            {
                BridgeEventSource.Log.RemoteForwardBridgeStopping(stopActivity, tcpClientBridge);
                tcpClientBridge.Close();
                BridgeEventSource.Log.RemoteForwardBridgeStop(stopActivity, tcpClientBridge);
            }
            catch (Exception exception)
            {
                BridgeEventSource.Log.RemoteForwardBridgeStopFailure(stopActivity, exception);
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
            EventTraceActivity stopActivity = BridgeEventSource.NewActivity("RemoteForwardBridgeStart", activity);
            stopActivity.DiagnosticsActivity.Start();

            try
            {
                BridgeEventSource.Log.RemoteForwardBridgeStarting(stopActivity, this, remoteForward);

                var rcbs = remoteForward.RelayConnectionStringBuilder ?? new RelayConnectionStringBuilder(this.config.AzureRelayConnectionString);
                rcbs.EntityPath = remoteForward.RelayName;
                hybridConnectionUri = new Uri(rcbs.Endpoint, rcbs.EntityPath);

                try
                {
                    tcpRemoteForwardBridge = new TcpRemoteForwardBridge(rcbs,
                        remoteForward.Host, remoteForward.HostPort);
                    tcpRemoteForwardBridge.Online += (s, e) =>
                    {
                        NotifyOnline(hybridConnectionUri, remoteForward);
                        BridgeEventSource.Log.RemoteForwardBridgeOnline(stopActivity, hybridConnectionUri, tcpRemoteForwardBridge);
                    };
                    tcpRemoteForwardBridge.Offline += (s, e) =>
                    {
                        NotifyOffline(hybridConnectionUri, remoteForward);
                        BridgeEventSource.Log.RemoteForwardBridgeOffline(stopActivity, hybridConnectionUri, tcpRemoteForwardBridge);
                    };
                    tcpRemoteForwardBridge.Connecting += (s, e) =>
                    {
                        NotifyConnecting(hybridConnectionUri, remoteForward);
                        BridgeEventSource.Log.RemoteForwardBridgeConnecting(stopActivity, hybridConnectionUri, tcpRemoteForwardBridge);
                    };
                    tcpRemoteForwardBridge.Open().Wait();

                    this.clientBridges.Add(hybridConnectionUri.AbsoluteUri, tcpRemoteForwardBridge);

                    BridgeEventSource.Log.RemoteForwardBridgeStart(stopActivity, hybridConnectionUri.AbsoluteUri);
                }
                catch (Exception exception)
                {
                    BridgeEventSource.Log.RemoteForwardBridgeStartFailure(stopActivity, hybridConnectionUri, exception);
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

                    if (!this.config.ExitOnForwardFailure.HasValue ||
                         this.config.ExitOnForwardFailure.Value)
                    {
                        throw;
                    }
                }
            }
            finally
            {
                stopActivity.DiagnosticsActivity.Stop();
            }
        }

        private void NotifyConnecting(Uri hybridConnectionUri, RemoteForward remoteForward)
        {
            // this hook will write to a named pipe
        }

        private void NotifyOffline(Uri hybridConnectionUri, RemoteForward remoteForward)
        {
            // this hook will write to a named pipe
        }

        private void NotifyOnline(Uri hybridConnectionUri, RemoteForward remoteForward)
        {
            // this hook will write to a named pipe
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