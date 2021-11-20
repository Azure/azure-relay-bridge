// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.Azure.Relay;

    sealed class TcpRemoteForwarder : IRemoteRequestForwarder
    {
        readonly Config config;
        readonly int targetPort;
        readonly string targetServer;
        static Random rnd = new Random();
        private HttpClient httpClient;
        private string relaySubpath;

        internal TcpRemoteForwarder(Config config, string relayName, string portName, string targetServer, int targetPort, string targetPath, bool http)
        {
            this.config = config;
            this.PortName = portName;
            this.targetServer = targetServer;
            this.targetPort = targetPort;

            
            if ( http )
            {
                this.httpClient = new HttpClient();
                this.httpClient.BaseAddress = new UriBuilder(portName, targetServer, targetPort, targetPath).Uri;
                this.httpClient.DefaultRequestHeaders.ExpectContinue = false;
                this.relaySubpath = "/" + relayName;
            }
        }

        public string PortName { get; }

        public async Task HandleConnectionAsync(HybridConnectionStream hybridConnectionStream)
        {
            EventTraceActivity handleConnectionActivity = BridgeEventSource.NewActivity("HandleTcpConnection");

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
                await client.ConnectAsync(targetServer, targetPort).ConfigureAwait(false);
                var tcpstream = client.GetStream();

                BridgeEventSource.Log.RemoteForwardTcpSocketAccepted(handleConnectionActivity, $"{targetServer}:{targetPort}");

                CancellationTokenSource socketAbort = new CancellationTokenSource();
                await Task.WhenAll(
                    StreamPump.RunAsync(hybridConnectionStream, tcpstream,
                        () => client.Client.Shutdown(SocketShutdown.Send), socketAbort.Token)
                        .ContinueWith((t) => socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted),
                    StreamPump.RunAsync(tcpstream, hybridConnectionStream, () => hybridConnectionStream.Shutdown(), socketAbort.Token))
                        .ContinueWith((t) => socketAbort.Cancel(), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

            }

        }

       
        public async Task HandleRequest(RelayedHttpListenerContext ctx)
        {
            EventTraceActivity handleRequestActivity = BridgeEventSource.NewActivity("HandleRequest");
            
            DateTime startTimeUtc = DateTime.UtcNow;
            try
            {
                HttpRequestMessage requestMessage = CreateHttpRequestMessage(ctx);
                HttpResponseMessage responseMessage = await this.httpClient.SendAsync(requestMessage);
                await SendResponseAsync(ctx, responseMessage);
                await ctx.Response.CloseAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.GetType().Name}: {e.Message}");
                SendErrorResponse(e, ctx);
            }
            finally
            {
                DateTime stopTimeUtc = DateTime.UtcNow;
                StringBuilder buffer = new StringBuilder();
                buffer.Append($"{startTimeUtc.ToString("s", CultureInfo.InvariantCulture)}, ");
                buffer.Append($"\"{ctx.Request.HttpMethod} {ctx.Request.Url.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped)}\", ");
                buffer.Append($"{(int)ctx.Response.StatusCode}, ");
                buffer.Append($"{(int)stopTimeUtc.Subtract(startTimeUtc).TotalMilliseconds}");
                BridgeEventSource.Log.RemoteForwardHttpRequestForwarded(handleRequestActivity, buffer.ToString());
            }
        }

        HttpRequestMessage CreateHttpRequestMessage(RelayedHttpListenerContext context)
        {
            var requestMessage = new HttpRequestMessage();
            if (context.Request.HasEntityBody)
            {
                requestMessage.Content = new StreamContent(context.Request.InputStream);
                string contentType = context.Request.Headers[HttpRequestHeader.ContentType];
                if (!string.IsNullOrEmpty(contentType))
                {
                    requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                }
            }

            string relativePath = context.Request.Url.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
            if ( relativePath.StartsWith(this.relaySubpath) )
            { 
                relativePath = relativePath.Substring(this.relaySubpath.Length);
                if (relativePath.StartsWith("/"))
                {
                    relativePath = relativePath.Substring(1);
                }                
            }
            requestMessage.RequestUri = new Uri(httpClient.BaseAddress, relativePath);
            requestMessage.Method = new HttpMethod(context.Request.HttpMethod);

            foreach (var headerName in context.Request.Headers.AllKeys)
            {
                if (string.Equals(headerName, "Host", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(headerName, "Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    // Don't flow these headers here
                    continue;
                }

                requestMessage.Headers.Add(headerName, context.Request.Headers[headerName]);
            }

            return requestMessage;
        }

        async Task SendResponseAsync(RelayedHttpListenerContext context, HttpResponseMessage responseMessage)
        {
            context.Response.StatusCode = responseMessage.StatusCode;
            context.Response.StatusDescription = responseMessage.ReasonPhrase;
            foreach (KeyValuePair<string, IEnumerable<string>> header in responseMessage.Headers)
            {
                if (string.Equals(header.Key, "Transfer-Encoding"))
                {
                    continue;
                }

                context.Response.Headers.Add(header.Key, string.Join(",", header.Value));
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in responseMessage.Content.Headers)
            {
                context.Response.Headers.Add(header.Key, string.Join(",", header.Value));
            }

            var responseStream = await responseMessage.Content.ReadAsStreamAsync();
            await responseStream.CopyToAsync(context.Response.OutputStream);
        }

        void SendErrorResponse(Exception e, RelayedHttpListenerContext context)
        {
            context.Response.StatusCode = HttpStatusCode.InternalServerError;

#if DEBUG || INCLUDE_ERROR_DETAILS
            context.Response.StatusDescription = $"Internal Server Error: {e.GetType().FullName}: {e.Message}";
#endif
            context.Response.Close();
        }

        
    }
}