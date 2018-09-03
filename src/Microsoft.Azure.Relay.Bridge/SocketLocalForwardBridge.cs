// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NETFRAMEWORK
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

    sealed class SocketLocalForwardBridge : IDisposable
    {
        public string PortName { get; }

        private readonly Config config;
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        readonly HybridConnectionClient hybridConnectionClient;
        private EventTraceActivity listenerActivity;
        Task<Task> acceptSocketLoop;

        Socket socketListener;
        string localEndpoint;

        public SocketLocalForwardBridge(Config config, RelayConnectionStringBuilder connectionString, string portName)
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

        public static SocketLocalForwardBridge FromConnectionString(Config config,
            RelayConnectionStringBuilder connectionString, string bindingPortName)
        {
            return new SocketLocalForwardBridge(config, connectionString, bindingPortName);
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
                this.socketListener?.Close();
                this.socketListener = null;
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

        public string GetSocketInfo()
        {
            return localEndpoint;
        }

        public void Run(string socketEndpoint)
        {
            this.localEndpoint = socketEndpoint;

            if (this.IsOpen)
            {
                throw BridgeEventSource.Log.ThrowingException(new InvalidOperationException(), this);
            }

            this.listenerActivity = BridgeEventSource.NewActivity("LocalForwardListener");

            try
            {
                this.IsOpen = true;
                BridgeEventSource.Log.LocalForwardListenerStarting(listenerActivity, localEndpoint);
                this.socketListener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                this.socketListener.Bind(new UnixDomainSocketEndPoint(socketEndpoint));
                this.socketListener.Listen(5);
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
                Socket socket;

                try
                {
                    socket = await this.socketListener.AcceptAsync();
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

        async Task BridgeSocketConnectionAsync(Socket socket)
        {
            EventTraceActivity bridgeActivity = BridgeEventSource.NewActivity("LocalForwardBridgeConnection");
            try
            {
                BridgeEventSource.Log.LocalForwardBridgeConnectionStarting(bridgeActivity, localEndpoint, HybridConnectionClient);

                socket.SendBufferSize = socket.ReceiveBufferSize = 65536;
                socket.SendTimeout = 60000;
                var tcpstream = new NetworkStream(socket);
                                
                using (var hybridConnectionStream = await HybridConnectionClient.CreateConnectionAsync())
                {
                    // read and write 4-byte header
                    hybridConnectionStream.WriteTimeout = 60000;

                    // write the 1.0 header with the portname for this connection
                    byte[] portNameBytes = Encoding.UTF8.GetBytes(PortName);
                    byte[] preamble =
                    {
                        /*major*/ 1, 
                        /*minor*/ 0,
                        /*stream */ 0,
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
                            throw new InvalidOperationException($"Malformed preamble from server");
                        }
                        read += r;
                    }

                    if (!(replyPreamble[0] == 1 && replyPreamble[1] == 0 && replyPreamble[2] == 0))
                    {
                        // version not supported
                        await hybridConnectionStream.ShutdownAsync(CancellationToken.None);
                        await hybridConnectionStream.CloseAsync(CancellationToken.None);
                        throw new InvalidOperationException($"Unsupported protocol version: Server reply {replyPreamble[0]} {replyPreamble[1]} {replyPreamble[2]}");
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
#endif