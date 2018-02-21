// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Relay;

    public sealed class TcpListenerBridge : IDisposable
    {
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        readonly HybridConnectionClient hybridConnectionClient;

        Task<Task> acceptSocketLoop;

        TcpListener tcpListener;

        public TcpListenerBridge(string connectionString)
        {
            this.hybridConnectionClient = new HybridConnectionClient(connectionString);
        }

        public event EventHandler NotifyException;

        public DateTime LastAttempt { get; private set; }

        public Exception LastError { get; private set; }

        internal bool IsOpen { get; private set; }

        public static TcpListenerBridge FromConnectionString(string connectionString)
        {
            return new TcpListenerBridge(connectionString);
        }

        public void Close()
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException();
            }

            this.IsOpen = false;
            this.cancellationTokenSource.Cancel();
            this.tcpListener?.Stop();
            this.tcpListener = null;
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
                throw new InvalidOperationException();
            }

            try
            {
                this.IsOpen = true;
                this.tcpListener = new TcpListener(listenEndpoint);
                this.tcpListener.Start();
                this.acceptSocketLoop = Task.Factory.StartNew(AcceptSocketLoopAsync);
                this.acceptSocketLoop.ContinueWith(AcceptSocketLoopFaulted, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception exception)
            {
                EventSource.Log.HandledExceptionAsWarning(this, exception);
                this.LastError = exception;
                throw;
            }
        }

        async Task AcceptSocketLoopAsync()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var socket = await this.tcpListener.AcceptTcpClientAsync();
                    this.LastAttempt = DateTime.Now;
                    try
                    {
                        BridgeSocketConnectionAsync(socket).Fork(this);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        EventSource.Log.HandledExceptionAsWarning(this, e);
                        this.LastError = e;
                        this.NotifyException?.Invoke(this, EventArgs.Empty);
                    }
                }
                catch (Exception e)
                {
                    EventSource.Log.HandledExceptionAsWarning(this, e);
                    this.LastError = e;
                    this.NotifyException?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        void AcceptSocketLoopFaulted(Task<Task> t)
        {
            this.LastError = t.Exception;
            this.NotifyException?.Invoke(this, EventArgs.Empty);
            this.Close();
        }

        async Task BridgeSocketConnectionAsync(TcpClient tcpClient)
        {
            try
            {
                var hybridConnectionStream = await hybridConnectionClient.CreateConnectionAsync();
                await Task.WhenAll(
                    Util.StreamPumpAsync(hybridConnectionStream, tcpClient.GetStream(), cancellationTokenSource.Token),
                    Util.StreamPumpAsync(tcpClient.GetStream(), hybridConnectionStream, cancellationTokenSource.Token));
            }
            catch (Exception e)
            {
                EventSource.Log.HandledExceptionAsWarning(this, e);
                throw;
            }
        }
    }
}