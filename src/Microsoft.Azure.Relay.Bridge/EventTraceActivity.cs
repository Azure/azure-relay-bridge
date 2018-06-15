// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;

    class EventTraceActivity
    {
        private EventTraceActivity parentActivity;

        public EventTraceActivity()
        {
            this.Activity = Guid.NewGuid();
        }

        public EventTraceActivity(EventTraceActivity activity)
        {
            this.parentActivity = activity;
        }

        public Guid Activity { get; private set; }
        public EventTraceActivity ParentActivity { get => parentActivity; }
    }
}