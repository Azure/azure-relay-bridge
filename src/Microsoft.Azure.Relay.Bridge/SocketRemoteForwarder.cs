// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NETFRAMEWORK
namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.Azure.Relay;

    sealed class SocketRemoteForwarder : IRemoteForwarder
    {
        public string PortName { get; }
        readonly string targetServer;
                                                                                                  
        internal SocketRemoteForwarder(string portName, string targetSocket)
        {
            PortName = portName;
            this.targetServer = targetSocket;
        }

        public async Task HandleConnectionAsync(Microsoft.Azure.Relay.HybridConnectionStream hybridConnectionStream)
        {
            using (Socket socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP))
            {
                socket.SendBufferSize = socket.ReceiveBufferSize = 65536;
                socket.SendTimeout = 60000;
                await socket.ConnectAsync(new UnixDomainSocketEndPoint(targetServer));
                var tcpstream = new NetworkStream(socket);

                CancellationTokenSource socketAbort = new CancellationTokenSource();
                await Task.WhenAll(
                        StreamPump.RunAsync(hybridConnectionStream, tcpstream,
                                () => socket.Shutdown(SocketShutdown.Send), socketAbort.Token)
                            .ContinueWith((t) => socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted),
                        StreamPump.RunAsync(tcpstream, hybridConnectionStream, () => hybridConnectionStream.Shutdown(),
                            socketAbort.Token))
                    .ContinueWith((t) => socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }
}
#endif