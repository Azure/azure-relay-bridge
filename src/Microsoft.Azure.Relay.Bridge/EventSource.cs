// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Linq;
    using System.Globalization;
#if USE_MDT_EVENTSOURCE
    using Microsoft.Diagnostics.Tracing;
#else
    using System.Diagnostics.Tracing;
#endif
    using System.Diagnostics;
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using System.Net;
    using System.Net.Sockets;
    using Newtonsoft.Json;

    /// <summary>
    /// EventSource for the new Dynamic EventSource type of Microsoft-Azure-Relay-Bridge traces.
    /// 
    /// The default Level is Informational
    /// 
    /// When defining Start/Stop tasks, the StopEvent.Id must be exactly StartEvent.Id + 1.
    /// 
    /// Do not explicity include the Guid here, since EventSource has a mechanism to automatically
    /// map to an EventSource Guid based on the Name (Microsoft-Azure-Relay-Bridge).  The Guid will 
    /// be consistent as long as the name stays Microsoft-Azure-Relay-Bridge
    /// </summary>
    [EventSource(Name = "Microsoft-Azure-RelayBridge")]
    sealed class BridgeEventSource :
#if USE_MDT_EVENTSOURCE
            Microsoft.Diagnostics.Tracing.EventSource
#else
            System.Diagnostics.Tracing.EventSource
#endif
    {
        static DiagnosticSource diags = new DiagnosticListener(typeof(BridgeEventSource).Namespace);
        public static readonly BridgeEventSource Log = new BridgeEventSource();
        // Prevent additional instances other than RelayEventSource.Log

        private BridgeEventSource() { }

        [NonEvent]
        internal static EventTraceActivity NewActivity(string name)
        {
            var activity = new Activity(name);
            return new EventTraceActivity(activity);
        }

        [NonEvent]
        internal static EventTraceActivity NewActivity(string name, EventTraceActivity parent)
        {
            var activity = new Activity(name);
            activity.SetParentId(parent.DiagnosticsActivity.Id);
            return new EventTraceActivity(activity);
        }


        [NonEvent]
        public ArgumentException Argument(string paramName, string message, object source = null,
            EventLevel level = EventLevel.Error)
        {
            if (diags.IsEnabled(nameof(ArgumentException)))
            {
                diags.Write(nameof(ArgumentException), new DiagnosticsRecord { Level = EventLevel.Error, Info = new { paramName, message, source } });
            }
            return this.ThrowingException(new ArgumentException(message, paramName), source, level);
        }


        [NonEvent]
        public ArgumentNullException ArgumentNull(string paramName, object source = null,
            EventLevel level = EventLevel.Error)
        {
            if (diags.IsEnabled(nameof(ArgumentNullException)))
            {
                diags.Write(nameof(ArgumentNullException), new DiagnosticsRecord { Level = EventLevel.Error, Info = new { paramName, source } });
            }
            return this.ThrowingException(new ArgumentNullException(paramName), source, level);
        }


        [NonEvent]
        public ArgumentOutOfRangeException ArgumentOutOfRange(string paramName, string message,
            object source = null, EventLevel level = EventLevel.Error)
        {
            if (diags.IsEnabled(nameof(ArgumentOutOfRangeException)))
            {
                diags.Write(nameof(ArgumentOutOfRangeException), new DiagnosticsRecord { Level = EventLevel.Error, Info = new { paramName, message, source } });
            }
            return this.ThrowingException(new ArgumentOutOfRangeException(paramName, message), source,
                level);
        }

        // Not the actual event definition since we're using object and Exception types
        [NonEvent]
        public void HandledExceptionAsError(object source, Exception exception)
        {
            if (diags.IsEnabled(nameof(HandledExceptionAsError)))
            {
                diags.Write(nameof(HandledExceptionAsError), new DiagnosticsRecord { Level = EventLevel.Error, Info = new { source, exception } });
            }
            if (this.IsEnabled())
            {
                this.HandledExceptionAsError(CreateSourceString(source), ExceptionToString(exception));
            }
        }

        // Not the actual event definition since we're using object and Exception types
        [NonEvent]
        public void HandledExceptionAsInformation(object source, Exception exception)
        {
            if (diags.IsEnabled(nameof(HandledExceptionAsInformation)))
            {
                diags.Write(nameof(HandledExceptionAsInformation), new DiagnosticsRecord { Level = EventLevel.Informational, Info = new { source, exception } });
            }
            if (this.IsEnabled())
            {
                this.HandledExceptionAsInformation(CreateSourceString(source), ExceptionToString(exception));
            }
        }

        // Not the actual event definition since we're using object and Exception types
        [NonEvent]
        public void HandledExceptionAsWarning(object source, Exception exception)
        {
            if (diags.IsEnabled(nameof(HandledExceptionAsWarning)))
            {
                diags.Write(nameof(HandledExceptionAsWarning), new DiagnosticsRecord { Level = EventLevel.Warning, Info = new { source, exception } });
            }
            if (this.IsEnabled())
            {
                this.HandledExceptionAsWarning(CreateSourceString(source), ExceptionToString(exception));
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


        [Event(1, Channel = EventChannel.Admin, Level = EventLevel.Error, Message = "Throwing an Exception: {0} {1}")]
        internal void ThrowingExceptionError(string source, string exception)
        {
            // The IsEnabled() check is in the [NonEvent] Wrapper method
            this.WriteEvent(1, source, exception);
        }

        [Event(2, Level = EventLevel.Warning, Message = "Throwing an Exception: {0} {1}")]
        internal void ThrowingExceptionWarning(string source, string exception)
        {
            // The IsEnabled() check is in the [NonEvent] Wrapper method
            this.WriteEvent(2, source, exception);
        }

        [Event(3, Level = EventLevel.Informational, Message = "Throwing an Exception: {0} {1}")]
        void ThrowingExceptionInfo(string source, string exception)
        {
            // The IsEnabled() check is in the [NonEvent] Wrapper method
            this.WriteEvent(3, source, exception);
        }

        [Event(4, Channel = EventChannel.Admin, Level = EventLevel.Error, Message = "Exception Handled: {0} {1}")]
        void HandledExceptionAsError(string source, string exception)
        {
            this.WriteEvent(4, source, exception);
        }

        [Event(5, Message = "Exception Handled: {0} {1}")]
        void HandledExceptionAsInformation(string source, string exception)
        {
            this.WriteEvent(5, source, exception);
        }

        [Event(6, Level = EventLevel.Warning, Message = "Exception Handled: {0} {1}")]
        void HandledExceptionAsWarning(string source, string exception)
        {
            this.WriteEvent(6, source, exception);
        }

        [NonEvent]
        public void RemoteForwardHostStart(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(RemoteForwardHostStart)))
            {
                diags.Write(nameof(RemoteForwardHostStart), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            }
            RemoteForwardHostStart();
        }

        [Event(100, Channel = EventChannel.Admin, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward, Message = "Remote forward host started")]
        void RemoteForwardHostStart()
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEvent(100);
            }
        }

        [NonEvent]
        internal void RemoteForwardHostStop(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(RemoteForwardHostStop)))
            {
                diags.Write(nameof(RemoteForwardHostStop), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            }
            RemoteForwardHostStop();
        }

        [Event(101, Channel = EventChannel.Admin, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward, Message = "Remote forward host stopped")]
        void RemoteForwardHostStop()
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEvent(101);
            }
        }

        [NonEvent]
        internal void RemoteForwardHostStarting(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(RemoteForwardHostStarting)))
            {
                diags.Write(nameof(RemoteForwardHostStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            }
            RemoteForwardHostStarting();
        }

        [Event(102, Channel = EventChannel.Debug, Level = EventLevel.Verbose,
               Keywords = Keywords.RemoteForward, Message = "Remote forward host is starting")]
        void RemoteForwardHostStarting()
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEvent(102);
            }
        }

        [NonEvent]
        public void RemoteForwardHostStopping(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(RemoteForwardHostStopping)))
            {
                diags.Write(nameof(RemoteForwardHostStopping), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            }
            RemoteForwardHostStopping();
        }

        [Event(103, Channel = EventChannel.Debug, Level = EventLevel.Verbose,
               Keywords = Keywords.RemoteForward, Message = "Remote forward host is stopping")]
        public void RemoteForwardHostStopping()
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEvent(103);
            }
        }

        [NonEvent]
        internal void RemoteForwardHostStartFailure(EventTraceActivity eventTraceActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(RemoteForwardHostStartFailure)))
            {
                diags.Write(nameof(RemoteForwardHostStartFailure), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { exception } });
            }
            RemoteForwardHostStartFailure(exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(104,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward host failed to start with exception {0}, message \"{1}\"")]
        void RemoteForwardHostStartFailure(string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEvent(104, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void RemoteForwardConfigUpdating(EventTraceActivity eventTraceActivity, Config configOld, Config configNew)
        {
            if (diags.IsEnabled(nameof(RemoteForwardConfigUpdating)))
            {
                diags.Write(nameof(RemoteForwardConfigUpdating), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { configOld, configNew } });
            }
            RemoteForwardConfigUpdating();
        }

        [Event(105,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward host configuration is being updated")]
        void RemoteForwardConfigUpdating()
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEvent(105);
            }
        }

        [NonEvent]
        internal void RemoteForwardConfigUpdated(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(RemoteForwardConfigUpdated)))
            {
                diags.Write(nameof(RemoteForwardConfigUpdated), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            }
            RemoteForwardConfigUpdated();
        }


        [Event(106,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward host configuration has been updated")]
        void RemoteForwardConfigUpdated()
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEvent(106);
            }
        }

        [NonEvent]
        public void LocalForwardHostStart(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(LocalForwardHostStart)))
            {
                diags.Write(nameof(LocalForwardHostStart), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            }

            LocalForwardHostStart();
        }

        [Event(110,
            Channel = EventChannel.Admin,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward host has been started")]
        void LocalForwardHostStart()
        {
            this.WriteEvent(110);
        }

        [NonEvent]
        internal void LocalForwardHostStop(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(LocalForwardHostStop)))
            {
                diags.Write(nameof(LocalForwardHostStop), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            }
            LocalForwardHostStop();
        }

        [Event(111,
            Channel = EventChannel.Admin,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward host has been stopped")]
        void LocalForwardHostStop()
        {
            this.WriteEvent(111);
        }


        [NonEvent]
        public void LocalForwardHostStarting(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(LocalForwardHostStarting)))
            {
                diags.Write(nameof(LocalForwardHostStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            }
            LocalForwardHostStarting();
        }

        [Event(112,
            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward host starting")]
        void LocalForwardHostStarting()
        {
            this.WriteEvent(112);
        }


        [NonEvent]
        public void LocalForwardHostStopping(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(LocalForwardHostStopping)))
            {
                diags.Write(nameof(LocalForwardHostStopping), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            }
            LocalForwardHostStopping();
        }

        [Event(113,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward host stopping")]
        void LocalForwardHostStopping()
        {
            this.WriteEvent(113);
        }

        [NonEvent]
        internal void LocalForwardConfigUpdated(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(LocalForwardConfigUpdated)))
            {
                diags.Write(nameof(LocalForwardConfigUpdated), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            }
            LocalForwardConfigUpdated();
        }

        [Event(114,
            Channel = EventChannel.Admin,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward configuration updated")]
        void LocalForwardConfigUpdated()
        {
            this.WriteEvent(114);
        }


        [NonEvent]
        internal void LocalForwardBridgeConnectionStart(EventTraceActivity bridgeActivity, string localEndpoint, HybridConnectionClient hybridConnectionClient)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeConnectionStart)))
            {
                diags.Write(nameof(LocalForwardBridgeConnectionStart), new DiagnosticsRecord { Level = EventLevel.Informational, Info = new { bridgeActivity, localEndpoint, hybridConnectionClient } });
            }

            LocalForwardBridgeConnectionStart(
                localEndpoint,
                hybridConnectionClient.Address.ToString());
        }

        [Event(120,
            Channel = EventChannel.Debug,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge connection started {0} via {1}")]
        void LocalForwardBridgeConnectionStart(string localEndpoint, string relayUri)
        {
            this.WriteEvent(120, localEndpoint, relayUri);
        }

        [NonEvent]

        internal void LocalForwardBridgeConnectionStop(EventTraceActivity bridgeActivity, string endpointInfo, HybridConnectionClient hybridConnectionClient)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeConnectionStop)))
            {
                diags.Write(nameof(LocalForwardBridgeConnectionStop), new DiagnosticsRecord { Level = EventLevel.Informational, Info = new { bridgeActivity, tcpClient = endpointInfo, hybridConnectionClient } });
            }
            LocalForwardBridgeConnectionStop(
                endpointInfo,
                hybridConnectionClient.Address.ToString());
        }

        [Event(121,
            Channel = EventChannel.Debug,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge connection stopped {0} via {1}")]
        void LocalForwardBridgeConnectionStop(string localEndpoint, string relayUri)
        {
            this.WriteEvent(40331, localEndpoint, relayUri);
        }

        [NonEvent]
        internal void LocalForwardBridgeConnectionFailed(EventTraceActivity bridgeActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeConnectionFailed)))
            {
                diags.Write(nameof(LocalForwardBridgeConnectionFailed), new DiagnosticsRecord { Level = EventLevel.Informational, Info = new { bridgeActivity, exception } });
            }

            LocalForwardBridgeConnectionFailed(exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(122,
            Channel = EventChannel.Admin,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge connection failed with exception {0}, message {1}")]
        void LocalForwardBridgeConnectionFailed(string exceptionName, string exceptionMessage, string stackTrace)
        {
            this.WriteEvent(122, exceptionName, exceptionMessage, stackTrace);
        }

        [NonEvent]
        internal void LocalForwardBridgeConnectionStarting(EventTraceActivity bridgeActivity, string localEndpoint, HybridConnectionClient hybridConnectionClient)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeConnectionStarting)))
            {
                diags.Write(nameof(LocalForwardBridgeConnectionStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Info = new { bridgeActivity, localEndpoint, hybridConnectionClient } });
            }

            LocalForwardBridgeConnectionStarting(localEndpoint, hybridConnectionClient.Address.ToString());
        }

        [Event(123,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge connection starting {0} via {1}")]
        internal void LocalForwardBridgeConnectionStarting(string localAddress, string relayUri)
        {
            this.WriteEvent(123, localAddress, relayUri);
        }

        [NonEvent]
        internal void LocalForwardBridgeStart(EventTraceActivity eventTraceActivity, IPAddress bindToAddress, LocalForward localForward)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStart)))
            {
                diags.Write(nameof(LocalForwardBridgeStart), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { bindToAddress, localForward } });
            }
            LocalForwardBridgeStart(bindToAddress.ToString(), localForward.RelayName);
        }

        [Event(130,
            Channel = EventChannel.Admin,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge started binding {0} to {1}")]
        void LocalForwardBridgeStart(string bindToAddress, string relayName)
        {
            this.WriteEvent(130, bindToAddress, relayName);
        }

        [NonEvent]
        internal void LocalForwardBridgeStop(EventTraceActivity eventTraceActivity, string tcpLocalForwardBridge, string relayUri)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStop)))
            {
                diags.Write(nameof(LocalForwardBridgeStop), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { tcpLocalForwardBridge } });
            }
            LocalForwardBridgeStop(tcpLocalForwardBridge, relayUri);
        }

        [Event(131,
            Channel = EventChannel.Admin,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge stopped; bound {0} to {1}")]
        void LocalForwardBridgeStop(string bindToAddress, string relayUri)
        {
            this.WriteEvent(131, bindToAddress, relayUri);
        }

        [NonEvent]
        internal void LocalForwardBridgeStarting(EventTraceActivity eventTraceActivity, LocalForward localForward)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStarting)))
            {
                diags.Write(nameof(LocalForwardBridgeStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { localForward } });
            }
            LocalForwardBridgeStarting(localForward.RelayName);
        }

        [Event(132,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge starting for '{0}'")]
        void LocalForwardBridgeStarting(string relayName)
        {
            this.WriteEvent(132, relayName);
        }

        [NonEvent]
        internal void LocalForwardBridgeStopping(EventTraceActivity eventTraceActivity, string localForwardBridge)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStopping)))
            {
                diags.Write(nameof(LocalForwardBridgeStopping), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { localForwardBridge = localForwardBridge } });
            }
            LocalForwardBridgeStopping(localForwardBridge);
        }

        [Event(133,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge stopping for '{0}'")]
        internal void LocalForwardBridgeStopping(string endpoint)
        {
            this.WriteEvent(133, endpoint);
        }

        [NonEvent]
        internal void LocalForwardBridgeStartFailure(EventTraceActivity eventTraceActivity, LocalForward localForward, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStartFailure)))
            {
                diags.Write(nameof(LocalForwardBridgeStartFailure), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { localForward, exception } });
            }
            LocalForwardBridgeStartFailure(exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(134,
            Channel = EventChannel.Admin,
            Level = EventLevel.Error,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge failed to start with exception {0}, message {1}")]
        void LocalForwardBridgeStartFailure(string exceptionName, string exceptionMessage, string stackTrace)
        {
            this.WriteEvent(134, exceptionName, exceptionMessage, stackTrace);
        }

        [NonEvent]
        internal void LocalForwardBridgeStopFailure(EventTraceActivity eventTraceActivity, string localForwardBridge, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStopFailure)))
            {
                diags.Write(nameof(LocalForwardBridgeStopFailure), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { localForwardBridge = localForwardBridge, exception } });
            }
            LocalForwardBridgeStopFailure(exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(135,
            Channel = EventChannel.Debug,
            Level = EventLevel.Error,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge failed to stop with exception {0}, message {1}")]
        internal void LocalForwardBridgeStopFailure(string exceptionName, string exceptionMessage, string stackTrace)
        {
            this.WriteEvent(135, exceptionName, exceptionMessage, stackTrace);
        }


        [NonEvent]
        public void RemoteForwardBridgeStart(EventTraceActivity eventTraceActivity, string uri)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStart)))
            {
                diags.Write(nameof(RemoteForwardBridgeStart), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { uri } });
            }
            RemoteForwardBridgeStart(uri);
        }

        [Event(140,
            Channel = EventChannel.Admin,
            Level = EventLevel.Informational,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge started for {0}")]
        void RemoteForwardBridgeStart(string uri)
        {
            this.WriteEvent(140, uri);
        }

        [NonEvent]
        internal void RemoteForwardBridgeStop(EventTraceActivity eventTraceActivity, string clientBridge)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStop)))
            {
                diags.Write(nameof(RemoteForwardBridgeStop), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { clientBridge } });
            }
            RemoteForwardBridgeStop(clientBridge);
        }

        [Event(141,
            Channel = EventChannel.Admin,
            Level = EventLevel.Informational,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge stopped for {0}")]
        internal void RemoteForwardBridgeStop(string remoteForward)
        {
            this.WriteEvent(141, remoteForward);
        }

        [NonEvent]
        internal void RemoteForwardBridgeStopping(EventTraceActivity eventTraceActivity, string clientBridge)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStopping)))
            {
                diags.Write(nameof(RemoteForwardBridgeStopping), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { clientBridge } });
            }

            RemoteForwardBridgeStopping(clientBridge);
        }

        [Event(142,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge stopping for '{0}'")]
        void RemoteForwardBridgeStopping(string remoteForward)
        {
            this.WriteEvent(142, remoteForward);
        }

        [NonEvent]
        internal void RemoteForwardBridgeStopFailure(EventTraceActivity eventTraceActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStopFailure)))
            {
                diags.Write(nameof(RemoteForwardBridgeStopFailure), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { exception } });
            }
            RemoteForwardBridgeStopFailure(exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(143,
            Channel = EventChannel.Debug,
            Level = EventLevel.Error,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge failed to stop with exception {0}, message {1}")]
        void RemoteForwardBridgeStopFailure(string exceptionName, string exceptionMessage, string stackTrace)
        {
            this.WriteEvent(143, exceptionName, exceptionMessage, stackTrace);
        }

        [NonEvent]
        internal void RemoteForwardBridgeStarting(EventTraceActivity eventTraceActivity, RemoteForwardHost remoteForwardHost, RemoteForward remoteForward)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStarting)))
            {
                diags.Write(nameof(RemoteForwardBridgeStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { remoteForward } });
            }
            RemoteForwardBridgeStarting(remoteForwardHost.ToString(), remoteForward.ToString());
        }

        [Event(144,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge starting for '{0}' on '{1}'")]
        void RemoteForwardBridgeStarting(string remoteForwardHost, string remoteForward)
        {
            this.WriteEvent(143, remoteForwardHost, remoteForward);
        }

        [NonEvent]
        internal void RemoteForwardBridgeStartFailure(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, Exception exception)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStartFailure)))
            {
                diags.Write(nameof(RemoteForwardBridgeStartFailure), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri, exception } });
            }
            RemoteForwardBridgeStartFailure(hybridConnectionUri.ToString(), exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(145,
            Channel = EventChannel.Admin,
            Level = EventLevel.Error,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge failed to start with exception {0}, message {1}")]
        void RemoteForwardBridgeStartFailure(string hybridConnectionUri, string exceptionName, string exceptionMessage, string stackTrace)
        {
            this.WriteEvent(143, hybridConnectionUri, exceptionName, exceptionMessage, stackTrace);
        }

        [NonEvent]
        internal void RemoteForwardBridgeOnline(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, object tcpRemoteForwardBridge)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeOnline)))
            {
                diags.Write(nameof(RemoteForwardBridgeOnline), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri, tcpRemoteForwardBridge } });
            }
            RemoteForwardBridgeOnline(hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(146,
            Channel = EventChannel.Debug,
            Level = EventLevel.Informational,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge online {0} {1}")]
        internal void RemoteForwardBridgeOnline(string hybridConnectionUri, string remoteForward)
        {
            this.WriteEvent(146, hybridConnectionUri, remoteForward);
        }

        [NonEvent]
        internal void RemoteForwardBridgeOffline(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, object tcpRemoteForwardBridge)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeOffline)))
            {
                diags.Write(nameof(RemoteForwardBridgeOffline), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri, tcpRemoteForwardBridge } });
            }
            RemoteForwardBridgeOffline(hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(147,
            Channel = EventChannel.Debug,
            Level = EventLevel.Warning,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge offline {0} {1}")]
        internal void RemoteForwardBridgeOffline(string hybridConnectionUri, string remoteForward)
        {
            this.WriteEvent(147, hybridConnectionUri, remoteForward);
        }

        [NonEvent]
        internal void RemoteForwardBridgeConnecting(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, object tcpRemoteForwardBridge)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeConnecting)))
            {
                diags.Write(nameof(RemoteForwardBridgeConnecting), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri, tcpRemoteForwardBridge } });
            }

            RemoteForwardBridgeConnecting(hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(148,
            Channel = EventChannel.Debug,
            Level = EventLevel.Informational,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge connecting {0} {1}")]
        internal void RemoteForwardBridgeConnecting(string hybridConnectionUri, string remoteForward)
        {
            this.WriteEvent(148, hybridConnectionUri, remoteForward);
        }

        [NonEvent]
        internal void LocalForwardListenerStart(EventTraceActivity eventTraceActivity, string localEndpoint)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStart)))
            {
                diags.Write(nameof(LocalForwardListenerStart), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { localEndpoint } });
            }

            LocalForwardListenerStart(localEndpoint);
        }

        [Event(150,
            Channel = EventChannel.Debug,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener started {0}")]
        void LocalForwardListenerStart(string localEndpoint)
        {
            this.WriteEvent(150, localEndpoint);
        }

        [NonEvent]
        internal void LocalForwardListenerStop(EventTraceActivity eventTraceActivity, string localEndpoint)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStop)))
            {
                diags.Write(nameof(LocalForwardListenerStop), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { localEndpoint } });
            }

            LocalForwardListenerStop(localEndpoint);
        }

        [Event(151,
            Channel = EventChannel.Debug,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener stopped {0}")]
        void LocalForwardListenerStop(string localEndpoint)
        {
            this.WriteEvent(151, localEndpoint);
        }


        [NonEvent]
        internal void LocalForwardListenerStarting(EventTraceActivity eventTraceActivity, string listenEndpoint)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStarting)))
            {
                diags.Write(nameof(LocalForwardListenerStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { listenEndpoint } });
            }

            LocalForwardListenerStarting(listenEndpoint);
        }


        [Event(152,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener starting {0}")]
        internal void LocalForwardListenerStarting(string listenEndpoint)
        {
            this.WriteEvent(152, listenEndpoint);
        }

        [NonEvent]
        internal void LocalForwardListenerStopping(EventTraceActivity eventTraceActivity, string localEndpoint)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStopping)))
            {
                diags.Write(nameof(LocalForwardListenerStopping), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { localEndpoint } });
            }
            LocalForwardListenerStopping(localEndpoint);
        }


        [Event(153,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener stopping {0}")]
        internal void LocalForwardListenerStopping(string listenEndpoint)
        {
            this.WriteEvent(153, listenEndpoint);
        }

        [NonEvent]
        internal void LocalForwardListenerStoppingFailed(EventTraceActivity eventTraceActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStoppingFailed)))
            {
                diags.Write(nameof(LocalForwardListenerStoppingFailed), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { exception } });
            }
            LocalForwardListenerStoppingFailed(exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(154,
            Channel = EventChannel.Debug,
            Level = EventLevel.Error,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener failed to stop with exception {0}, message {1}")]
        internal void LocalForwardListenerStoppingFailed(string exceptionName, string exceptionMessage, string stackTrace)
        {
            this.WriteEvent(154, exceptionName, exceptionMessage, stackTrace);
        }

        [NonEvent]
        internal void LocalForwardListenerStartFailed(EventTraceActivity eventTraceActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStartFailed)))
            {
                diags.Write(nameof(LocalForwardListenerStartFailed), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { exception } });
            }

            LocalForwardListenerStartFailed(exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(155,
            Channel = EventChannel.Admin,
            Level = EventLevel.Error,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener failed to start with exception {0}, message {1}")]
        internal void LocalForwardListenerStartFailed(string exceptionName, string exceptionMessage, string stackTrace)
        {
            this.WriteEvent(155, exceptionName, exceptionMessage, stackTrace);
        }

        [NonEvent]
        internal void LocalForwardSocketAccepted(EventTraceActivity eventTraceActivity, string localEndpoint)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketAccepted)))
            {
                diags.Write(nameof(LocalForwardSocketAccepted), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { localEndpoint } });
            }

            LocalForwardSocketAccepted(localEndpoint);
        }

        [Event(160,
            Channel = EventChannel.Debug,
            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket accepted {0}")]
        void LocalForwardSocketAccepted(string socket)
        {
            this.WriteEvent(160, socket);
        }

        [NonEvent]
        internal void LocalForwardSocketError(EventTraceActivity eventTraceActivity, string endpoint, AggregateException exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketError)))
            {
                diags.Write(nameof(LocalForwardSocketError), new DiagnosticsRecord { Level = EventLevel.Warning, Activity = eventTraceActivity.Activity, Info = new { endpoint, exception } });
            }
            LocalForwardSocketError(endpoint, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(161,
            Channel = EventChannel.Debug,
            Level = EventLevel.Warning,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket error {0} {1} {2} {3}")]
        void LocalForwardSocketError(string socket, string exceptionName, string exceptionMessage, string stackTrace)
        {
            this.WriteEvent(161, socket, exceptionName, exceptionMessage, stackTrace);
        }


        [NonEvent]
        internal void LocalForwardSocketClosed(EventTraceActivity eventTraceActivity, string socket)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketClosed)))
            {
                diags.Write(nameof(LocalForwardSocketClosed), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { socket } });
            }

            LocalForwardSocketClosed("");
        }

        [Event(162,
            Channel = EventChannel.Debug,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket closed {0}")]
        internal void LocalForwardSocketClosed(string socket)
        {
            this.WriteEvent(162, socket);
        }

        [NonEvent]
        internal void LocalForwardSocketCloseFailed(EventTraceActivity eventTraceActivity, string socket, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketCloseFailed)))
            {
                diags.Write(nameof(LocalForwardSocketCloseFailed), new DiagnosticsRecord { Level = EventLevel.Warning, Activity = eventTraceActivity.Activity, Info = new { socket, exception } });
            }
            LocalForwardSocketCloseFailed(socket, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(163,
            Channel = EventChannel.Debug,
            Level = EventLevel.Warning,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket close failed {0} {1} {2} {3}")]
        void LocalForwardSocketCloseFailed(string socket, string exceptionName, string exceptionMessage, string stackTrace)
        {
            this.WriteEvent(163, socket, exceptionName, exceptionMessage, stackTrace);
        }

        [NonEvent]
        internal void LocalForwardSocketComplete(EventTraceActivity eventTraceActivity, string endpoint)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketComplete)))
            {
                diags.Write(nameof(LocalForwardSocketComplete), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { endpoint } });
            }
            LocalForwardSocketComplete(endpoint);
        }

        [Event(164,
            Channel = EventChannel.Debug,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket complete {0}")]
        void LocalForwardSocketComplete(string socket)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEvent(164, socket);
            }
        }


        [NonEvent]
        internal void LocalForwardSocketAcceptLoopFailed(EventTraceActivity eventTraceActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketAcceptLoopFailed)))
            {
                diags.Write(nameof(LocalForwardSocketAcceptLoopFailed), new DiagnosticsRecord { Level = EventLevel.Warning, Activity = eventTraceActivity.Activity, Info = new { exception } });
            }
            LocalForwardSocketAcceptLoopFailed(exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(165,
            Channel = EventChannel.Debug,
            Level = EventLevel.Error,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket accept loop failed {0} {1} {2}")]
        void LocalForwardSocketAcceptLoopFailed(string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.LocalForward))
            {
                this.WriteEvent(160, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardConfigUpdating(EventTraceActivity eventTraceActivity, Config configOld, Config configNew)
        {
            if (diags.IsEnabled(nameof(LocalForwardConfigUpdating)))
            {
                diags.Write(nameof(LocalForwardConfigUpdating), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { configOld, configNew } });
            }
            LocalForwardConfigUpdating();
        }

        [Event(166,
            Channel = EventChannel.Debug,
            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward config updating")]
        internal void LocalForwardConfigUpdating()
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEvent(166);
            }
        }

        public class Keywords // This is a bitvector
        {
            public const EventKeywords RemoteForward = (EventKeywords)0x0001;
            public const EventKeywords LocalForward = (EventKeywords)0x0002;
        }
    }
}