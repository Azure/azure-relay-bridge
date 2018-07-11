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
                diags.Write(nameof(ArgumentException), new DiagnosticsRecord { Level  = EventLevel.Error, Info = new { paramName, message, source } });
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
                diags.Write(nameof(HandledExceptionAsError), new DiagnosticsRecord { Level  = EventLevel.Error, Info = new { source, exception } });
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
                diags.Write(nameof(HandledExceptionAsWarning), new DiagnosticsRecord { Level  = EventLevel.Warning, Info = new { source, exception } });
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
                diags.Write(nameof(RemoteForwardHostStart), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            }
            RemoteForwardHostStart(eventTraceActivity.Activity);
        }

        [Event(100, Channel = EventChannel.Admin, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward, Message = "Remote forward host started")]
        void RemoteForwardHostStart(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(100, relatedActivityId);
            }
        }

        [NonEvent]
        internal void RemoteForwardHostStop(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(RemoteForwardHostStop)))
            {
                diags.Write(nameof(RemoteForwardHostStop), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            }
            RemoteForwardHostStop(eventTraceActivity.Activity);
        }

        [Event(101, Channel = EventChannel.Admin, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward, Message = "Remote forward host stopped")]
        void RemoteForwardHostStop(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(101, relatedActivityId);
            }
        }

        [NonEvent]
        internal void RemoteForwardHostStarting(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(RemoteForwardHostStarting)))
            {
                diags.Write(nameof(RemoteForwardHostStarting), new DiagnosticsRecord { Level  = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            }
            RemoteForwardHostStarting(eventTraceActivity.Activity);
        }

        [Event(102, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward, Message = "Remote forward host is starting")]
        void RemoteForwardHostStarting(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(102, relatedActivityId);
            }
        }

        [NonEvent]
        public void RemoteForwardHostStopping(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(RemoteForwardHostStopping)))
            {
                diags.Write(nameof(RemoteForwardHostStopping), new DiagnosticsRecord { Level  = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            }
            RemoteForwardHostStopping(eventTraceActivity.Activity);
        }

        [Event(103, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward, Message = "Remote forward host is stopping")]
        public void RemoteForwardHostStopping(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(103, relatedActivityId);
            }
        }

        [NonEvent]
        internal void RemoteForwardHostStartFailure(EventTraceActivity eventTraceActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(RemoteForwardHostStartFailure)))
            {
                diags.Write(nameof(RemoteForwardHostStartFailure), new DiagnosticsRecord { Level  = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { exception } });
            }
            RemoteForwardHostStartFailure(eventTraceActivity.Activity, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(104, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward, Message = "Remote forward host failed to start with exception {0}, message \"{1}\"")]
        void RemoteForwardHostStartFailure(Guid relatedActivityId, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(104, relatedActivityId, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void RemoteForwardConfigUpdating(EventTraceActivity eventTraceActivity, Config configOld, Config configNew)
        {
            if (diags.IsEnabled(nameof(RemoteForwardConfigUpdating)))
            {
                diags.Write(nameof(RemoteForwardConfigUpdating), new DiagnosticsRecord { Level  = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { configOld, configNew } });
            }
            RemoteForwardConfigUpdating(eventTraceActivity.Activity);
        }

        [Event(105, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward, Message = "Remote forward host configuration is being updated")]
        void RemoteForwardConfigUpdating(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(105, relatedActivityId);
            }
        }

        [NonEvent]
        internal void RemoteForwardConfigUpdated(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(RemoteForwardConfigUpdated)))
            {
                diags.Write(nameof(RemoteForwardConfigUpdated), new DiagnosticsRecord { Level  = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            }
            RemoteForwardConfigUpdated(eventTraceActivity.Activity);
        }


        [Event(106, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward, Message = "Remote forward host configuration has been updated")]
        void RemoteForwardConfigUpdated(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(106, relatedActivityId);
            }
        }

        [NonEvent]
        public void LocalForwardHostStart(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(LocalForwardHostStart)))
            {
                diags.Write(nameof(LocalForwardHostStart), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            }

            LocalForwardHostStart(eventTraceActivity.Activity);
        }

        [Event(110, Channel = EventChannel.Admin, Level = EventLevel.Informational, Keywords = Keywords.LocalForward, Message = "Local forward host has been started")]
        void LocalForwardHostStart(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(110, relatedActivityId);
            }
        }

        [NonEvent]
        internal void LocalForwardHostStop(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(LocalForwardHostStop)))
            {
                diags.Write(nameof(LocalForwardHostStop), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            }
            LocalForwardHostStop(eventTraceActivity.Activity);
        }

        [Event(111, Channel = EventChannel.Admin, Level = EventLevel.Informational, Keywords = Keywords.LocalForward, Message = "Local forward host has been stopped")]
        void LocalForwardHostStop(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(111, relatedActivityId);
            }
        }


        [NonEvent]
        public void LocalForwardHostStarting(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(LocalForwardHostStarting)))
            {
                diags.Write(nameof(LocalForwardHostStarting), new DiagnosticsRecord { Level  = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            }
            LocalForwardHostStarting(eventTraceActivity.Activity);
        }

        [Event(112, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        void LocalForwardHostStarting(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(112, relatedActivityId);
            }
        }


        [NonEvent]
        public void LocalForwardHostStopping(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(LocalForwardHostStopping)))
            {
                diags.Write(nameof(LocalForwardHostStopping), new DiagnosticsRecord { Level  = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            }
            LocalForwardHostStopping(eventTraceActivity.Activity);
        }

        [Event(113, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        void LocalForwardHostStopping(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(113, relatedActivityId);
            }
        }

        [NonEvent]
        internal void LocalForwardConfigUpdated(EventTraceActivity eventTraceActivity)
        {
            if (diags.IsEnabled(nameof(LocalForwardConfigUpdated)))
            {
                diags.Write(nameof(LocalForwardConfigUpdated), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            }
            LocalForwardConfigUpdated(eventTraceActivity.Activity);
        }

        [Event(114, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardConfigUpdated(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(114, relatedActivityId);
            }
        }


        [NonEvent]
        internal void LocalForwardBridgeConnectionStart(EventTraceActivity bridgeActivity, TcpClient tcpClient, HybridConnectionClient hybridConnectionClient)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeConnectionStart)))
            {
                diags.Write(nameof(LocalForwardBridgeConnectionStart), new DiagnosticsRecord { Level = EventLevel.Informational, Info = new { bridgeActivity, tcpClient, hybridConnectionClient } });
            }

            LocalForwardBridgeConnectionStart(
                bridgeActivity.Activity,
                tcpClient.Client.LocalEndPoint.ToString(),
                hybridConnectionClient.Address.ToString());
        }

        [Event(120, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardBridgeConnectionStart(Guid relatedActivityId, string localEndpoint, string relayUri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(120, relatedActivityId, localEndpoint, relayUri);
            }
        }

        [NonEvent]

        internal void LocalForwardBridgeConnectionStop(EventTraceActivity bridgeActivity, string endpointInfo, HybridConnectionClient hybridConnectionClient)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeConnectionStop)))
            {
                diags.Write(nameof(LocalForwardBridgeConnectionStop), new DiagnosticsRecord { Level = EventLevel.Informational, Info = new { bridgeActivity, tcpClient = endpointInfo, hybridConnectionClient } });
            }
            LocalForwardBridgeConnectionStop(
                bridgeActivity.Activity,
                endpointInfo,
                hybridConnectionClient.Address.ToString());
        }

        [Event(121, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardBridgeConnectionStop(Guid relatedActivityId, string localEndpoint, string relayUri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(40331, relatedActivityId, localEndpoint, relayUri);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeConnectionFailed(EventTraceActivity bridgeActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeConnectionFailed)))
            {
                diags.Write(nameof(LocalForwardBridgeConnectionFailed), new DiagnosticsRecord { Level = EventLevel.Informational, Info = new { bridgeActivity, exception } });
            }

            LocalForwardBridgeConnectionFailed(bridgeActivity.Activity, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(122, Channel = EventChannel.Admin, Level = EventLevel.Informational, Keywords = Keywords.LocalForward, Message = "Local forward bridge connection failed with exception {0}, message {1}")]
        void LocalForwardBridgeConnectionFailed(Guid relatedActivityId, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(122, relatedActivityId, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeConnectionStarting(EventTraceActivity bridgeActivity, TcpClient tcpClient, HybridConnectionClient hybridConnectionClient)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeConnectionStarting)))
            {
                diags.Write(nameof(LocalForwardBridgeConnectionStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Info = new { bridgeActivity, tcpClient, hybridConnectionClient } });
            }

            LocalForwardBridgeConnectionStarting(bridgeActivity.Activity, tcpClient.Client.LocalEndPoint.ToString(), hybridConnectionClient.Address.ToString());
        }

        [Event(123, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        internal void LocalForwardBridgeConnectionStarting(Guid relatedActivityId, string localAddress, string relayUri)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(123, relatedActivityId, localAddress, relayUri);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeStart(EventTraceActivity eventTraceActivity, IPAddress bindToAddress, LocalForward localForward)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStart)))
            {
                diags.Write(nameof(LocalForwardBridgeStart), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { bindToAddress, localForward } });
            }
            LocalForwardBridgeStart(eventTraceActivity.Activity, bindToAddress.ToString(), localForward.RelayName);
        }

        [Event(130, Channel = EventChannel.Admin, Level = EventLevel.Informational, Keywords = Keywords.LocalForward, Message = "Local forward bridge started binding {0} to {1}")]
        void LocalForwardBridgeStart(Guid relatedActivityId, string bindToAddress, string relayName)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(130, relatedActivityId, bindToAddress, relayName);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeStop(EventTraceActivity eventTraceActivity, TcpLocalForwardBridge tcpLocalForwardBridge)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStop)))
            {
                diags.Write(nameof(LocalForwardBridgeStop), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { tcpLocalForwardBridge } });
            }
            LocalForwardBridgeStop(eventTraceActivity.Activity, tcpLocalForwardBridge.GetIpEndPoint()?.ToString(), tcpLocalForwardBridge.HybridConnectionClient?.Address.ToString());
        }

        [Event(131, Channel = EventChannel.Admin, Level = EventLevel.Informational, Keywords = Keywords.LocalForward, Message = "Local forward bridge stopped; bound {0} to {1}")]
        void LocalForwardBridgeStop(Guid relatedActivityId, string bindToAddress, string relayUri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(131, relatedActivityId, bindToAddress, relayUri);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeStarting(EventTraceActivity eventTraceActivity, LocalForward localForward)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStarting)))
            {
                diags.Write(nameof(LocalForwardBridgeStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { localForward } });
            }
            LocalForwardBridgeStarting(eventTraceActivity.Activity, localForward.RelayName);
        }

        [Event(132, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        void LocalForwardBridgeStarting(Guid relatedActivityId, string relayName)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(132, relatedActivityId, relayName);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeStopping(EventTraceActivity eventTraceActivity, TcpLocalForwardBridge tcpLocalForwardBridge)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStopping)))
            {
                diags.Write(nameof(LocalForwardBridgeStopping), new DiagnosticsRecord { Level  = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { tcpLocalForwardBridge } });
            }
            LocalForwardBridgeStopping(eventTraceActivity.Activity, tcpLocalForwardBridge.GetIpEndPoint()?.ToString());
        }

        [Event(133, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        internal void LocalForwardBridgeStopping(Guid relatedActivityId, string endpoint)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(133, relatedActivityId, endpoint);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeStartFailure(EventTraceActivity eventTraceActivity, LocalForward localForward, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStartFailure)))
            {
                diags.Write(nameof(LocalForwardBridgeStartFailure), new DiagnosticsRecord { Level  = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { localForward, exception } });
            }
            LocalForwardBridgeStartFailure(eventTraceActivity.Activity, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(134, Channel = EventChannel.Admin, Level = EventLevel.Error, Keywords = Keywords.LocalForward, Message = "Local forward bridge failed to start with exception {0}, message {1}")]
        void LocalForwardBridgeStartFailure(Guid relatedActivityId, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(134, relatedActivityId, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeStopFailure(EventTraceActivity eventTraceActivity, TcpLocalForwardBridge tcpLocalForwardBridge, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardBridgeStopFailure)))
            {
                diags.Write(nameof(LocalForwardBridgeStopFailure), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { tcpLocalForwardBridge, exception } });
            }
            LocalForwardBridgeStopFailure(eventTraceActivity.Activity, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(135, Channel = EventChannel.Debug, Level = EventLevel.Error, Keywords = Keywords.LocalForward, Message = "Local forward bridge failed to stop with exception {0}, message {1}")]
        internal void LocalForwardBridgeStopFailure(Guid relatedActivityId, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(135, relatedActivityId, exceptionName, exceptionMessage, stackTrace);
            }
        }


        [NonEvent]
        public void RemoteForwardBridgeStart(EventTraceActivity eventTraceActivity, string uri)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStart)))
            {
                diags.Write(nameof(RemoteForwardBridgeStart), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { uri } });
            }
            RemoteForwardBridgeStart(eventTraceActivity.Activity, uri);
        }

        [Event(140, Channel = EventChannel.Admin, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward, Message = "Remote forward bridge started for {0}")]
        void RemoteForwardBridgeStart(Guid relatedActivityId, string uri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(140, relatedActivityId, uri);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeStop(EventTraceActivity eventTraceActivity, TcpRemoteForwardBridge tcpClientBridge)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStop)))
            {
                diags.Write(nameof(RemoteForwardBridgeStop), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { tcpClientBridge } });
            }
            RemoteForwardBridgeStop(eventTraceActivity.Activity, tcpClientBridge.ToString());
        }

        [Event(141, Channel = EventChannel.Admin, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward, Message = "Remote forward bridge stopped for {0}")]
        internal void RemoteForwardBridgeStop(Guid relatedActivityId, string remoteForward)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(141, relatedActivityId, remoteForward);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeStopping(EventTraceActivity eventTraceActivity, TcpRemoteForwardBridge tcpClientBridge)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStopping)))
            {
                diags.Write(nameof(RemoteForwardBridgeStopping), new DiagnosticsRecord { Level  = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { tcpClientBridge } });
            }

            RemoteForwardBridgeStopping(eventTraceActivity.Activity, tcpClientBridge.ToString());
        }

        [Event(142, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward)]
        void RemoteForwardBridgeStopping(Guid relatedActivityId, string remoteForward)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(142, relatedActivityId, remoteForward);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeStopFailure(EventTraceActivity eventTraceActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStopFailure)))
            {
                diags.Write(nameof(RemoteForwardBridgeStopFailure), new DiagnosticsRecord { Level  = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { exception } });
            }
            RemoteForwardBridgeStopFailure(eventTraceActivity.Activity, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(143, Channel = EventChannel.Debug, Level = EventLevel.Error, Keywords = Keywords.RemoteForward, Message = "Remote forward bridge failed to stop with exception {0}, message {1}")]
        void RemoteForwardBridgeStopFailure(Guid relatedActivityId, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(143, relatedActivityId, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeStarting(EventTraceActivity eventTraceActivity, RemoteForwardHost remoteForwardHost, RemoteForward remoteForward)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStarting)))
            {
                diags.Write(nameof(RemoteForwardBridgeStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { remoteForward } });
            }
            RemoteForwardBridgeStarting(eventTraceActivity.Activity, remoteForwardHost.ToString(), remoteForward.ToString());
        }

        [Event(144, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward)]
        void RemoteForwardBridgeStarting(Guid relatedActivityId, string remoteForwardHost, string remoteForward)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(143, relatedActivityId, remoteForwardHost, remoteForward);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeStartFailure(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, Exception exception)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeStartFailure)))
            {
                diags.Write(nameof(RemoteForwardBridgeStartFailure), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri, exception } });
            }
            RemoteForwardBridgeStartFailure(eventTraceActivity.Activity, hybridConnectionUri.ToString(), exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(145, Channel = EventChannel.Admin, Level = EventLevel.Error, Keywords = Keywords.RemoteForward, Message = "Remote forward bridge failed to start with exception {0}, message {1}")]
        void RemoteForwardBridgeStartFailure(Guid relatedActivityId, string hybridConnectionUri, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(143, relatedActivityId, hybridConnectionUri, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeOnline(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, TcpRemoteForwardBridge tcpRemoteForwardBridge)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeOnline)))
            {
                diags.Write(nameof(RemoteForwardBridgeOnline), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri, tcpRemoteForwardBridge } });
            }
            RemoteForwardBridgeOnline(eventTraceActivity.Activity, hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(146, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward)]
        internal void RemoteForwardBridgeOnline(Guid relatedActivityId, string hybridConnectionUri, string remoteForward)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(146, relatedActivityId, hybridConnectionUri, remoteForward);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeOffline(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, TcpRemoteForwardBridge tcpRemoteForwardBridge)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeOffline)))
            {
                diags.Write(nameof(RemoteForwardBridgeOffline), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri, tcpRemoteForwardBridge } });
            }
            RemoteForwardBridgeOffline(eventTraceActivity.Activity, hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(147, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward)]
        internal void RemoteForwardBridgeOffline(Guid relatedActivityId, string hybridConnectionUri, string remoteForward)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(147, relatedActivityId, hybridConnectionUri, remoteForward);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeConnecting(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, TcpRemoteForwardBridge tcpRemoteForwardBridge)
        {
            if (diags.IsEnabled(nameof(RemoteForwardBridgeConnecting)))
            {
                diags.Write(nameof(RemoteForwardBridgeConnecting), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri, tcpRemoteForwardBridge } });
            }

            RemoteForwardBridgeConnecting(eventTraceActivity.Activity, hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(148, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward)]
        internal void RemoteForwardBridgeConnecting(Guid relatedActivityId, string hybridConnectionUri, string remoteForward)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(148, relatedActivityId, hybridConnectionUri, remoteForward);
            }
        }

        [NonEvent]
        internal void LocalForwardListenerStart(EventTraceActivity eventTraceActivity, TcpListener tcpListener)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStart)))
            {
                diags.Write(nameof(LocalForwardListenerStart), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { tcpListener } });
            }

            LocalForwardListenerStart(eventTraceActivity.Activity, tcpListener.LocalEndpoint.ToString());
        }

        [Event(150, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardListenerStart(Guid relatedActivityId, string localEndpoint)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(150, relatedActivityId, localEndpoint);
            }
        }

        [NonEvent]
        internal void LocalForwardListenerStop(EventTraceActivity eventTraceActivity, TcpListener tcpListener)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStop)))
            {
                diags.Write(nameof(LocalForwardListenerStop), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { tcpListener } });
            }

            LocalForwardListenerStop(eventTraceActivity.Activity, tcpListener.LocalEndpoint.ToString());
        }

        [Event(151, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardListenerStop(Guid relatedActivityId, string localEndpoint)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(151, relatedActivityId, localEndpoint);
            }
        }


        [NonEvent]
        internal void LocalForwardListenerStarting(EventTraceActivity eventTraceActivity, IPEndPoint listenEndpoint)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStarting)))
            {
                diags.Write(nameof(LocalForwardListenerStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { listenEndpoint } });
            }

            LocalForwardListenerStarting(eventTraceActivity.Activity, listenEndpoint.ToString());
        }


        [Event(152, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        internal void LocalForwardListenerStarting(Guid relatedActivityId, string listenEndpoint)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(152, relatedActivityId, listenEndpoint);
            }
        }

        [NonEvent]
        internal void LocalForwardListenerStopping(EventTraceActivity eventTraceActivity, TcpListener listenEndpoint)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStopping)))
            {
                diags.Write(nameof(LocalForwardListenerStopping), new DiagnosticsRecord { Level  = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { listenEndpoint } });
            }
            LocalForwardListenerStopping(eventTraceActivity.Activity, listenEndpoint.LocalEndpoint.ToString());
        }


        [Event(153, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        internal void LocalForwardListenerStopping(Guid relatedActivityId, string listenEndpoint)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(153, relatedActivityId, listenEndpoint);
            }
        }

        [NonEvent]
        internal void LocalForwardListenerStoppingFailed(EventTraceActivity eventTraceActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStoppingFailed)))
            {
                diags.Write(nameof(LocalForwardListenerStoppingFailed), new DiagnosticsRecord { Level  = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { exception } });
            }
            LocalForwardListenerStoppingFailed(eventTraceActivity.Activity, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(154, Channel = EventChannel.Debug, Level = EventLevel.Error, Keywords = Keywords.LocalForward, Message = "Local forward listener failed to stop with exception {0}, message {1}")]
        internal void LocalForwardListenerStoppingFailed(Guid relatedActivityId, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(154, relatedActivityId, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardListenerStartFailed(EventTraceActivity eventTraceActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardListenerStartFailed)))
            {
                diags.Write(nameof(LocalForwardListenerStartFailed), new DiagnosticsRecord { Level  = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { exception } });
            }

            LocalForwardListenerStartFailed(eventTraceActivity.Activity, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(155, Channel = EventChannel.Admin, Level = EventLevel.Error, Keywords = Keywords.LocalForward, Message = "Local forward listener failed to start with exception {0}, message {1}")]
        internal void LocalForwardListenerStartFailed(Guid relatedActivityId, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(155, relatedActivityId, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardSocketAccepted(EventTraceActivity eventTraceActivity, TcpClient socket)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketAccepted)))
            {
                diags.Write(nameof(LocalForwardSocketAccepted), new DiagnosticsRecord { Level  = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { socket } });
            }

            LocalForwardSocketAccepted(eventTraceActivity.Activity, socket.Client.LocalEndPoint.ToString());
        }

        [Event(160, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        void LocalForwardSocketAccepted(Guid relatedActivityId, string socket)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(160, relatedActivityId, socket);
            }
        }

        [NonEvent]
        internal void LocalForwardSocketError(EventTraceActivity eventTraceActivity, string endpoint, AggregateException exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketError)))
            {
                diags.Write(nameof(LocalForwardSocketError), new DiagnosticsRecord { Level  = EventLevel.Warning, Activity = eventTraceActivity.Activity, Info = new { endpoint, exception } });
            }
            LocalForwardSocketError(eventTraceActivity.Activity, endpoint, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(161, Channel = EventChannel.Debug, Level = EventLevel.Warning, Keywords = Keywords.LocalForward)]
        void LocalForwardSocketError(Guid relatedActivityId, string socket, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Warning, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(161, relatedActivityId, socket, exceptionName, exceptionMessage, stackTrace);
            }
        }


        [NonEvent]
        internal void LocalForwardSocketClosed(EventTraceActivity eventTraceActivity, string socket)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketClosed)))
            {
                diags.Write(nameof(LocalForwardSocketClosed), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { socket } });
            }

            LocalForwardSocketClosed(eventTraceActivity.Activity, "");
        }

        [Event(162, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        internal void LocalForwardSocketClosed(Guid relatedActivityId, string socket)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(162, relatedActivityId, socket);
            }
        }

        [NonEvent]
        internal void LocalForwardSocketCloseFailed(EventTraceActivity eventTraceActivity, string socket, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketCloseFailed)))
            {
                diags.Write(nameof(LocalForwardSocketCloseFailed), new DiagnosticsRecord { Level  = EventLevel.Warning, Activity = eventTraceActivity.Activity, Info = new { socket, exception } });
            }
            LocalForwardSocketCloseFailed(eventTraceActivity.Activity, socket, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(163, Level = EventLevel.Warning, Keywords = Keywords.LocalForward)]
        void LocalForwardSocketCloseFailed(Guid relatedActivityId, string socket, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Warning, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(163, relatedActivityId, socket, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardSocketComplete(EventTraceActivity eventTraceActivity, string endpoint)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketComplete)))
            {
                diags.Write(nameof(LocalForwardSocketComplete), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { endpoint } });
            }
            LocalForwardSocketComplete(eventTraceActivity.Activity, endpoint);
        }

        [Event(164, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardSocketComplete(Guid relatedActivityId, string socket)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(164, relatedActivityId, socket);
            }
        }


        [NonEvent]
        internal void LocalForwardSocketAcceptLoopFailed(EventTraceActivity eventTraceActivity, Exception exception)
        {
            if (diags.IsEnabled(nameof(LocalForwardSocketAcceptLoopFailed)))
            {
                diags.Write(nameof(LocalForwardSocketAcceptLoopFailed), new DiagnosticsRecord { Level  = EventLevel.Warning, Activity = eventTraceActivity.Activity, Info = new { exception } });
            }
            LocalForwardSocketAcceptLoopFailed(eventTraceActivity.Activity, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(165, Channel = EventChannel.Debug, Level = EventLevel.Error, Keywords = Keywords.LocalForward)]
        void LocalForwardSocketAcceptLoopFailed(Guid relatedActivityId, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(160, relatedActivityId, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardConfigUpdating(EventTraceActivity eventTraceActivity, Config configOld, Config configNew)
        {
            if (diags.IsEnabled(nameof(LocalForwardConfigUpdating)))
            {
                diags.Write(nameof(LocalForwardConfigUpdating), new DiagnosticsRecord { Level  = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { configOld, configNew } });
            }
            LocalForwardConfigUpdating(eventTraceActivity.Activity);
        }

        [Event(166, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        internal void LocalForwardConfigUpdating(Guid relatedActivityId)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(166, relatedActivityId);
            }
        }
                                                                                                                                                                                                            
        public class Keywords // This is a bitvector
        {
            public const EventKeywords RemoteForward = (EventKeywords)0x0001;
            public const EventKeywords LocalForward = (EventKeywords)0x0002;
        }
    }
}