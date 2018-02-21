// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ForwardingServiceCommon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ConfigurationBinding
    {
        public string ConnectionString { get; set; }

        public int Port { get; set; }

        public bool IsV2 { get; set; }
    }
}
