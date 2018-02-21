// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using HybridConnectionManagerIbizaUi.ArmClient;
    using Microsoft.Azure.Relay;
    using Microsoft.HybridConnectionManager;
    using Microsoft.HybridConnectionManager.Commands;
    using Microsoft.HybridConnectionManager.Configuration;
    using Microsoft.Win32;

    public static class HybridConnectionDataManager
    {
        static Dictionary<string, CacheEntry> availableHybridConnections;

        static Dictionary<string, HybridConnectionCacheEntity> configuredHybridConnections;

        static bool isReadingConfiguredConnections;

        static object readLock = new object();

        static Dictionary<string, Subscription> subscriptionCache;

        static HybridConnectionDataManager()
        {
            availableHybridConnections = new Dictionary<string, CacheEntry>();
            isReadingConfiguredConnections = false;
        }

        static Configuration Config { get; set; }

        static HybridConnectionsConfigurationSection HybridConnectionsSection { get; set; }

        public static async Task ConfigureHybridConnectionFromConnectionString(string connectionString)
        {
            var cb = new RelayConnectionStringBuilder(connectionString);

            HybridConnectionsSection.HybridConnections.Add(new HybridConnectionElement()
            {
                ConnectionString = cb.ToString()
            });
            Config.Save();

            await Task.Delay(5000);

            await Main.MainForm.LoadConfiguredHybridConnections();
        }

        public static async Task ConfigureNewHybridConnections(List<HybridConnectionCacheEntity> hybridConnections)
        {
            // We will add each hybrid connection to the config and save it.
            foreach (var conn in hybridConnections)
            {
                // We need to query the keys for the connection before we can add it.
                var bestKey = await GetBestListenKey(conn.ArmUri, "2016-07-01");

                if (bestKey == null)
                {
                    // We can't add this connection...
                    throw new Exception("Hybrid connection " + conn.RelayName + " has no suitable listen key.");
                }

                // Build the connection string
                string connectionString =
                    string.Format("Endpoint=sb://{0}/{1};SharedAccessKeyName={2};SharedAccessKey={3}",
                        conn.ServiceBusEndpoint, conn.RelayName,
                        bestKey.KeyName, bestKey.KeyValue);

                string armUri = conn.ArmUri;

                HybridConnectionsSection.HybridConnections.Add(new HybridConnectionElement()
                {
                    ConnectionString = connectionString,
                    ArmUri = armUri
                });
            }

            Config.Save();

            // Hack, wait 5 seconds to allow the connection to be made.
            await Task.Delay(5000);

            await Main.MainForm.LoadConfiguredHybridConnections();
        }

        public static async Task<IEnumerable<HybridConnectionCacheEntity>> GetAllHybridConnectionsFromArmOrCache(
            string subscriptionId)
        {
            Subscription subscription;
            if (!subscriptionCache.TryGetValue(subscriptionId, out subscription))
            {
                throw new InvalidOperationException("Subscription " + subscriptionId +
                                                    " is not defined. Call GetSubscriptions() first.");
            }

            lock (availableHybridConnections)
            {
                // Add an empty cache entry. If we already have a cache entry just return it.
                CacheEntry hybridConnectionsCache;
                if (availableHybridConnections.TryGetValue(subscription.Id, out hybridConnectionsCache))
                {
                    if ((DateTime.UtcNow - hybridConnectionsCache.CacheTime).TotalSeconds < 30)
                    {
                        return hybridConnectionsCache.Connections;
                    }
                }
                else
                {
                    // We will add a null entry. If caller receives a null entry that should mean to them that loading is
                    // still in progress in another thread.
                    availableHybridConnections.Add(subscription.Id, null);
                }
            }

            var hybridConnectionsFromArmOrCache = GetHybridConnectionsFromArmOrCache(subscriptionId);

            var hcs = await hybridConnectionsFromArmOrCache;

            var relays = hcs;

            lock (availableHybridConnections)
            {
                availableHybridConnections[subscription.Id] = new CacheEntry()
                {
                    Connections = relays,
                    CacheTime = DateTime.UtcNow
                };
            }

            return relays;
        }

        public static IEnumerable<HybridConnectionCacheEntity> GetCachedConfiguredConnections()
        {
            return configuredHybridConnections.Values;
        }

        public static async Task<IEnumerable<HybridConnectionCacheEntity>> GetConfiguredHybridConnections()
        {
            lock (readLock)
            {
                if (isReadingConfiguredConnections)
                {
                    // Still loading. Return no results.
                    return null;
                }
            }

            try
            {
                lock (readLock)
                {
                    isReadingConfiguredConnections = true;
                }

                // We will read from the Hybrid Connection Managers' listeners .exe.config file
                ReadConfig();

                HybridConnectionsSection =
                    Config.GetSection(HybridConnectionConstants.HybridConnectionsSectionConfigName) as
                        HybridConnectionsConfigurationSection;

                configuredHybridConnections = new Dictionary<string, HybridConnectionCacheEntity>();
                if (HybridConnectionsSection != null)
                {
                    foreach (var hc in HybridConnectionsSection.HybridConnections)
                    {
                        var connString = ((HybridConnectionElement)hc).ConnectionString;
                        var armUri = ((HybridConnectionElement)hc).ArmUri;
                        var cb = new RelayConnectionStringBuilder(connString);

                        // The URI above is like sb://[namespace].servicebus.windows.net/[relayname], we need to talk to 
                        // sb://[namespace].servicebus.windows.net, so we'll just construct a new URI with only the protocol and host parts
                        Uri namespaceUri = new Uri(string.Format("{0}://{1}", cb.Endpoint.Scheme, cb.Endpoint.Host));
                        string namespaceName = cb.Endpoint.Host.Split('.')[0];
                        var relayName = cb.Endpoint.AbsolutePath.Substring(1);

                        HybridConnectionCacheEntity hybridConnection = new HybridConnectionCacheEntity()
                        {
                            Namespace = namespaceName,
                            RelayName = relayName,
                            KeyName = cb.SharedAccessKeyName,
                            KeyValue = cb.SharedAccessKey,
                            ServiceBusEndpoint = cb.Endpoint.Host,
                            ArmUri = armUri,
                            CreatedDate = DateTime.MinValue,
                            LastUpdatedDate = DateTime.MinValue
                        };

                        try
                        {
                            await Dns.GetHostAddressesAsync(namespaceUri.Host);
                            HybridConnectionListener listener = new HybridConnectionListener(connString);
                            var runtimeRelay = await listener.GetRuntimeInformationAsync();

                            hybridConnection.Endpoint =
                                Util.GetEndpointStringFromUserMetadata(runtimeRelay.UserMetadata);
                            hybridConnection.State = runtimeRelay.ListenerCount > 0
                                ? HybridConnectionState.Connected
                                : HybridConnectionState.NotConnected;
                            hybridConnection.ListenersConnected = runtimeRelay.ListenerCount;
                            hybridConnection.CreatedDate = runtimeRelay.CreatedAt;
                            hybridConnection.LastUpdatedDate = runtimeRelay.UpdatedAt;
                        }
                        catch (AggregateException e)
                        {
                            // AggregateException is thrown by Dns.GetHostAddressesAsync. In it there should be a SocketException with code 11001 (name not found). Anything else
                            // we will consider status unknown.
                            bool isNotFound = false;
                            foreach (var exception in e.InnerExceptions)
                            {
                                if (exception is SocketException)
                                {
                                    var socketException = exception as SocketException;
                                    if (socketException.ErrorCode == 11001)
                                    {
                                        isNotFound = true;
                                        break;
                                    }
                                }
                            }

                            if (isNotFound)
                            {
                                hybridConnection.State = HybridConnectionState.NotFound;
                            }
                            else
                            {
                                // Network might be down.
                                hybridConnection.State = HybridConnectionState.Unknown;
                            }
                        }
                        catch (SocketException e)
                        {
                            if (e.ErrorCode == 11001)
                            {
                                hybridConnection.State = HybridConnectionState.NotFound;
                            }
                            else
                            {
                                hybridConnection.State = HybridConnectionState.Unknown;
                            }
                        }
                        catch (WebException)
                        {
                            // Probably due to network connectivity..
                            hybridConnection.State = HybridConnectionState.Unknown;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            hybridConnection.State = HybridConnectionState.Unauthorized;
                        }
                        catch (Exception)
                        {
                            hybridConnection.State = HybridConnectionState.Unknown;
                        }

                        configuredHybridConnections.Add(GetHybridConnectionKey(namespaceName, relayName),
                            hybridConnection);
                    }
                }

                return configuredHybridConnections.Values;
            }
            finally
            {
                lock (readLock)
                {
                    isReadingConfiguredConnections = false;
                }
            }
        }

        public static string GetHybridConnectionKey(string relayNamespace, string relayName)
        {
            return string.Format("{0}+{1}", relayNamespace.ToLower(), relayName.ToLower());
        }

        // NOTE: There are three kinds of hybrid connections. Old Biztalk Hybrid Connections, new ServiceBus Hybrid Connections (Microsoft.ServiceBus/relays
        // which will be deprecated and unused) and new ServiceBus relay provider Hybrid Connections (Microsoft.Relays/hybridconnections)
        public static async Task<IEnumerable<HybridConnectionCacheEntity>> GetHybridConnectionsFromArmOrCache(
            string subscriptionId)
        {
            Subscription subscription;
            if (!subscriptionCache.TryGetValue(subscriptionId, out subscription))
            {
                throw new InvalidOperationException("Subscription " + subscriptionId +
                                                    " is not defined. Call GetSubscriptions() first.");
            }

            Uri uri = new Uri(string.Format(
                "https://management.azure.com/subscriptions/{0}/resources?$filter=resourceType%20eq%20%27Microsoft.Relay/namespaces%27&api-version=2015-01-01",
                subscription.Id));

            var token = ArmAuthorization.GetToken(subscription.TenantId, subscription.UserInfo);

            List<HybridConnectionCacheEntity> relayList = new List<HybridConnectionCacheEntity>();

            var namespaces = await ArmClient<ArmCollectionEnvelope<EmptyProperties>>.GetAsync(uri, token);

            foreach (var ns in namespaces.Value)
            {
                // Get the relays associated with this namespace
                try
                {
                    uri = new Uri(string.Format("https://management.azure.com{0}?api-version=2016-07-01", ns.Id));
                    var retrievedNamespace = await ArmClient<ArmEnvelope<RelayNamespaceProperties>>.GetAsync(uri, token);

                    uri =
                        new Uri(string.Format(
                            "https://management.azure.com{0}/hybridconnections?api-version=2016-07-01", ns.Id));
                    var relays =
                        await ArmClient<ArmCollectionEnvelope<RelayHybridConnectionProperties>>.GetAsync(uri, token);

                    foreach (var relay in relays.Value)
                    {
                        Uri serviceBusUri = new Uri(retrievedNamespace.Properties.ServiceBusEndpoint);

                        string endpoint = relay.Properties.UserMetadata != null ? Util.GetEndpointStringFromUserMetadata(relay.Properties.UserMetadata) : null;
                        bool hasEndpoint = !string.IsNullOrEmpty(endpoint);
                        if (!hasEndpoint)
                        {
                            endpoint = "No endpoint configured.";
                        }

                        relayList.Add(
                            new HybridConnectionCacheEntity()
                            {
                                Endpoint = endpoint,
                                KeyName = string.Empty,
                                KeyValue = string.Empty,
                                Namespace = ns.Name,
                                RelayName = relay.Name,
                                ServiceBusEndpoint = serviceBusUri.Host,
                                State = HybridConnectionState.Unknown,
                                Region = ns.Location,
                                ArmUri = relay.Id,
                                Enableable = hasEndpoint
                            });
                    }
                }
                catch (HttpRequestException)
                {
                    // Can't do anything about this...
                }
            }

            return relayList;
        }

        public static string GetInstallPath()
        {
            return Path.GetDirectoryName(typeof(HybridConnectionDataManager).Assembly.Location);
        }

        public static async Task<IEnumerable<Subscription>> GetSubscriptions()
        {
            if (subscriptionCache != null)
            {
                return subscriptionCache.Values;
            }
            else
            {
                var subscriptions = await ArmAuthorization.GetSubscriptions();

                subscriptionCache = new Dictionary<string, Subscription>();

                foreach (var subscription in subscriptions)
                {
                    subscriptionCache.Add(subscription.Id, subscription);
                }

                return subscriptions;
            }
        }

        public static bool IsHybridConnectionAlreadyConfigured(string namespaceName, string relayName)
        {
            string key = GetHybridConnectionKey(namespaceName, relayName);

            if (configuredHybridConnections != null)
            {
                return configuredHybridConnections.ContainsKey(key);
            }
            else
            {
                return false;
            }
        }

        public static async Task<bool> IsUpdateAvailable()
        {
            try
            {
                string latestVersion = await HybridConnectionDataManager.GetLatestHybridConnectionManagerVersion();
                Version latest = Version.Parse(latestVersion);
                Version current = Version.Parse(HybridConnectionConstants.HybridConnectionManagerVersion);

                return latest > current;
            }
            catch (Exception e)
            {
                Util.HybridConnectionManagerTrace("Failed to retrieve update information: {0}", e.ToString());
                return false;
            }
        }

        public static void RemoveHybridConnection(HybridConnectionCacheEntity hybridConnection)
        {
            string endpointKey = string.Format("sb://{0}/{1}", hybridConnection.ServiceBusEndpoint,
                hybridConnection.RelayName);

            HybridConnectionsSection.HybridConnections.Remove(endpointKey);

            Config.Save();
        }

        static HybridConnectionCacheEntity CreateFailedHybridConnectionWithState(string namespaceName, string relayName,
            HybridConnectionState state)
        {
            return new HybridConnectionCacheEntity() { Namespace = namespaceName, RelayName = relayName, State = state };
        }

        static async Task<HybridConnectionKey> GetBestListenKey(string armUri, string apiVersion = "2015-08-01")
        {
            // We want to query the relay for a suitable key first - a suitable key being a key that has the Listen permission
            // and, ideally, no other permission (this is prioritized). If we only have keys that have both Listen and another
            // permission then we will use it. If there are no keys, then we will query the namespace instead for a suitable
            // key.
            string subscriptionId = GetSubscriptionFromArmUri(armUri);

            if (subscriptionId == null)
            {
                throw new InvalidOperationException("Arm URI is malformed or has no subscription.");
            }

            Subscription subscription;
            if (!subscriptionCache.TryGetValue(subscriptionId, out subscription))
            {
                throw new InvalidOperationException("Subscription " + subscriptionId +
                                                    " is not defined. Call GetSubscriptions() first.");
            }

            var suitableKey = await GetBestListenKeyFromUri(armUri, apiVersion);

            if (suitableKey == null)
            {
                // We have no suitable key. If we are looking at relay, look at namespace instead. Otherwise, fail.
                string[] uriParts = armUri.Split('/');

                if (
                    string.Equals(uriParts[uriParts.Length - 2], "relays", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(uriParts[uriParts.Length - 2], "HybridConnections",
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    // Build a URI just up to the namespace part.
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < uriParts.Length - 2; i++)
                    {
                        sb.Append(uriParts[i]);
                        if (i != uriParts.Length - 3)
                        {
                            sb.Append("/");
                        }
                    }

                    return await GetBestListenKeyFromUri(sb.ToString(), apiVersion);
                }
                else
                {
                    throw new Exception("No suitable keys could be found for this relay.");
                }
            }

            return suitableKey;
        }

        static async Task<HybridConnectionKey> GetBestListenKeyFromUri(string armUri, string apiVersion = "2015-08-01")
        {
            string subscriptionId = GetSubscriptionFromArmUri(armUri);

            Subscription subscription;
            if (!subscriptionCache.TryGetValue(subscriptionId, out subscription))
            {
                throw new InvalidOperationException("Subscription " + subscriptionId +
                                                    " is not defined. Call GetSubscriptions() first.");
            }

            var token = ArmAuthorization.GetToken(subscription.TenantId, subscription.UserInfo);

            // Query relay keys
            Uri uri =
                new Uri(string.Format("https://management.azure.com{0}/authorizationRules?api-version={1}", armUri,
                    apiVersion));

            var keys = await ArmClient<ArmCollectionEnvelope<AuthorizationRuleProperties>>.GetAsync(uri, token);

            ArmEnvelope<AuthorizationRuleProperties> suitableKey = null;

            // Find a suitable key, first check for keys that have the listen right and no other, then check for keys that have the listen right regardless
            // of others
            if (keys.Value.Count() > 0)
            {
                foreach (var key in keys.Value)
                {
                    if (key.Properties.Rights.Length == 1)
                    {
                        if (string.Equals("Listen", key.Properties.Rights[0],
                            StringComparison.InvariantCultureIgnoreCase))
                        {
                            suitableKey = key;
                            break;
                        }
                    }
                }

                if (suitableKey == null)
                {
                    // Look for any key that has the listen property instead
                    foreach (var key in keys.Value)
                    {
                        foreach (var right in key.Properties.Rights)
                        {
                            if (string.Equals("Listen", right, StringComparison.InvariantCultureIgnoreCase))
                            {
                                suitableKey = key;
                                break;
                            }
                        }
                    }
                }
            }

            if (suitableKey == null)
            {
                return null;
            }
            else
            {
                // Do a POST on the uri:
                // [armuri]/authorizationRules/{keyName}/listKeys
                // the suitable keys' id goes up to the key name, so we just need to add on /listKeys
                uri =
                    new Uri(string.Format("https://management.azure.com{0}/listKeys?api-version={1}", suitableKey.Id,
                        apiVersion));

                var keyValue = await ArmClient<ServiceBusListKeysResponse>.PostAsync(uri, token);

                return new HybridConnectionKey()
                {
                    KeyName = keyValue.KeyName,
                    KeyValue = keyValue.PrimaryKey
                };
            }
        }

        static async Task<string> GetLatestHybridConnectionManagerVersion()
        {
            HttpClient client = new HttpClient();
            var result = await client.GetAsync("https://go.microsoft.com/fwlink/?linkid=864323");

            // In the link above, first line is version number, second line is build date.
            using (StreamReader reader = new StreamReader(await result.Content.ReadAsStreamAsync()))
            {
                return reader.ReadLine();
            }
        }

        static string GetSubscriptionFromArmUri(string armUri)
        {
            string[] uriParts = armUri.Split('/');

            if (uriParts.Length < 3)
            {
                return null;
            }

            return uriParts[2];
        }

        static void ReadConfig()
        {
            var installPath = GetInstallPath();

            string configFilePath = Path.Combine(installPath, HybridConnectionConstants.ConfigFileName);
            if (!File.Exists(configFilePath))
            {
                Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            else
            {
                Config = ConfigurationManager.OpenMappedExeConfiguration(
                                    new ExeConfigurationFileMap { ExeConfigFilename = configFilePath, }, ConfigurationUserLevel.None);
            }
        }
    }
}