// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.ArmClient
{
    using System.Runtime.Serialization;

    [DataContract]
    public class AuthorizationRuleProperties
    {
        [DataMember(Name = "rights")]
        public string[] Rights { get; set; }
    }
}