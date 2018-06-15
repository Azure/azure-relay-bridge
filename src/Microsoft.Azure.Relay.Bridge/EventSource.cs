// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Globalization;
    using System.Diagnostics.Tracing;
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using System.Net;
    using System.Net.Sockets;

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
    [EventSource(Name = "Microsoft-Azure-Relay-Bridge")]
    sealed class BridgeEventSource : System.Diagnostics.Tracing.EventSource
    {
        public static readonly BridgeEventSource Log = new BridgeEventSource();
        // Prevent additional instances other than RelayEventSource.Log

        private BridgeEventSource() { }

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


        [Event(1, Level = EventLevel.Error, Message = "Throwing an Exception: {0} {1}")]
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

        [Event(4, Level = EventLevel.Error, Message = "Exception Handled: {0} {1}")]
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
        public void RemoteForwardHostStarted(EventTraceActivity activity)
        {
            RemoteForwardHostStarted(activity.Activity);
        }

        [Event(100, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward)]
        void RemoteForwardHostStarted(Guid activity)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(100, activity);
            }
        }

        [NonEvent]
        internal void RemoteForwardHostStopped(EventTraceActivity activity)
        {
            RemoteForwardHostStopped(activity.Activity);
        }

        [Event(101, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward)]
        void RemoteForwardHostStopped(Guid activity)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(101, activity);
            }
        }

        [NonEvent]
        internal void RemoteForwardHostStarting(EventTraceActivity activity)
        {
            RemoteForwardHostStarting(activity.Activity);
        }

        [Event(102, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward)]
        void RemoteForwardHostStarting(Guid activity)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(102, activity);
            }
        }

        [NonEvent]
        public void RemoteForwardHostStopping(EventTraceActivity activity)
        {
            RemoteForwardHostStopping(activity.Activity);
        }

        [Event(103, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward)]
        public void RemoteForwardHostStopping(Guid activity)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(103, activity);
            }
        }

        [NonEvent]
        internal void RemoteForwardHostFailedToStart(EventTraceActivity activity, Exception e)
        {
            RemoteForwardHostFailedToStart(activity.Activity, e.GetType().FullName, e.Message, e.StackTrace);
        }

        [Event(104, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward)]
        void RemoteForwardHostFailedToStart(Guid guid, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(104, guid, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void RemoteForwardConfigUpdating(EventTraceActivity activity, Config config1, Config config2)
        {
            RemoteForwardConfigUpdating(activity.Activity);
        }

        [Event(105, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward)]
        void RemoteForwardConfigUpdating(Guid guid)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(105, guid);
            }
        }

        [NonEvent]
        internal void RemoteForwardConfigUpdated(EventTraceActivity activity)
        {
            RemoteForwardConfigUpdated(activity.Activity);
        }


        [Event(106, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward)]
        void RemoteForwardConfigUpdated(Guid guid)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(106, guid);
            }
        }

        [NonEvent]
        public void LocalForwardHostStarted(EventTraceActivity activity)
        {
            LocalForwardHostStarted(activity.Activity);
        }

        [Event(110, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardHostStarted(Guid activity)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(110, activity);
            }
        }

        [NonEvent]
        internal void LocalForwardHostStopped(EventTraceActivity activity)
        {
            LocalForwardHostStopped(activity.Activity);
        }

        [Event(111, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardHostStopped(Guid activity)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(111, activity);
            }
        }


        [NonEvent]
        public void LocalForwardHostStarting(EventTraceActivity activity)
        {
            LocalForwardHostStarting(activity.Activity);
        }

        [Event(112, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        void LocalForwardHostStarting(Guid activity)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(112, activity);
            }
        }


        [NonEvent]
        public void LocalForwardHostStopping(EventTraceActivity activity)
        {
            LocalForwardHostStopping(activity.Activity);
        }

        [Event(113, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        void LocalForwardHostStopping(Guid activity)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(113, activity);
            }
        }

        [NonEvent]
        internal void LocalForwardConfigUpdated(EventTraceActivity activity)
        {
            LocalForwardConfigUpdated(activity.Activity);
        }

        [Event(114, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardConfigUpdated(Guid activity)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(114, activity);
            }
        }


        [NonEvent]
        internal void LocalForwardBridgeConnectionStarted(EventTraceActivity bridgeActivity, TcpClient tcpClient, HybridConnectionClient hybridConnectionClient)
        {
            LocalForwardBridgeConnectionStarted(
                bridgeActivity.Activity,
                tcpClient.Client.LocalEndPoint.ToString(),
                hybridConnectionClient.Address.ToString());
        }

        [Event(120, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardBridgeConnectionStarted(Guid activity, string localEndpoint, string relayUri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(120, activity, localEndpoint, relayUri);
            }
        }

        internal void LocalForwardBridgeConnectionStopped(EventTraceActivity bridgeActivity, TcpClient tcpClient, HybridConnectionClient hybridConnectionClient)
        {
            LocalForwardBridgeConnectionStopped(
                bridgeActivity.Activity,
                tcpClient.Client.LocalEndPoint.ToString(),
                hybridConnectionClient.Address.ToString());
        }

        [Event(121, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardBridgeConnectionStopped(Guid activity, string localEndpoint, string relayUri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(40331, activity, localEndpoint, relayUri);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeConnectionFailed(EventTraceActivity bridgeActivity, Exception e)
        {
            LocalForwardBridgeConnectionFailed(bridgeActivity.Activity, e.GetType().FullName, e.Message, e.StackTrace);
        }

        [Event(122, Level = EventLevel.Error, Keywords = Keywords.LocalForward)]
        void LocalForwardBridgeConnectionFailed(Guid activity, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(122, activity, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeConnectionStarting(EventTraceActivity bridgeActivity, TcpClient tcpClient, HybridConnectionClient hybridConnectionClient)
        {
            LocalForwardBridgeConnectionStarting(bridgeActivity.Activity, tcpClient.Client.LocalEndPoint.ToString(), hybridConnectionClient.Address.ToString());
        }

        [Event(123, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        internal void LocalForwardBridgeConnectionStarting(Guid activity, string localAddress, string relayUri)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(123, activity, localAddress, relayUri);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeStarted(EventTraceActivity activity, IPAddress bindToAddress, LocalForward localForward)
        {
            LocalForwardBridgeStarted(activity.Activity, bindToAddress.ToString(), localForward.RelayName);
        }

        [Event(130, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardBridgeStarted(Guid guid, string bindToAddress, string relayName)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(130, guid, bindToAddress, relayName);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeStopped(EventTraceActivity activity, TcpLocalForwardBridge tcpLocalForwardBridge)
        {
            LocalForwardBridgeStopped(activity.Activity, tcpLocalForwardBridge.GetIpEndPoint()?.ToString(), tcpLocalForwardBridge.HybridConnectionClient?.Address.ToString());
        }

        [Event(131, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardBridgeStopped(Guid activity, string bindToAddress, string relayUri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(131, activity, bindToAddress, relayUri);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeStarting(EventTraceActivity activity, LocalForward localForward)
        {
            LocalForwardBridgeStarting(activity.Activity, localForward.RelayName);
        }

        [Event(132, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        void LocalForwardBridgeStarting(Guid activity, string relayName)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(132, activity, relayName);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeStopping(EventTraceActivity activity, TcpLocalForwardBridge tcpLocalForwardBridge)
        {
            LocalForwardBridgeStopping(activity.Activity, tcpLocalForwardBridge.GetIpEndPoint()?.ToString());
        }

        [Event(133, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        internal void LocalForwardBridgeStopping(Guid activity, string endpoint)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(133, activity, endpoint);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeFailedToStart(EventTraceActivity activity, LocalForward localForward, Exception e)
        {
            LocalForwardBridgeFailedToStart(activity.Activity, e.GetType().FullName, e.Message, e.StackTrace);
        }

        [Event(134, Level = EventLevel.Error, Keywords = Keywords.LocalForward)]
        void LocalForwardBridgeFailedToStart(Guid activity, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(134, activity, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardBridgeFailedToStop(EventTraceActivity activity, TcpLocalForwardBridge tcpLocalForwardBridge, Exception e)
        {
            LocalForwardBridgeFailedToStop(activity.Activity, e.GetType().FullName, e.Message, e.StackTrace);
        }

        [Event(135, Level = EventLevel.Error, Keywords = Keywords.LocalForward)]
        internal void LocalForwardBridgeFailedToStop(Guid activity, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(135, activity, exceptionName, exceptionMessage, stackTrace);
            }
        }


        [NonEvent]
        public void RemoteForwardBridgeStarted(EventTraceActivity activity, string uri)
        {
            RemoteForwardBridgeStarted(activity.Activity, uri);
        }

        [Event(140, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward)]
        void RemoteForwardBridgeStarted(Guid activity, string uri)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(140, activity, uri);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeStopped(EventTraceActivity activity, TcpRemoteForwardBridge tcpClientBridge)
        {
            RemoteForwardBridgeStopped(activity.Activity, tcpClientBridge.ToString());
        }

        [Event(141, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward)]
        internal void RemoteForwardBridgeStopped(Guid activity, string remoteForward)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(141, activity, remoteForward);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeStopping(EventTraceActivity activity, TcpRemoteForwardBridge tcpClientBridge)
        {
            RemoteForwardBridgeStopping(activity.Activity, tcpClientBridge.ToString());
        }

        [Event(142, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward)]
        void RemoteForwardBridgeStopping(Guid activity, string remoteForward)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(142, activity, remoteForward);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeFailedToStop(EventTraceActivity activity, Exception exception)
        {
            RemoteForwardBridgeFailedToStop(activity.Activity, exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(143, Level = EventLevel.Error, Keywords = Keywords.RemoteForward)]
        void RemoteForwardBridgeFailedToStop(Guid activity, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(143, activity, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeStarting(EventTraceActivity activity, RemoteForwardHost remoteForwardHost, RemoteForward remoteForward)
        {
            RemoteForwardBridgeStarting(activity.Activity, remoteForwardHost.ToString(), remoteForward.ToString());
        }

        [Event(144, Level = EventLevel.Verbose, Keywords = Keywords.RemoteForward)]
        void RemoteForwardBridgeStarting(Guid activity, string remoteForwardHost, string remoteForward)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(143, activity, remoteForwardHost, remoteForward);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeFailedToStart(EventTraceActivity activity, Uri hybridConnectionUri, Exception exception)
        {
            RemoteForwardBridgeFailedToStart(activity.Activity, hybridConnectionUri.ToString(), exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(145, Level = EventLevel.Error, Keywords = Keywords.RemoteForward)]
        void RemoteForwardBridgeFailedToStart(Guid activity, string hybridConnectionUri, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(143, activity, hybridConnectionUri, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeOnline(EventTraceActivity activity, Uri hybridConnectionUri, TcpRemoteForwardBridge tcpRemoteForwardBridge)
        {
            RemoteForwardBridgeOnline(activity.Activity, hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(146, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward)]
        internal void RemoteForwardBridgeOnline(Guid activity, string hybridConnectionUri, string remoteForward)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(146, activity, hybridConnectionUri, remoteForward);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeOffline(EventTraceActivity activity, Uri hybridConnectionUri, TcpRemoteForwardBridge tcpRemoteForwardBridge)
        {
            RemoteForwardBridgeOffline(activity.Activity, hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(147, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward)]
        internal void RemoteForwardBridgeOffline(Guid activity, string hybridConnectionUri, string remoteForward)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(147, activity, hybridConnectionUri, remoteForward);
            }
        }

        [NonEvent]
        internal void RemoteForwardBridgeConnecting(EventTraceActivity activity, Uri hybridConnectionUri, TcpRemoteForwardBridge tcpRemoteForwardBridge)
        {
            RemoteForwardBridgeConnecting(activity.Activity, hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(148, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward)]
        internal void RemoteForwardBridgeConnecting(Guid activity, string hybridConnectionUri, string remoteForward)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(148, activity, hybridConnectionUri, remoteForward);
            }
        }

        [NonEvent]
        internal void LocalForwardListenerStarted(EventTraceActivity activity, TcpListener tcpListener)
        {
            LocalForwardListenerStarted(activity.Activity, tcpListener.LocalEndpoint.ToString());
        }

        [Event(150, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardListenerStarted(Guid activity, string localEndpoint)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(150, activity, localEndpoint);
            }
        }

        [NonEvent]
        internal void LocalForwardListenerStopped(EventTraceActivity activity, TcpListener tcpListener)
        {
            LocalForwardListenerStopped(activity.Activity, tcpListener.LocalEndpoint.ToString());
        }

        [Event(151, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardListenerStopped(Guid activity, string localEndpoint)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(151, activity, localEndpoint);
            }
        }


        [NonEvent]
        internal void LocalForwardListenerStarting(EventTraceActivity activity, IPEndPoint listenEndpoint)
        {
            LocalForwardListenerStarting(activity.Activity, listenEndpoint.ToString());
        }


        [Event(152, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        internal void LocalForwardListenerStarting(Guid activity, string listenEndpoint)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(152, activity, listenEndpoint);
            }
        }

        [NonEvent]
        internal void LocalForwardListenerStopping(EventTraceActivity activity, IPEndPoint listenEndpoint)
        {
            LocalForwardListenerStopping(activity.Activity, listenEndpoint.ToString());
        }


        [Event(153, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        internal void LocalForwardListenerStopping(Guid activity, string listenEndpoint)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(153, activity, listenEndpoint);
            }
        }

        [NonEvent]
        internal void LocalForwardListenerStoppingFailed(EventTraceActivity activity, Exception ex)
        {
            LocalForwardListenerStoppingFailed(activity.Activity, ex.GetType().FullName, ex.Message, ex.StackTrace);
        }

        [Event(154, Level = EventLevel.Error, Keywords = Keywords.LocalForward)]
        internal void LocalForwardListenerStoppingFailed(Guid activity, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(154, activity, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardListenerStartFailed(EventTraceActivity activity, Exception ex)
        {
            LocalForwardListenerStartFailed(activity.Activity, ex.GetType().FullName, ex.Message, ex.StackTrace);
        }

        [Event(155, Level = EventLevel.Error, Keywords = Keywords.LocalForward)]
        internal void LocalForwardListenerStartFailed(Guid activity, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.RemoteForward))
            {
                this.WriteEventWithRelatedActivityId(155, activity, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardSocketAccepted(EventTraceActivity socketActivity, TcpClient socket)
        {
            LocalForwardSocketAccepted(socketActivity.Activity, socket.Client.LocalEndPoint.ToString());
        }

        [Event(160, Level = EventLevel.Verbose, Keywords = Keywords.LocalForward)]
        void LocalForwardSocketAccepted(Guid socketActivity, string socket)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(160, socketActivity, socket);
            }
        }

        [NonEvent]
        internal void LocalForwardSocketError(EventTraceActivity socketActivity, TcpClient socket, AggregateException exception)
        {
            LocalForwardSocketError(socketActivity.Activity, socket.Client.LocalEndPoint.ToString(), exception.GetType().FullName, exception.Message, exception.StackTrace);
        }

        [Event(161, Level = EventLevel.Warning, Keywords = Keywords.LocalForward)]
        void LocalForwardSocketError(Guid socketActivity, string socket, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Warning, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(161, socketActivity, socket, exceptionName, exceptionMessage, stackTrace);
            }
        }


        [NonEvent]
        internal void LocalForwardSocketClosed(EventTraceActivity socketActivity, TcpClient socket)
        {
            LocalForwardSocketClosed(socketActivity.Activity, socket.Client.LocalEndPoint.ToString());
        }

        [Event(162, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        internal void LocalForwardSocketClosed(Guid socketActivity, string socket)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(162, socketActivity, socket);
            }
        }

        [NonEvent]
        internal void LocalForwardSocketCloseFailed(EventTraceActivity socketActivity, TcpClient socket, Exception e)
        {
            LocalForwardSocketCloseFailed(socketActivity.Activity, socket.Client.LocalEndPoint.ToString(), e.GetType().FullName, e.Message, e.StackTrace);
        }

        [Event(163, Level = EventLevel.Warning, Keywords = Keywords.LocalForward)]
        void LocalForwardSocketCloseFailed(Guid socketActivity, string socket, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Warning, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(163, socketActivity, socket, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardSocketComplete(EventTraceActivity socketActivity, TcpClient socket)
        {
            LocalForwardSocketComplete(socketActivity.Activity, socket.Client.LocalEndPoint.ToString());
        }

        [Event(164, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        void LocalForwardSocketComplete(Guid socketActivity, string socket)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(164, socketActivity, socket);
            }
        }


        [NonEvent]
        internal void LocalForwardSocketAcceptLoopFailed(EventTraceActivity socketLoopActivity, Exception e)
        {
            LocalForwardSocketAcceptLoopFailed(socketLoopActivity.Activity, e.GetType().FullName, e.Message, e.StackTrace);
        }

        [Event(165, Level = EventLevel.Error, Keywords = Keywords.LocalForward)]
        void LocalForwardSocketAcceptLoopFailed(Guid socketLoopActivity, string exceptionName, string exceptionMessage, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(160, socketLoopActivity, exceptionName, exceptionMessage, stackTrace);
            }
        }

        [NonEvent]
        internal void LocalForwardConfigUpdating(EventTraceActivity activity, Config config1, Config config2)
        {
            LocalForwardConfigUpdating(activity.Activity);
        }

        [Event(166, Level = EventLevel.Informational, Keywords = Keywords.LocalForward)]
        internal void LocalForwardConfigUpdating(Guid activity)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.LocalForward))
            {
                this.WriteEventWithRelatedActivityId(166, activity);
            }
        }


        public class Keywords // This is a bitvector
        {
            public const EventKeywords RemoteForward = (EventKeywords)0x0001;
            public const EventKeywords LocalForward = (EventKeywords)0x0002;
        }
    }
}