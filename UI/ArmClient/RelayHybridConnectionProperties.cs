// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.ArmClient
{
    using System.Runtime.Serialization;

    [DataContract]
    public class RelayHybridConnectionProperties
    {
        [DataMember(Name = "createdAt")]
        public string CreatedAt { get; set; }

        [DataMember(Name = "listenerCount")]
        public int ListenerCount { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "requiresClientAuthorization")]
        public bool RequiresClientAuthorization { get; set; }

        [DataMember(Name = "updatedAt")]
        public string UpdatedAt { get; set; }

        [DataMember(Name = "userMetadata")]
        public string UserMetadata { get; set; }
    }
}