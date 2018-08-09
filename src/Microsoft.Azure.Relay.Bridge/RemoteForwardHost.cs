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
        readonly Dictionary<string, TcpRemoteForwardBridge> tcpClientBridges =
            new Dictionary<string, TcpRemoteForwardBridge>();
#if !NETFRAMEWORK
        readonly Dictionary<string, SocketRemoteForwardBridge> socketClientBridges =
            new Dictionary<string, SocketRemoteForwardBridge>();
#endif
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
                BridgeEventSource.Log.RemoteForwardBridgeStopping(stopActivity, tcpClientBridge.ToString());
                tcpClientBridge.Close();
                BridgeEventSource.Log.RemoteForwardBridgeStop(stopActivity, tcpClientBridge.ToString());
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

#if !NETFRAMEWORK
        void StopEndpoint(SocketRemoteForwardBridge socketClientBridge)
        {
            EventTraceActivity stopActivity = BridgeEventSource.NewActivity("RemoteForwardBridgeStop", activity);
            try
            {
                BridgeEventSource.Log.RemoteForwardBridgeStopping(stopActivity, socketClientBridge.ToString());
                socketClientBridge.Close();
                BridgeEventSource.Log.RemoteForwardBridgeStop(stopActivity, socketClientBridge.ToString());
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
#endif

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

#if !NETFRAMEWORK
                if (!string.IsNullOrEmpty(remoteForward.LocalSocket))
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        BridgeEventSource.Log.ThrowingException(
                            new NotSupportedException("Unix sockets are not supported on Windows"));
                    }

                    SocketRemoteForwardBridge socketRemoteForwardBridge = null;

                    try
                    {
                        socketRemoteForwardBridge = new SocketRemoteForwardBridge(config, rcbs,
                            remoteForward.LocalSocket);
                        socketRemoteForwardBridge.Online += (s, e) =>
                        {
                            NotifyOnline(hybridConnectionUri, remoteForward);
                            BridgeEventSource.Log.RemoteForwardBridgeOnline(stopActivity, hybridConnectionUri, socketRemoteForwardBridge);
                        };
                        socketRemoteForwardBridge.Offline += (s, e) =>
                        {
                            NotifyOffline(hybridConnectionUri, remoteForward);
                            BridgeEventSource.Log.RemoteForwardBridgeOffline(stopActivity, hybridConnectionUri, socketRemoteForwardBridge);
                        };
                        socketRemoteForwardBridge.Connecting += (s, e) =>
                        {
                            NotifyConnecting(hybridConnectionUri, remoteForward);
                            BridgeEventSource.Log.RemoteForwardBridgeConnecting(stopActivity, hybridConnectionUri, socketRemoteForwardBridge);
                        };
                        socketRemoteForwardBridge.Open().Wait();

                        this.socketClientBridges.Add(hybridConnectionUri.AbsoluteUri, socketRemoteForwardBridge);

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
                            if (socketRemoteForwardBridge != null)
                            {
                                socketRemoteForwardBridge.Dispose();
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
                    finally
                    {
                        stopActivity.DiagnosticsActivity.Stop();
                    }
                    return;
                }
#endif

                TcpRemoteForwardBridge tcpRemoteForwardBridge = null;

                try
                {
                    tcpRemoteForwardBridge = new TcpRemoteForwardBridge(config, rcbs,
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

                    this.tcpClientBridges.Add(hybridConnectionUri.AbsoluteUri, tcpRemoteForwardBridge);

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

        void StartEndpoints(IEnumerable<RemoteForward> remoteForwards)
        {
            foreach (var remoteForward in remoteForwards)
            {
                this.StartEndpoint(remoteForward);
            }
        }

        void StopEndpoints()
        {
            foreach (var bridge in this.tcpClientBridges.Values)
            {
                StopEndpoint(bridge);
            }
#if !NETFRAMEWORK
            foreach (var bridge in this.socketClientBridges.Values)
            {
                StopEndpoint(bridge);
            }
#endif
        }
    }
}