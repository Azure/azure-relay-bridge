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
        readonly Dictionary<string, RemoteForwardBridge> forwardBridges = new Dictionary<string, RemoteForwardBridge>();
        Config config;
        EventTraceActivity activity = BridgeEventSource.NewActivity("RemoteForwardHost");

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


        void StopEndpoint(RemoteForwardBridge forwardBridge)
        {
            EventTraceActivity stopActivity = BridgeEventSource.NewActivity("RemoteForwardBridgeStop", activity);
            try
            {
                BridgeEventSource.Log.RemoteForwardBridgeStopping(stopActivity, forwardBridge.ToString());
                forwardBridge.Close();
                BridgeEventSource.Log.RemoteForwardBridgeStop(stopActivity, forwardBridge.ToString());
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
            
            EventTraceActivity stopActivity = BridgeEventSource.NewActivity("RemoteForwardBridgeStart", activity);
            stopActivity.DiagnosticsActivity.Start();

            try
            {
                BridgeEventSource.Log.RemoteForwardBridgeStarting(stopActivity, this, remoteForward);

                var rcbs = remoteForward.RelayConnectionStringBuilder ?? new RelayConnectionStringBuilder(this.config.AzureRelayConnectionString);
                rcbs.EntityPath = remoteForward.RelayName;
                hybridConnectionUri = new Uri(rcbs.Endpoint, rcbs.EntityPath);

                RemoteForwardBridge remoteForwardBridge = null;

                try
                {

                    var remoteForwarders = new Dictionary<string, IRemoteForwarder>();
                    foreach (var binding in remoteForward.Bindings)
                    {
#if !NETFRAMEWORK
                        if (!string.IsNullOrEmpty(binding.LocalSocket))
                        {
                            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            {
                                BridgeEventSource.Log.ThrowingException(
                                    new NotSupportedException("Unix sockets are not supported on Windows"));
                            }

                            var socketRemoteForwarder =
                                new SocketRemoteForwarder(binding.PortName, binding.LocalSocket);
                            remoteForwarders.Add(socketRemoteForwarder.PortName, socketRemoteForwarder);
                        }
                        else
#endif
                        if (binding.HostPort > 0)
                        {
                            var tcpRemoteForwarder =
                                new TcpRemoteForwarder(this.config, binding.PortName, binding.Host, binding.HostPort);
                            remoteForwarders.Add(tcpRemoteForwarder.PortName, tcpRemoteForwarder);
                        }
                        else if (binding.HostPort < 0)
                        {
                            var udpRemoteForwarder =
                                new UdpRemoteForwarder(this.config, binding.PortName, binding.Host, -binding.HostPort);
                            remoteForwarders.Add(udpRemoteForwarder.PortName, udpRemoteForwarder);
                        }
                    }

                    remoteForwardBridge = new RemoteForwardBridge(config, rcbs, remoteForwarders);
                    remoteForwardBridge.Online += (s, e) =>
                                {
                                    NotifyOnline(hybridConnectionUri, remoteForward);
                                    BridgeEventSource.Log.RemoteForwardBridgeOnline(stopActivity, hybridConnectionUri, remoteForwardBridge);
                                };
                    remoteForwardBridge.Offline += (s, e) =>
                                    {
                                        NotifyOffline(hybridConnectionUri, remoteForward);
                                        BridgeEventSource.Log.RemoteForwardBridgeOffline(stopActivity, hybridConnectionUri, remoteForwardBridge);
                                    };
                    remoteForwardBridge.Connecting += (s, e) =>
                                        {
                                            NotifyConnecting(hybridConnectionUri, remoteForward);
                                            BridgeEventSource.Log.RemoteForwardBridgeConnecting(stopActivity, hybridConnectionUri, remoteForwardBridge);
                                        };
                    remoteForwardBridge.Open().Wait();

                    this.forwardBridges.Add(hybridConnectionUri.AbsoluteUri, remoteForwardBridge);

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
                        if (remoteForwardBridge != null)
                        {
                            remoteForwardBridge.Dispose();
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

        void StartEndpoints(IEnumerable<RemoteForward> remoteForwards)
        {
            foreach (var remoteForward in remoteForwards)
            {
                this.StartEndpoint(remoteForward);
            }
        }

        void StopEndpoints()
        {
            foreach (var bridge in this.forwardBridges.Values)
            {
                StopEndpoint(bridge);
            }
        }
    }
}