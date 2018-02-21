// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.ArmClient
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class ArmCollectionEnvelope<T>
    {
        [DataMember(Name = "value")]
        public IEnumerable<ArmEnvelope<T>> Value { get; set; }
    }
}