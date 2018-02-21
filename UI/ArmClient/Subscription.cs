// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.ArmClient
{
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public class Subscription
    {
        public string DisplayName { get; set; }

        public string Id { get; set; }

        public string State { get; set; }

        public string TenantId { get; set; }

        public UserInfo UserInfo { get; internal set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", DisplayName, Id);
        }
    }
}