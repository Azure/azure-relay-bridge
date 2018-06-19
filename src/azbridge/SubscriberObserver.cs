// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace azbridge
{
    using System;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;
#if USE_MDT_EVENTSOURCE
#else
    using System.Diagnostics.Tracing;
#endif

    class SubscriberObserver : IObserver<DiagnosticListener>
    {
        private ILogger logger;

        public SubscriberObserver(ILogger logger)
        {
            this.logger = logger;
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == "Microsoft.Azure.Relay.Bridge")
            {
                value.Subscribe(new TraceObserver(logger));
            }
        }
    }
}
