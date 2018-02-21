// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.ArmClient
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    class ArmClient<T>
    {
        public static string[] RdfeUrls { get; } = new[]
        {
            "https://management.core.chinacloudapi.cn",
            "https://umapi.rdfetest.dnsdemo4.com",
            "https://umapi-preview.core.windows-int.net",
            "https://management.core.windows.net"
        };

        public static async Task<T> GetAsync(Uri uri, AuthenticationResult token)
        {
            var client = new HttpClient();
            var response = await HttpInvoke(client, uri, token, "get", null);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T),
                new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
            T result = (T)ser.ReadObject(stream);
            return result;
        }

        public static async Task<HttpResponseMessage> HttpInvoke(HttpClient client, Uri uri, AuthenticationResult token,
            string verb, HttpContent content)
        {
            client.DefaultRequestHeaders.Add("Authorization", token.CreateAuthorizationHeader());
            client.DefaultRequestHeaders.Add("User-Agent", "MyUserAgent");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            if (IsRdfe(uri))
            {
                client.DefaultRequestHeaders.Add("x-ms-version", "2013-10-01");
            }

            client.DefaultRequestHeaders.Add("x-ms-request-id", Guid.NewGuid().ToString());

            HttpResponseMessage response = null;
            if (string.Equals(verb, "get", StringComparison.OrdinalIgnoreCase))
            {
                response = await client.GetAsync(uri);
            }
            else if (string.Equals(verb, "delete", StringComparison.OrdinalIgnoreCase))
            {
                response = await client.DeleteAsync(uri);
            }
            else if (string.Equals(verb, "post", StringComparison.OrdinalIgnoreCase))
            {
                response = await client.PostAsync(uri, content);
            }
            else if (string.Equals(verb, "put", StringComparison.OrdinalIgnoreCase))
            {
                response = await client.PutAsync(uri, content);
            }
            else if (string.Equals(verb, "patch", StringComparison.OrdinalIgnoreCase))
            {
                using (var message = new HttpRequestMessage(new HttpMethod("PATCH"), uri))
                {
                    message.Content = content;
                    response = await client.SendAsync(message).ConfigureAwait(false);
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("Invalid http verb {0}!", verb));
            }

            return response;
        }

        public static bool IsRdfe(Uri uri)
        {
            var host = uri.Host;
            return RdfeUrls.Any(url => url.IndexOf(host, StringComparison.OrdinalIgnoreCase) > 0);
        }

        public static async Task<T> PostAsync(Uri uri, AuthenticationResult token)
        {
            var client = new HttpClient();
            var response = await HttpInvoke(client, uri, token, "post", null);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T),
                new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
            T result = (T)ser.ReadObject(stream);
            return result;
        }
    }
}