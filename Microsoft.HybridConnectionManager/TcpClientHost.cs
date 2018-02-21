// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using Microsoft.Azure.Relay;


    class TcpClientHost
    {
        readonly Dictionary<string, TcpClientBridge> clientBridges =
            new Dictionary<string, TcpClientBridge>();
        private readonly TcpClientSettingsCollection connectionInfo;

        public TcpClientHost(TcpClientSettingsCollection connectionInfo)
        {
            this.connectionInfo = connectionInfo;
        }

        public void Start(string[] args)
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
        void StartEndpoint(TcpClientSetting hybridConnectionInfo)
        {
            RelayConnectionStringBuilder cb = new RelayConnectionStringBuilder(hybridConnectionInfo.RelayConnectionString);
            Uri hybridConnectionUri = null;
            TcpClientBridge tcpClientBridge = null;

            try
            {
                tcpClientBridge = new TcpClientBridge(hybridConnectionInfo.RelayConnectionString,
                    hybridConnectionInfo.TargetHostName, hybridConnectionInfo.TargetPort);
                tcpClientBridge.Open().Wait();

                this.clientBridges.Add(hybridConnectionInfo.Key, tcpClientBridge);

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

        void StartEndpoints(TcpClientSettingsCollection tcpClientSettings)
        {
            foreach (var tcpClientSetting in tcpClientSettings.Values)
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