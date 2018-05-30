// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using Microsoft.Azure.Relay;
    using Microsoft.Azure.Relay.Bridge.Configuration;

    sealed class TcpClientHost
    {
        readonly Dictionary<string, TcpClientBridge> clientBridges =
            new Dictionary<string, TcpClientBridge>();
        private readonly IEnumerable<RemoteForward> connectionInfo;

        public TcpClientHost(IEnumerable<RemoteForward> connectionInfo)
        {
            this.connectionInfo = connectionInfo;
        }

        public void Start()
        {
            try
            {
                EventSource.Log.HybridConnectionManagerStarting();
                StartEndpoints(this.connectionInfo);

            }
            catch (Exception e)
            {
                EventWriteHybridConnectionServiceError(e);
            }
        }

        public void Stop()
        {
            EventSource.Log.HybridConnectionManagerStopping();
            this.StopEndpoints();
        }

        void EventWriteHybridConnectionServiceError(Exception e)
        {
            EventSource.Log.HybridConnectionManagerManagementServerError(null, e.InnerException != null ? e.InnerException.ToString() : e.ToString());
        }

        void StopEndpoint(TcpClientBridge tcpClientBridge)
        {
            try
            {
                tcpClientBridge.Close();

                EventSource.Log.HybridConnectionStopped(null, null);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                EventSource.Log.HybridConnectionFailedToStop(null, null, exception.Message, exception.StackTrace);
            }
        }

        internal void UpdateConfig(List<RemoteForward> targets)
        {
            foreach (var item in targets)
            {
              
            }
        }

        void StartEndpoint(RemoteForward hybridConnectionInfo)
        {
            Uri hybridConnectionUri = null;
            TcpClientBridge tcpClientBridge = null;

            var rcbs = hybridConnectionInfo.ConnectionString;
            hybridConnectionUri = rcbs.Endpoint;

            try
            {
                tcpClientBridge = new TcpClientBridge(hybridConnectionInfo.ConnectionString,
                    hybridConnectionInfo.Host, hybridConnectionInfo.Port);
                tcpClientBridge.Open().Wait();

                this.clientBridges.Add(hybridConnectionUri.AbsoluteUri, tcpClientBridge);

                EventSource.Log.HybridConnectionStarted(null, hybridConnectionUri.AbsoluteUri);
            }
            catch (SecurityException exception)
            {
                EventSource.Log.HybridConnectionSecurityException(null, hybridConnectionUri.AbsoluteUri, exception.ToString());
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                try
                {
                    if (tcpClientBridge != null)
                    {
                        tcpClientBridge.Dispose();
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    EventSource.Log.HandledExceptionAsWarning(this, e);
                }

                //HybridConnectionManagerEventSource.Log.HybridConnectionFailedToStart(activity, hybridConnectionUri.AbsoluteUri, exception.Message, exception.StackTrace);
            }
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