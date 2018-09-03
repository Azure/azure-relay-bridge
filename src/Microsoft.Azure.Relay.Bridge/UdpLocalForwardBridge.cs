// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.Azure.Relay;

    sealed class UdpLocalForwardBridge : IDisposable
    {
        public string PortName { get; }

        private readonly Config config;
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        Dictionary<IPEndPoint, UdpRoute> routes = new Dictionary<IPEndPoint, UdpRoute>();

        readonly HybridConnectionClient hybridConnectionClient;
        private EventTraceActivity listenerActivity;
        Task<Task> receiveDatagramLoop;

        UdpClient udpClient;
        string localEndpoint;

        public UdpLocalForwardBridge(Config config, RelayConnectionStringBuilder connectionString, string portName)
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

        public static UdpLocalForwardBridge FromConnectionString(Config config,
            RelayConnectionStringBuilder connectionString, string portName)
        {
            return new UdpLocalForwardBridge(config, connectionString, portName);
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
                this.udpClient?.Close();
                this.udpClient = null;
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
                this.udpClient = new UdpClient(listenEndpoint);
                this.receiveDatagramLoop = Task.Factory.StartNew(ReceiveDatagramAsync);
                this.receiveDatagramLoop.ContinueWith(ReceiveLoopFaulted, TaskContinuationOptions.OnlyOnFaulted);
                BridgeEventSource.Log.LocalForwardListenerStart(listenerActivity, localEndpoint);
            }
            catch (Exception exception)
            {
                BridgeEventSource.Log.LocalForwardListenerStartFailed(listenerActivity, exception);
                this.LastError = exception;
                throw;
            }
        }

        void ReceiveLoopFaulted(Task<Task> t)
        {
            BridgeEventSource.Log.LocalForwardSocketAcceptLoopFailed(listenerActivity, t.Exception);
            this.LastError = t.Exception;
            this.NotifyException?.Invoke(this, EventArgs.Empty);
            this.Close();
        }

        async Task ReceiveDatagramAsync()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                var socketActivity = BridgeEventSource.NewActivity("LocalForwardSocket");
                UdpReceiveResult datagram;

                try
                {
                    datagram = await this.udpClient.ReceiveAsync();
                }
                catch (ObjectDisposedException)
                {
                    // occurs on shutdown and signals that we need to exit
                    return;
                }

                BridgeEventSource.Log.LocalForwardSocketAccepted(socketActivity, localEndpoint);

                this.LastAttempt = DateTime.Now;

                if (routes.TryGetValue(datagram.RemoteEndPoint, out var route))
                {
                    using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
                    {
                        await route.SendAsync(datagram, ct.Token);
                    }
                }
                else
                {
                    var hybridConnectionStream = await HybridConnectionClient.CreateConnectionAsync();
                    // read and write version preamble
                    hybridConnectionStream.WriteTimeout = 60000;

                    // write the 1.0 header with the portname for this connection
                    byte[] portNameBytes = Encoding.UTF8.GetBytes(PortName);
                    byte[] preamble =
                    {
                        /*major*/ 1, 
                        /*minor*/ 0,
                        /*dgram*/ 1,
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

                    if (!(replyPreamble[0] == 1 && replyPreamble[1] == 0 && replyPreamble[2] == 1))
                    {
                        // version not supported
                        await hybridConnectionStream.ShutdownAsync(CancellationToken.None);
                        await hybridConnectionStream.CloseAsync(CancellationToken.None);
                        return;
                    }

                    var newRoute = new UdpRoute(udpClient, datagram.RemoteEndPoint, hybridConnectionStream,
                        () => { routes.Remove(datagram.RemoteEndPoint); });
                    routes.Add(datagram.RemoteEndPoint, newRoute);
                    newRoute.StartReceiving();
                    using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
                    {
                        await newRoute.SendAsync(datagram, ct.Token);
                    }
                }
            }
        }
    }

    class UdpRoute
    {
        readonly UdpClient client;

        readonly Stream target;

        public IPEndPoint IPEndPoint { get; set; }
        CancellationTokenSource routeCancel;
        TimeSpan maxIdleTime = TimeSpan.FromMinutes(4);

        public UdpRoute(UdpClient client, IPEndPoint ipEndPoint, Stream target, Action routeExpired = null)
        {
            this.client = client;
            this.target = target;
            IPEndPoint = ipEndPoint;
            this.routeCancel = new CancellationTokenSource(maxIdleTime);
            if (routeExpired != null)
            {
                this.routeCancel.Token.Register(routeExpired);
            }
        }

        public async Task SendAsync(UdpReceiveResult datagram, CancellationToken ct)
        {
            // update the route expiry
            this.routeCancel.CancelAfter(maxIdleTime);

            // the send operation timeout is intentionally short and we'll start
            // dropping packets on timeout. The copy operation via MemoryStream
            // is required since we need to force the whole frame out in one write
            // and need a preamble in the stream to delineate the datagrams
            var buffer = new byte[datagram.Buffer.Length + 2];
            using (var ms = new MemoryStream(buffer))
            {
                ms.Write(new[] { (byte)(datagram.Buffer.Length / 256), (byte)(datagram.Buffer.Length % 256) }, 0, 2);
                ms.Write(datagram.Buffer, 0, datagram.Buffer.Length);
                if (!ct.IsCancellationRequested)
                {
                    await target.WriteAsync(buffer, 0, (int)ms.Length, ct);
                    await target.FlushAsync(ct);
                }
            }
        }

        public void StartReceiving()
        {
            Task.Factory.StartNew(async () =>
            {
                byte[] length = new byte[2];
                byte[] buffer = new byte[65536];

                do
                {
                    int read = 0;
                    while (read < 2)
                    {
                        var r = await target.ReadAsync(length, 0, 2, this.routeCancel.Token);
                        if (r == 0)
                        {
                            return;
                        }
                        read += r;
                    }

                    // update the route expiry
                    this.routeCancel.CancelAfter(maxIdleTime);

                    read = 0;
                    int toRead = length[0] * 256 + length[1];
                    while (read < toRead)
                    {
                        var r = await target.ReadAsync(buffer, 0, toRead, this.routeCancel.Token);
                        if (r == 0)
                        {
                            return;
                        }         
                        read += r;
                    }                                                                               
                    await client.SendAsync(buffer, toRead, this.IPEndPoint);
                }
                while (!this.routeCancel.IsCancellationRequested);
            });
        }
    }
}