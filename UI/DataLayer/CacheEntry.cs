// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi.DataLayer
{
    using System;
    using System.Collections.Generic;

    public class CacheEntry
    {
        public IEnumerable<HybridConnectionCacheEntity> Connections { get; set; }

        public DateTime CacheTime { get; set; }
    }
}