// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.Azure.Relay;

    sealed class TcpLocalForwardBridge : IDisposable
    {
        public string PortName { get; }

        private readonly Config config;
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        readonly HybridConnectionClient hybridConnectionClient;
        private EventTraceActivity listenerActivity;
        Task<Task> acceptSocketLoop;

        TcpListener tcpListener;
        string localEndpoint;

        public TcpLocalForwardBridge(Config config, RelayConnectionStringBuilder connectionString, string portName)
        {
            PortName = portName;
            this.config = config;
            this.hybridConnectionClient = new HybridConnectionClient(connectionString.ToString());
        }

        public event EventHandler NotifyException;

        public DateTime LastAttempt { get; private set; }

        public Exception LastError { get; private set; }

        internal bool IsOpen { get; private set; }

        public HybridConnectionClient HybridConnectionClient => hybridConnectionClient;

        public static TcpLocalForwardBridge FromConnectionString(Config config,
            RelayConnectionStringBuilder connectionString, string portName)
        {
            return new TcpLocalForwardBridge(config, connectionString, portName);
        }

        public void Close()
        {
            BridgeEventSource.Log.LocalForwardListenerStopping(listenerActivity, localEndpoint);

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
            }
            BridgeEventSource.Log.LocalForwardListenerStop(listenerActivity, localEndpoint);
        }

        public void Dispose()
        {
            this.Close();
        }

        public string GetIpEndPointInfo()
        {
            return localEndpoint;
        }

        public void Run(IPEndPoint listenEndpoint)
        {
            this.localEndpoint = listenEndpoint.ToString();

            if (this.IsOpen)
            {
                throw BridgeEventSource.Log.ThrowingException(new InvalidOperationException(), this);
            }

            this.listenerActivity = BridgeEventSource.NewActivity("LocalForwardListener");

            try
            {
                this.IsOpen = true;
                BridgeEventSource.Log.LocalForwardListenerStarting(listenerActivity, localEndpoint);
                this.tcpListener = new TcpListener(listenEndpoint);
                this.tcpListener.Start();
                this.acceptSocketLoop = Task.Factory.StartNew(AcceptSocketLoopAsync);
                this.acceptSocketLoop.ContinueWith(AcceptSocketLoopFaulted, TaskContinuationOptions.OnlyOnFaulted);
                BridgeEventSource.Log.LocalForwardListenerStart(listenerActivity, localEndpoint);
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
                var socketActivity = BridgeEventSource.NewActivity("LocalForwardSocket");
                TcpClient socket;

                try
                {
                    socket = await this.tcpListener.AcceptTcpClientAsync();
                }
                catch (ObjectDisposedException)
                {
                    // occurs on shutdown and signals that we need to exit
                    return;
                }

                BridgeEventSource.Log.LocalForwardSocketAccepted(socketActivity, localEndpoint);

                this.LastAttempt = DateTime.Now;

                BridgeSocketConnectionAsync(socket)
                    .ContinueWith((t, s) =>
                    {
                        if (t.Exception != null)
                        {
                            BridgeEventSource.Log.LocalForwardSocketError(socketActivity, localEndpoint, t.Exception);
                        }
                        socket.Dispose();
                    }, TaskContinuationOptions.OnlyOnFaulted)
                    .ContinueWith((t, s) =>
                    {

                        try
                        {
                            BridgeEventSource.Log.LocalForwardSocketComplete(socketActivity, localEndpoint);
                            socket.Close();
                            BridgeEventSource.Log.LocalForwardSocketClosed(socketActivity, localEndpoint);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                            BridgeEventSource.Log.LocalForwardSocketCloseFailed(socketActivity, localEndpoint, e);
                            socket.Dispose();
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion)
                    .Fork();
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
            EventTraceActivity bridgeActivity = BridgeEventSource.NewActivity("LocalForwardBridgeConnection");
            try
            {
                BridgeEventSource.Log.LocalForwardBridgeConnectionStarting(bridgeActivity, localEndpoint, HybridConnectionClient);

                tcpClient.SendBufferSize = tcpClient.ReceiveBufferSize = 65536;
                tcpClient.SendTimeout = 60000;
                var tcpstream = tcpClient.GetStream();
                var socket = tcpClient.Client;
                
                using (var hybridConnectionStream = await HybridConnectionClient.CreateConnectionAsync())
                {
                    // read and write version preamble
                    hybridConnectionStream.WriteTimeout = 60000;

                    // write the 1.0 header with the portname for this connection
                    byte[] portNameBytes = Encoding.UTF8.GetBytes(PortName);
                    byte[] preamble =
                    {
                        /*major*/ 1, 
                        /*minor*/ 0, 
                        /*stream mode*/ 0,
                        (byte)portNameBytes.Length
                    };
                    await hybridConnectionStream.WriteAsync(preamble, 0, preamble.Length);
                    await hybridConnectionStream.WriteAsync(portNameBytes, 0, portNameBytes.Length);
                    
                    byte[] replyPreamble = new byte[3];
                    for (int read = 0; read < replyPreamble.Length;)
                    {
                        var r = await hybridConnectionStream.ReadAsync(replyPreamble, read,
                            replyPreamble.Length - read);
                        if (r == 0)
                        {
                            await hybridConnectionStream.ShutdownAsync(CancellationToken.None);
                            await hybridConnectionStream.CloseAsync(CancellationToken.None);
                            return;
                        }
                        read += r;
                    }

                    if (!(replyPreamble[0] == 1 && replyPreamble[1] == 0 && replyPreamble[2] == 0)) 
                    {
                        // version not supported
                        await hybridConnectionStream.ShutdownAsync(CancellationToken.None);
                        await hybridConnectionStream.CloseAsync(CancellationToken.None);
                        return;
                    }


                    BridgeEventSource.Log.LocalForwardBridgeConnectionStart(bridgeActivity, localEndpoint, HybridConnectionClient);

                    try
                    {
                        CancellationTokenSource socketAbort = new CancellationTokenSource();
                        await Task.WhenAll(
                            StreamPump.RunAsync(hybridConnectionStream, tcpstream,
                                () => socket.Shutdown(SocketShutdown.Send), socketAbort.Token)
                                .ContinueWith((t)=>socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted),
                            StreamPump.RunAsync(tcpstream, hybridConnectionStream,
                                () => hybridConnectionStream?.Shutdown(), socketAbort.Token))
                                .ContinueWith((t) => socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted);

                        using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1)))
                        {
                            await hybridConnectionStream.CloseAsync(cts.Token);
                        }
                    }
                    catch
                    {
                        if (socket.Connected)
                        {
                            socket.Close(0);
                        }
                        throw;
                    }
                }
                BridgeEventSource.Log.LocalForwardBridgeConnectionStop(bridgeActivity, localEndpoint, HybridConnectionClient);
            }
            catch (Exception e)
            {
                BridgeEventSource.Log.LocalForwardBridgeConnectionFailed(bridgeActivity, e);
            }
        }
    }
}