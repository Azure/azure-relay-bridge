// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ForwardingServiceCommon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Configuration
    {
        public Configuration()
        {
            KeyValuePairs = new Dictionary<string, string>();
            Bindings = new List<ConfigurationBinding>();
        }

        public Dictionary<string, string> KeyValuePairs { get; set; }

        public List<ConfigurationBinding> Bindings { get; set; }
    }
}
