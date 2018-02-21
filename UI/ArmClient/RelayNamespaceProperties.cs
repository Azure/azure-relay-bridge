// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.ArmClient
{
    using System.Runtime.Serialization;

    [DataContract]
    public class RelayNamespaceProperties
    {
        [DataMember(Name = "createdAt")]
        public string CreatedAt { get; set; }

        [DataMember(Name = "critical")]
        public bool Critical { get; set; }

        [DataMember(Name = "enabled")]
        public bool Enabled { get; set; }

        [DataMember(Name = "eventHubEnabled")]
        public string EventHubEnabled { get; set; }

        [DataMember(Name = "messagingSku")]
        public int MessagingSku { get; set; }

        [DataMember(Name = "metricId")]
        public string MetricId { get; set; }

        [DataMember(Name = "namespaceType")]
        public string NamespaceType { get; set; }

        [DataMember(Name = "provisioningState")]
        public string ProvisioningState { get; set; }

        [DataMember(Name = "serviceBusEndpoint")]
        public string ServiceBusEndpoint { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "updatedAt")]
        public string UpdatedAt { get; set; }
    }
}