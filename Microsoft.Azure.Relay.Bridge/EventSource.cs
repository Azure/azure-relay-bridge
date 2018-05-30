// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Globalization;
    using System.Diagnostics.Tracing;

    /// <summary>
    /// EventSource for the new Dynamic EventSource type of Microsoft-HybridConnectionManager traces.
    /// 
    /// The default Level is Informational
    /// 
    /// When defining Start/Stop tasks, the StopEvent.Id must be exactly StartEvent.Id + 1.
    /// 
    /// Do not explicity include the Guid here, since EventSource has a mechanism to automatically
    /// map to an EventSource Guid based on the Name (Microsoft-Azure-Relay).  The Guid will 
    /// be consistent as long as the name stays Microsoft-Azure-Relay
    /// </summary>
    [EventSource(Name = "Microsoft-HybridConnectionManager")]
    class EventSource : System.Diagnostics.Tracing.EventSource
    {
        public static readonly EventSource Log = new EventSource();

        // Prevent additional instances other than RelayEventSource.Log

        [NonEvent]
        public ArgumentException Argument(string paramName, string message, object source = null,
            EventLevel level = EventLevel.Error)
        {
            return this.ThrowingException(new ArgumentException(message, paramName), source, level);
        }

        [NonEvent]
        public ArgumentNullException ArgumentNull(string paramName, object source = null,
            EventLevel level = EventLevel.Error)
        {
            return this.ThrowingException(new ArgumentNullException(paramName), source, level);
        }

        [NonEvent]
        public ArgumentOutOfRangeException ArgumentOutOfRange(string paramName, object actualValue, string message,
            object source = null, EventLevel level = EventLevel.Error)
        {
            return this.ThrowingException(new ArgumentOutOfRangeException(paramName, actualValue, message), source,
                level);
        }

        [Event(39999, Level = EventLevel.Critical)]
        public void EventWriteFailFastOccurred(string errorMessage)
        {
            if (IsEnabled())
            {
                WriteEvent(39999, errorMessage);
            }
        }

        [Event(60004, Level = EventLevel.Warning, Keywords = Keywords.Client)]
        public void EventWriteLogAsWarning(string Value)
        {
            if (IsEnabled(EventLevel.Warning, Keywords.Client))
            {
                WriteEvent(60004, null, Value);
            }
        }

        public void EventWriteUnhandledException(string s)
        {
            throw new NotImplementedException();
        }

        [NonEvent]
        public void GetTokenStart(object source)
        {
            if (this.IsEnabled())
            {
                this.GetTokenStart(CreateSourceString(source));
            }
        }

        [NonEvent]
        public void GetTokenStop(DateTime tokenExpiry)
        {
            if (this.IsEnabled())
            {
                this.GetTokenStop(DateTimeToString(tokenExpiry));
            }
        }

        // Not the actual event definition since we're using object and Exception types
        [NonEvent]
        public void HandledExceptionAsError(object source, Exception exception)
        {
            if (this.IsEnabled())
            {
                this.HandledExceptionAsError(CreateSourceString(source), ExceptionToString(exception));
            }
        }

        // Not the actual event definition since we're using object and Exception types
        [NonEvent]
        public void HandledExceptionAsInformation(object source, Exception exception)
        {
            if (this.IsEnabled())
            {
                this.HandledExceptionAsInformation(CreateSourceString(source), ExceptionToString(exception));
            }
        }

        // Not the actual event definition since we're using object and Exception types
        [NonEvent]
        public void HandledExceptionAsWarning(object source, Exception exception)
        {
            if (this.IsEnabled())
            {
                this.HandledExceptionAsWarning(CreateSourceString(source), ExceptionToString(exception));
            }
        }

        public void HandledExceptionWarning(EventTraceActivity activity, string source, string memberName,
            string systemTracker, string trackingId, string toStringSlim)
        {
            throw new NotImplementedException();
        }

        public void HandledExceptionWithFunctionName(EventTraceActivity activity, string catchLocation,
            string toStringSlim, string empty)
        {
            throw new NotImplementedException();
        }

        [Event(40328, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionClientConfigSettingsChanged(EventTraceActivity activity, string message)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40328, activity, message);
            }
        }

        [Event(40330, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionClientConfigurationFileError(EventTraceActivity activity, string message)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40330, activity, message);
            }
        }

        [Event(40324, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionClientFailedToStart(EventTraceActivity activity, string uri, string message,
            string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40324, activity, uri, message, stackTrace);
            }
        }

        [Event(40325, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionClientFailedToStop(EventTraceActivity activity, string uri, string message,
            string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40325, activity, uri, message, stackTrace);
            }
        }

        [Event(40333, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionClientManagementServerError(EventTraceActivity activity, string error)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40333, activity, error);
            }
        }

        [Event(40331, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionClientManagementServiceStarting(EventTraceActivity activity, string port)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40331, activity, port);
            }
        }

        [Event(40332, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionClientManagementServiceStopping(EventTraceActivity activity, string port)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40332, activity, port);
            }
        }

        [Event(40329, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionClientProxyError(EventTraceActivity activity, string error)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40329, activity, error);
            }
        }

        [Event(40320, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionClientServiceStarting()
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40320);
            }
        }

        [Event(40321, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionClientServiceStopping()
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40321);
            }
        }

        [Event(40322, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionClientStarted(EventTraceActivity activity, string uri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40322, activity, uri);
            }
        }

        [Event(40323, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionClientStopped(EventTraceActivity activity, string uri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40323, activity, uri);
            }
        }

        [Event(40326, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionClientTrace(EventTraceActivity activity, string trace)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40326, activity, trace);
            }
        }

        [Event(40306, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionFailedToReadResourceDescriptionMetaData(EventTraceActivity activity, string uri,
            string exception, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40306, activity, uri, exception, stackTrace);
            }
        }

        [Event(40304, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionFailedToStart(EventTraceActivity activity, string uri, string message,
            string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40304, activity, uri, message, stackTrace);
            }
        }

        [Event(40305, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionFailedToStop(EventTraceActivity activity, string uri, string message,
            string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40305, activity, uri, message, stackTrace);
            }
        }

        [Event(40309, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionInvalidConnectionString(EventTraceActivity activity, string uri)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40309, activity, uri);
            }
        }

        [Event(40308, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionManagerConfigSettingsChanged(EventTraceActivity activity, string message)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40308, activity, message);
            }
        }

        [Event(40310, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionManagerConfigurationFileError(EventTraceActivity activity, string message)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40310, activity, message);
            }
        }

        [Event(40311, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionManagerManagementServerError(EventTraceActivity activity, string error)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40311, activity, error);
            }
        }

        [Event(40312, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionManagerManagementServiceStarting(EventTraceActivity activity, string port)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40312, activity, port);
            }
        }

        [Event(40313, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionManagerManagementServiceStopping(EventTraceActivity activity, string port)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40313, activity, port);
            }
        }

        [Event(40300, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionManagerStarting()
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40300);
            }
        }

        [Event(40301, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionManagerStopping()
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40301);
            }
        }

        [Event(40314, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionManagerTrace(EventTraceActivity activity, string trace)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40314, activity, trace);
            }
        }

        [Event(40307, Level = EventLevel.Error, Keywords = Keywords.Client)]
        public void HybridConnectionSecurityException(EventTraceActivity activity, string uri, string exception)
        {
            if (IsEnabled(EventLevel.Error, Keywords.Client))
            {
                this.WriteEvent(40307, activity, uri, exception);
            }
        }

        [Event(40302, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionStarted(EventTraceActivity activity, string uri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40302, activity, uri);
            }
        }

        [Event(40303, Level = EventLevel.Informational, Keywords = Keywords.Client)]
        public void HybridConnectionStopped(EventTraceActivity activity, string uri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Client))
            {
                this.WriteEvent(40303, activity, uri);
            }
        }

        [NonEvent]
        public void RelayClientCloseException(object source, Exception exception)
        {
            if (this.IsEnabled())
            {
                this.RelayClientCloseException(CreateSourceString(source), ExceptionToString(exception));
            }
        }

        [NonEvent]
        public void RelayClientCloseStart(object source)
        {
            if (this.IsEnabled())
            {
                this.RelayClientCloseStart(CreateSourceString(source));
            }
        }

        [Event(40213, Message = "Relay object closed.")]
        public void RelayClientCloseStop()
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(40213);
            }
        }

        [NonEvent]
        public void RelayClientConnectStart(object source)
        {
            if (this.IsEnabled())
            {
                this.RelayClientConnectStart(CreateSourceString(source));
            }
        }

        [Event(40202, Message = "Relay object Connect stop.")]
        public void RelayClientConnectStop()
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(40202);
            }
        }

        [Event(40191, Message = "Relay object is online: Source: {0}.")]
        public void RelayClientGoingOnline(string source)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(40191, source);
            }
        }

        [NonEvent]
        public void RelayClientShutdownStart(object source)
        {
            if (this.IsEnabled())
            {
                this.RelayClientShutdownStart(CreateSourceString(source));
            }
        }

        [Event(40216, Message = "Relay object Shutdown complete.")]
        public void RelayClientShutdownStop()
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(40216);
            }
        }

        [Event(40193, Message = "Relay object stop connecting: Source: {0}, ListenerType: {1}.")]
        public void RelayClientStopConnecting(string source, string listenerType)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(40193, source, listenerType);
            }
        }

        [Event(40205, Message = "Relay Listener accepted a client connection.")]
        public void RelayListenerRendezvousStop()
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(40205);
            }
        }

        [Event(40208, Level = EventLevel.Warning,
            Message = "Relayed Listener received an unknown command. {0}, Command: {1}.")]
        public void RelayListenerUnknownCommand(string source, string command)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(40208, source, command);
            }
        }

        [NonEvent]
        public TException ThrowingException<TException>(TException exception, object source = null,
            EventLevel level = EventLevel.Error)
            where TException : Exception
        {
            // Avoid converting ToString, etc. if ETW tracing is not enabled.
            if (this.IsEnabled())
            {
                switch (level)
                {
                    case EventLevel.Critical:
                    case EventLevel.LogAlways:
                    case EventLevel.Error:
                        this.ThrowingExceptionError(CreateSourceString(source), ExceptionToString(exception));
                        break;
                    case EventLevel.Warning:
                        this.ThrowingExceptionWarning(CreateSourceString(source), ExceptionToString(exception));
                        break;
                    case EventLevel.Informational:
                    case EventLevel.Verbose:
                    default:
                        this.ThrowingExceptionInfo(CreateSourceString(source), ExceptionToString(exception));
                        break;
                }
            }

            // This allows "throw ServiceBusEventSource.Log.ThrowingException(..."
            return exception;
        }

        public void ThrowingExceptionVerbose(EventTraceActivity activity, Exception exception)
        {
            throw new NotImplementedException();
        }

        [NonEvent]
        public void TokenRenewScheduled(TimeSpan interval, object source)
        {
            if (this.IsEnabled())
            {
                this.TokenRenewScheduled(interval.ToString(), CreateSourceString(source));
            }
        }

        [NonEvent]
        internal static string CreateSourceString(object source)
        {
            Type type;
            if (source == null)
            {
                return string.Empty;
            }
            else if ((type = source as Type) != null)
            {
                return type.Name;
            }

            return source.ToString();
        }

        [Event(40262, Level = EventLevel.Error, Message = "Throwing an Exception: {0} {1}")]
        internal void ThrowingExceptionError(string source, string exception)
        {
            // The IsEnabled() check is in the [NonEvent] Wrapper method
            this.WriteEvent(40262, source, exception);
        }

        [Event(40263, Level = EventLevel.Warning, Message = "Throwing an Exception: {0} {1}")]
        internal void ThrowingExceptionWarning(string source, string exception)
        {
            // The IsEnabled() check is in the [NonEvent] Wrapper method
            this.WriteEvent(40263, source, exception);
        }

        [NonEvent]
        static string DateTimeToString(DateTime dateTime)
        {
            return dateTime.ToString(CultureInfo.InvariantCulture);
        }

        [NonEvent]
        static string ExceptionToString(Exception exception)
        {
            return exception?.ToString() ?? string.Empty;
        }

        [Event(40255, Level = EventLevel.Informational, Message = "GetToken start: {0}")]
        void GetTokenStart(string source)
        {
            this.WriteEvent(40255, source);
        }

        [Event(40256, Level = EventLevel.Informational, Message = "GetToken stop. New token expires at {0}.")]
        void GetTokenStop(string tokenExpiry)
        {
            this.WriteEvent(40256, tokenExpiry);
        }

        [Event(40251, Level = EventLevel.Error, Message = "Exception Handled: {0} {1}")]
        void HandledExceptionAsError(string source, string exception)
        {
            this.WriteEvent(40251, source, exception);
        }

        [Event(40249, Message = "Exception Handled: {0} {1}")]
        void HandledExceptionAsInformation(string source, string exception)
        {
            this.WriteEvent(40249, source, exception);
        }

        [Event(40250, Level = EventLevel.Warning, Message = "Exception Handled: {0} {1}")]
        void HandledExceptionAsWarning(string source, string exception)
        {
            this.WriteEvent(40250, source, exception);
        }

        [Event(40214, Message = "Relay object closing encountered exception: {0}, Exception: {1}.",
            Level = EventLevel.Warning)]
        void RelayClientCloseException(string source, string exception)
        {
            this.WriteEvent(40214, source, exception);
        }

        [Event(40212, Message = "Relay object closing: {0}.")]
        void RelayClientCloseStart(string source)
        {
            this.WriteEvent(40212, source);
        }

        [Event(40201, Message = "Relay object Connect start: {0}.")]
        void RelayClientConnectStart(string source)
        {
            this.WriteEvent(40201, source);
        }

        [Event(40215, Message = "Relay object Shutdown beginning: {0}")]
        void RelayClientShutdownStart(string source)
        {
            this.WriteEvent(40215, source);
        }

        // 40203 Available

        [Event(40204,
            Message = "Relay Listener received a connection request. {0}, ConnectionId: {1}, Rendezvous Address: {2}.")]
        void RelayListenerRendezvousStart(string source, string clientId, string rendezvousAddress)
        {
            this.WriteEvent(40204, source, clientId, rendezvousAddress);
        }

        [Event(40264, Level = EventLevel.Informational, Message = "Throwing an Exception: {0} {1}")]
        void ThrowingExceptionInfo(string source, string exception)
        {
            // The IsEnabled() check is in the [NonEvent] Wrapper method
            this.WriteEvent(40264, source, exception);
        }

        [Event(40257, Level = EventLevel.Informational, Message = "Scheduling Token renewal after {0}. {1}.")]
        void TokenRenewScheduled(string interval, string source)
        {
            this.WriteEvent(40257, interval, source);
        }

        public class Keywords // This is a bitvector
        {
            public const EventKeywords Client = (EventKeywords)0x0001;

            //public const EventKeywords Relay = (EventKeywords)0x0002;
            //public const EventKeywords Messaging = (EventKeywords)0x0002;
        }
    }

    class EventTraceActivity
    {
    }
}