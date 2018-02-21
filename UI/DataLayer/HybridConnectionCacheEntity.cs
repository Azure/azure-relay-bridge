// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.DataLayer
{
    using System;
    using System.Drawing;
    using Microsoft.HybridConnectionManager;

    public enum HybridConnectionState
    {
        NotConnected,

        Connected,

        NotFound,

        Unauthorized,

        Unknown
    }

    public class HybridConnectionCacheEntity
    {
        public string ArmUri { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool Enableable { get; set; }

        public string Endpoint { get; set; }

        public string KeyName { get; set; }

        public string KeyValue { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public int ListenersConnected { get; set; }

        public string Namespace { get; set; }

        public string Region { get; set; }

        public string RelayName { get; set; }

        public string ServiceBusEndpoint { get; set; }

        public HybridConnectionState State { get; set; }

        public string GetNamespaceDisplayName()
        {
            // Biztalk names are actually service bus namespaces with the name OmAV7RWgl5lJiUGmC09nSCOm-[biztalkservicename].
            // The - character is reserved and cannot be created normally, but only through biztalk API's so by checking for it
            // we can determine if this is a biztalk name (and remove the ugly part).
            string biztalkSpecialString = "OmAV7RWgl5lJiUGmC09nSCOm-".ToLower();

            if (Namespace.ToLower().StartsWith(biztalkSpecialString))
            {
                return Namespace.Substring(biztalkSpecialString.Length);
            }
            else
            {
                return Namespace;
            }
        }

        public string GetStateText()
        {
            switch (State)
            {
                case HybridConnectionState.NotConnected:
                    return "Not Connected";
                case HybridConnectionState.Connected:
                    return "Connected";
                case HybridConnectionState.NotFound:
                    return "Not Found";
                case HybridConnectionState.Unauthorized:
                    return "Unauthorized";
                case HybridConnectionState.Unknown:
                default:
                    return "Status Unknown";
            }
        }

        public Color GetStateTextColor()
        {
            if (State == HybridConnectionState.Connected)
            {
                return Color.FromArgb(255, 0, 176, 80);
            }

            return Color.Red;
        }
    }
}