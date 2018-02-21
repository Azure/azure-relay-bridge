// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.ArmClient
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class ArmEnvelope<T>
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "kind")]
        public string Kind { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "properties")]
        public T Properties { get; set; }

        [DataMember(Name = "tags")]
        public Dictionary<string, string> Tags { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}