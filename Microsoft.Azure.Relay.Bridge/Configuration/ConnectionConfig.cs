// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.HybridConnectionManager.Configuration
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ConnectionConfig
    {
        public List<ConnectionListener> Listeners { get; } = new List<ConnectionListener>();
        
        public List<ConnectionTarget> Targets { get; } = new List<ConnectionTarget>();

    }
}
