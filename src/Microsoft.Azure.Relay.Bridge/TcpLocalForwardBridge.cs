// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Relay;

    sealed class TcpLocalForwardBridge : IDisposable
    {
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        readonly HybridConnectionClient hybridConnectionClient;
        private EventTraceActivity listenerActivity;
        Task<Task> acceptSocketLoop;

        TcpListener tcpListener;

        public TcpLocalForwardBridge(RelayConnectionStringBuilder connectionString)
        {
            this.hybridConnectionClient = new HybridConnectionClient(connectionString.ToString());
        }

        public event EventHandler NotifyException;

        public DateTime LastAttempt { get; private set; }

        public Exception LastError { get; private set; }

        internal bool IsOpen { get; private set; }

        public HybridConnectionClient HybridConnectionClient => hybridConnectionClient;

        public static TcpLocalForwardBridge FromConnectionString(RelayConnectionStringBuilder connectionString)
        {
            return new TcpLocalForwardBridge(connectionString);
        }

        public void Close()
        {
            BridgeEventSource.Log.LocalForwardListenerStopping(listenerActivity, tcpListener);

            try
            {
                if (!this.IsOpen)
                {
                    throw BridgeEventSource.Log.ThrowingException(new InvalidOperationException(), this);
                }   
                this.IsOpen = false;
                this.cancellationTokenSource.Cancel();
                this.tcpListener?.Stop();
                this.tcpListener = null;
            }
            catch (Exception ex)
            {
                BridgeEventSource.Log.LocalForwardListenerStoppingFailed(listenerActivity, ex);
                throw;
            }
            BridgeEventSource.Log.LocalForwardListenerStopped(listenerActivity, tcpListener);
        }

        public void Dispose()
        {
            this.Close();
        }

        public IPEndPoint GetIpEndPoint()
        {
            return this.tcpListener?.LocalEndpoint as IPEndPoint;
        }

        public void Run(IPEndPoint listenEndpoint)
        {
            if (this.IsOpen)
            {
                throw BridgeEventSource.Log.ThrowingException(new InvalidOperationException(),this);
            }

            this.listenerActivity = new EventTraceActivity();

            try
            {
                this.IsOpen = true;
                BridgeEventSource.Log.LocalForwardListenerStarting(listenerActivity, listenEndpoint);
                this.tcpListener = new TcpListener(listenEndpoint);
                this.tcpListener.Start();
                this.acceptSocketLoop = Task.Factory.StartNew(AcceptSocketLoopAsync);
                this.acceptSocketLoop.ContinueWith(AcceptSocketLoopFaulted, TaskContinuationOptions.OnlyOnFaulted);
                BridgeEventSource.Log.LocalForwardListenerStarted(listenerActivity, tcpListener);
            }
            catch (Exception exception)
            {
                BridgeEventSource.Log.LocalForwardListenerStartFailed(listenerActivity, exception);
                this.LastError = exception;
                throw;
            }
        }

        async Task AcceptSocketLoopAsync()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                var socketActivity = new EventTraceActivity();
                var socket = await this.tcpListener.AcceptTcpClientAsync();
                BridgeEventSource.Log.LocalForwardSocketAccepted(socketActivity, socket);

                this.LastAttempt = DateTime.Now;

                BridgeSocketConnectionAsync(socket)
                    .ContinueWith((t, s) =>
                    {
                        BridgeEventSource.Log.LocalForwardSocketError(socketActivity, socket, t.Exception);
                        socket.Dispose();
                    }, TaskContinuationOptions.OnlyOnFaulted)
                    .ContinueWith((t, s) =>
                    {

                        try
                        {
                            BridgeEventSource.Log.LocalForwardSocketComplete(socketActivity, socket);
                            socket.Close();
                            BridgeEventSource.Log.LocalForwardSocketClosed(socketActivity, socket);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                            BridgeEventSource.Log.LocalForwardSocketCloseFailed(socketActivity, socket, e);
                            socket.Dispose();
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion)
                    .Fork();                                                            \
            }
        }

        void AcceptSocketLoopFaulted(Task<Task> t)
        {
            BridgeEventSource.Log.LocalForwardSocketAcceptLoopFailed(listenerActivity, t.Exception);
            this.LastError = t.Exception;
            this.NotifyException?.Invoke(this, EventArgs.Empty);
            this.Close();
        }

        async Task BridgeSocketConnectionAsync(TcpClient tcpClient)
        {
            EventTraceActivity bridgeActivity = new EventTraceActivity();
            try
            {   
                BridgeEventSource.Log.LocalForwardBridgeConnectionStarting(bridgeActivity, tcpClient, HybridConnectionClient);
                using (var hybridConnectionStream = await HybridConnectionClient.CreateConnectionAsync())
                {
                    BridgeEventSource.Log.LocalForwardBridgeConnectionStarted(bridgeActivity, tcpClient, HybridConnectionClient);
                    await Task.WhenAll(
                        StreamPump.RunAsync(hybridConnectionStream, tcpClient.GetStream(), cancellationTokenSource.Token),
                        StreamPump.RunAsync(tcpClient.GetStream(), hybridConnectionStream, cancellationTokenSource.Token));
                }
                BridgeEventSource.Log.LocalForwardBridgeConnectionStopped(bridgeActivity, tcpClient, HybridConnectionClient);
            }
            catch (Exception e)
            {
                BridgeEventSource.Log.LocalForwardBridgeConnectionFailed(bridgeActivity, e);
                throw;
            }
        }
    }
}