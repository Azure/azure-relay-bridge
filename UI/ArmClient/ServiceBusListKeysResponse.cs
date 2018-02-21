// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.ArmClient
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ServiceBusListKeysResponse
    {
        [DataMember(Name = "keyName")]
        public string KeyName { get; set; }

        [DataMember(Name = "primaryConnectionString")]
        public string PrimaryConnectionString { get; set; }

        [DataMember(Name = "primaryKey")]
        public string PrimaryKey { get; set; }

        [DataMember(Name = "secondaryConnectionString")]
        public string SecondaryConnectionString { get; set; }

        [DataMember(Name = "secondaryKey")]
        public string SecondaryKey { get; set; }
    }
}