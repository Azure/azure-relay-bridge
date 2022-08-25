// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Diagnostics.Tracing;

    public class DiagnosticsRecord
    {
        public EventLevel Level { get; set; }
        public Guid Activity { get; set; }
        public string Message { get; set; }
        public dynamic Info { get; set; }
    }
}