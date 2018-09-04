// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.Azure.Relay;

    sealed class TcpRemoteForwarder : IRemoteForwarder
    {
        readonly Config config;
        readonly int targetPort;
        readonly string targetServer;
        static Random rnd = new Random();

        internal TcpRemoteForwarder(Config config, string portName, string targetServer, int targetPort)
        {
            this.config = config;
            this.PortName = portName;
            this.targetServer = targetServer;
            this.targetPort = targetPort;
        }

        public string PortName { get; }

        public async Task HandleConnectionAsync(HybridConnectionStream hybridConnectionStream)
        {
            using (TcpClient client = new TcpClient())
            {
                client.NoDelay = true;
                client.SendBufferSize = client.ReceiveBufferSize = 65536;
                client.SendTimeout = 60000;
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
                    client.Client.Bind(new IPEndPoint(eligibleAddresses[rnd.Next(eligibleAddresses.Count)], 0));
                }
                await client.ConnectAsync(targetServer, targetPort);
                var tcpstream = client.GetStream();

                CancellationTokenSource socketAbort = new CancellationTokenSource();
                await Task.WhenAll(
                    StreamPump.RunAsync(hybridConnectionStream, tcpstream,
                        () => client.Client.Shutdown(SocketShutdown.Send), socketAbort.Token)
                        .ContinueWith((t) => socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted),
                    StreamPump.RunAsync(tcpstream, hybridConnectionStream, () => hybridConnectionStream.Shutdown(), socketAbort.Token))
                        .ContinueWith((t) => socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted);

            }

        }
    }
}