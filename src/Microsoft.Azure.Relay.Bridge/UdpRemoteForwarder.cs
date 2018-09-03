// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.Azure.Relay;

    sealed class UdpRemoteForwarder : IRemoteForwarder
    {
        readonly Config config;
        readonly int targetPort;
        readonly string targetServer;
        static Random rnd = new Random();
        TimeSpan maxIdleTime = TimeSpan.FromMinutes(4);

        internal UdpRemoteForwarder(Config config, string portName, string targetServer, int targetPort)
        {
            this.config = config;
            this.PortName = portName;
            this.targetServer = targetServer;
            this.targetPort = targetPort;
        }

        public string PortName { get; }

        public async Task HandleConnectionAsync(HybridConnectionStream hybridConnectionStream)
        {
            UdpClient client = null;

            if (config.BindAddress != null)
            {
                var computerProperties = IPGlobalProperties.GetIPGlobalProperties();
                var unicastAddresses = computerProperties.GetUnicastAddresses();
                IList<IPAddress> ipAddresses = null;

                ipAddresses = IPAddress.TryParse(config.BindAddress, out var ipAddress)
                    ? new[] { ipAddress }
                    : Dns.GetHostEntry(config.BindAddress).AddressList;

                List<IPAddress> eligibleAddresses = new List<IPAddress>();
                eligibleAddresses.AddRange(from hostAddress in ipAddresses
                    where IPAddress.IsLoopback(hostAddress)
                    select hostAddress);
                eligibleAddresses.AddRange(from unicastAddress in unicastAddresses
                    join hostAddress in ipAddresses on unicastAddress.Address equals hostAddress
                    where !IPAddress.IsLoopback(hostAddress)
                    select hostAddress);

                // pick one of those eligible endpoints
                client = new UdpClient(new IPEndPoint(eligibleAddresses[rnd.Next(eligibleAddresses.Count)], 0));
            }
            else
            {
                client = new UdpClient();
            }

            using (client)
            {
                client.Connect(targetServer, targetPort);

                CancellationTokenSource socketAbort = new CancellationTokenSource(maxIdleTime);
                await Task.WhenAll(
                    Send(hybridConnectionStream, client, socketAbort)
                        .ContinueWith((t) => socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted),
                    Receive(hybridConnectionStream, client, socketAbort)
                        .ContinueWith((t) => socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted));
            }
        }

        async Task Send(Stream source, UdpClient target, CancellationTokenSource cancellationToken)
        {
            try
            {
                byte[] length = new byte[2];
                byte[] buffer = new byte[65536];

                do
                {
                    int read = 0;
                    while (read < 2)
                    {
                        var r = await source.ReadAsync(length, 0, 2, cancellationToken.Token);
                        if (r == 0)
                        {
                            return;
                        }
                        read += r;
                    }

                    cancellationToken.CancelAfter(maxIdleTime);

                    read = 0;
                    int toRead = length[0] * 256 + length[1];
                    while (read < toRead)
                    {
                        var r = await source.ReadAsync(buffer, 0, toRead, cancellationToken.Token);
                        if (r == 0)
                        {
                            return;
                        }
                        read += r;
                    }
                    await target.SendAsync(buffer, toRead);
                }
                while (!cancellationToken.IsCancellationRequested);
            }
            catch (Exception e)
            {
                BridgeEventSource.Log.HandledExceptionAsWarning(source, e);
                throw;
            }
        }

        async Task Receive(Stream source, UdpClient target, CancellationTokenSource cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var datagram = await target.ReceiveAsync();
                    cancellationToken.CancelAfter(maxIdleTime);
                    var buffer = new byte[datagram.Buffer.Length + 2];
                    using (var ms = new MemoryStream(buffer))
                    {
                        ms.Write(new[] { (byte)(datagram.Buffer.Length / 256), (byte)(datagram.Buffer.Length % 256) }, 0, 2);
                        ms.Write(datagram.Buffer, 0, datagram.Buffer.Length);
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await source.WriteAsync(buffer, 0, (int)ms.Length, cancellationToken.Token);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                BridgeEventSource.Log.HandledExceptionAsWarning(source, e);
                throw;
            }
        }
    }
}