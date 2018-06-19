// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace azbridge
{
    using System;
    using Microsoft.Azure.Relay.Bridge;
#if USE_MDT_EVENTSOURCE
    using Microsoft.Diagnostics.Tracing;
#else
    using System.Diagnostics.Tracing;
#endif
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    class TraceObserver : IObserver<KeyValuePair<string, object>>
    {
        private ILogger logger;

        public TraceObserver(ILogger logger)
        {
            this.logger = logger;
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            DiagnosticsRecord record = (DiagnosticsRecord)value.Value;

            string message = $"[{DateTime.UtcNow}], {value.Key}, {record.Activity}, {record.Info}";
            switch (record.Level)
            {
                case EventLevel.Critical:
                    logger.LogCritical(message);
                    break;
                case EventLevel.Error:
                    logger.LogError(message);
                    break;
                case EventLevel.Informational:
                    logger.LogInformation(message);
                    break;
                case EventLevel.LogAlways:
                    logger.LogTrace(message);
                    break;
                case EventLevel.Verbose:
                    logger.LogDebug(message);
                    break;
                case EventLevel.Warning:
                    logger.LogWarning(message);
                    break;
            }
        }
    }
}
