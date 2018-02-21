// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.ArmClient
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class ArmAuthorization
    {
        const string AzureActiveDirectoryEndpoint = "https://login.windows.net/";

        const string AzureActiveDirectoryEndpointResourceId = "https://management.core.windows.net/";

        const string AzurePowerShellCliendId = "1950a258-227b-4e31-a9cf-717495945fc2";

        const string AzurePowerShellRedirectUri = "urn:ietf:wg:oauth:2.0:oob";

        const string CsmApiVersion = "2014-04-01-preview";

        const string CsmEndpoint = "https://management.azure.com/";

        const string EnableEbdMagicCookie = "site_id=501358&display=popup";

        public static async Task<IEnumerable<Subscription>> GetSubscriptions(object ownerWindow = null)
        {
            List<Subscription> subscriptions = new List<Subscription>();

            AuthenticationResult token = GetToken(ownerWindow: ownerWindow);

            IEnumerable<string> tenantIds = (await CallAzure<JObject>("GET", "/tenants", token))
                .Value<JArray>("value")
                .Select(tenant => tenant.Value<string>("tenantId"));

            foreach (string tenantId in tenantIds)
            {
                AuthenticationResult tenantToken = GetToken(
                    tenantId: tenantId,
                    userInfo: token.UserInfo,
                    ownerWindow: ownerWindow);

                IEnumerable<Subscription> subscriptionsInTenant = (await
                    CallAzure<JObject>("GET", "/subscriptions", tenantToken))
                    .Value<JArray>("value")
                    .Select(tenant => new Subscription()
                    {
                        Id = tenant.Value<string>("subscriptionId"),
                        DisplayName = tenant.Value<string>("displayName"),
                        State = tenant.Value<string>("state"),
                        TenantId = tenantId,
                        UserInfo = token.UserInfo
                    });

                subscriptions.AddRange(subscriptionsInTenant);
            }

            return subscriptions.OrderBy(sub => sub.DisplayName);
        }

        public static AuthenticationResult GetToken(string tenantId = "common", UserInfo userInfo = null,
            object ownerWindow = null)
        {
            AuthenticationContext authContext =
                new AuthenticationContext(authority: AzureActiveDirectoryEndpoint + tenantId, validateAuthority: false);
            authContext.OwnerWindow = ownerWindow;

            AuthenticationResult authenticationResult = userInfo == null
                ? authContext.AcquireToken(
                    resource: AzureActiveDirectoryEndpointResourceId,
                    clientId: AzurePowerShellCliendId,
                    redirectUri: new Uri(AzurePowerShellRedirectUri),
                    promptBehavior: tenantId == "common" ? PromptBehavior.Always : PromptBehavior.Auto,
                    userId: UserIdentifier.AnyUser,
                    extraQueryParameters: EnableEbdMagicCookie)
                : authContext.AcquireToken(
                    resource: AzureActiveDirectoryEndpointResourceId,
                    clientId: AzurePowerShellCliendId,
                    redirectUri: new Uri(AzurePowerShellRedirectUri),
                    promptBehavior: PromptBehavior.Auto,
                    userId: new UserIdentifier(userInfo.DisplayableId, UserIdentifierType.OptionalDisplayableId),
                    extraQueryParameters: EnableEbdMagicCookie);

            return authenticationResult;
        }

        static async Task<T> CallAzure<T>(string method, string path, AuthenticationResult token,
            string body = null)
        {
            return await CallAzure<T>(method, path, token != null ? token.CreateAuthorizationHeader() : null, body);
        }

        static async Task<T> CallAzure<T>(string method, string path, string token, string body = null)
        {
            Uri requestUri = CreateCsmUri(path.Replace(" ", "%20"));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = method;
            request.Headers["Authorization"] = token;
            request.ContentType = "application/json";

            if (!string.IsNullOrWhiteSpace(body))
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    byte[] content = Encoding.UTF8.GetBytes(body);
                    requestStream.Write(content, 0, content.Length);
                }
            }

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            using (Stream receiveStream = response.GetResponseStream())
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    string content = readStream.ReadToEnd();

                    return JsonConvert.DeserializeObject<T>(content);
                }
            }
        }

        static Uri CreateCsmUri(string path)
        {
            return new Uri(string.Format("{0}{1}?api-version={2}", CsmEndpoint, path, CsmApiVersion));
        }
    }
}