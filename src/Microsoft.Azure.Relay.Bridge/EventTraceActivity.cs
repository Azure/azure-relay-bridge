// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;

    class EventTraceActivity
    {
        private System.Diagnostics.Activity diagnosticsActivity;

        public EventTraceActivity(System.Diagnostics.Activity diagnosticsActivity)
        {
            this.diagnosticsActivity = diagnosticsActivity;
            this.Activity = Guid.NewGuid();
        }
                                                                                                                                                     
        public Guid Activity { get; private set; }
        public System.Diagnostics.Activity DiagnosticsActivity { get => diagnosticsActivity; }
    }
}