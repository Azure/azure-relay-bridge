﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Linq;
    using System.Globalization;
    using System.Diagnostics.Tracing;
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
            System.Diagnostics.Tracing.EventSource
    {
        static DiagnosticSource diags = new DiagnosticListener(typeof(BridgeEventSource).Namespace);
        public static readonly BridgeEventSource Log = new BridgeEventSource();
        // Prevent additional instances other than RelayEventSource.Log

        private BridgeEventSource() : base() { }

        [NonEvent]
        public static EventTraceActivity NewActivity(string name)
        {
            var activity = new Activity(name);
            return new EventTraceActivity(activity);
        }

        [NonEvent]
        public static EventTraceActivity NewActivity(string name, EventTraceActivity parent)
        {
            var activity = new Activity(name);
            activity.SetParentId(parent.DiagnosticsActivity.Id);
            return new EventTraceActivity(activity);
        }


        [NonEvent]
        public ArgumentException Argument(string paramName, string message, object source = null,
            EventLevel level = EventLevel.Error)
        {
            diags.Write(nameof(ArgumentException), new DiagnosticsRecord { Level = EventLevel.Error, Info = new { paramName, message, source } });
            return this.ThrowingException(new ArgumentException(message, paramName), source, level);
        }


        [NonEvent]
        public ArgumentNullException ArgumentNull(string paramName, object source = null,
            EventLevel level = EventLevel.Error)
        {
            diags.Write(nameof(ArgumentNullException), new DiagnosticsRecord { Level = EventLevel.Error, Info = new { paramName, source } });
            return this.ThrowingException(new ArgumentNullException(paramName), source, level);
        }


        [NonEvent]
        public ArgumentOutOfRangeException ArgumentOutOfRange(string paramName, string message,
            object source = null, EventLevel level = EventLevel.Error)
        {
            diags.Write(nameof(ArgumentOutOfRangeException), new DiagnosticsRecord { Level = EventLevel.Error, Info = new { paramName, message, source } });
            return this.ThrowingException(new ArgumentOutOfRangeException(paramName, message), source,
                level);
        }

        // Not the actual event definition since we're using object and Exception types
        [NonEvent]
        public void HandledExceptionAsError(object source, Exception exception)
        {
            diags.Write(nameof(HandledExceptionAsError), new DiagnosticsRecord { Level = EventLevel.Error, Info = new { source,exception.Message } });
            this.HandledExceptionAsError(CreateSourceString(source), ExceptionToString(exception));
        }

        // Not the actual event definition since we're using object and Exception types
        [NonEvent]
        public void HandledExceptionAsInformation(object source, Exception exception)
        {
            diags.Write(nameof(HandledExceptionAsInformation), new DiagnosticsRecord { Level = EventLevel.Informational, Info = new { source,exception.Message } });
            this.HandledExceptionAsInformation(CreateSourceString(source), ExceptionToString(exception));
        }

        // Not the actual event definition since we're using object and Exception types
        [NonEvent]
        public void HandledExceptionAsWarning(object source, Exception exception)
        {
            diags.Write(nameof(HandledExceptionAsWarning), new DiagnosticsRecord { Level = EventLevel.Warning, Info = new { source,exception.Message } });
            this.HandledExceptionAsWarning(CreateSourceString(source), ExceptionToString(exception));
        }


        [NonEvent]
        public TException ThrowingException<TException>(TException exception, object source = null,
            EventLevel level = EventLevel.Error)
            where TException : Exception
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

            // This allows "throw ServiceBusEventSource.Log.ThrowingException(..."
            return exception;
        }

        [NonEvent]
        public static string CreateSourceString(object source)
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
        public void ThrowingExceptionError(string source, string exception)
        {
            // The IsEnabled() check is in the [NonEvent] Wrapper method
            this.WriteEvent(1, source, exception);
        }

        [Event(2, Level = EventLevel.Warning, Message = "Throwing an Exception: {0} {1}")]
        public void ThrowingExceptionWarning(string source, string exception)
        {
            // The IsEnabled() check is in the [NonEvent] Wrapper method
            this.WriteEvent(2, source, exception);
        }

        [Event(3, Level = EventLevel.Informational, Message = "Throwing an Exception: {0} {1}")]
        public void ThrowingExceptionInfo(string source, string exception)
        {
            // The IsEnabled() check is in the [NonEvent] Wrapper method
            this.WriteEvent(3, source, exception);
        }

        [Event(4, Level = EventLevel.Error, Message = "Exception Handled: {0} {1}")]
        public void HandledExceptionAsError(string source, string exception)
        {
            this.WriteEvent(4, source, exception);
        }

        [Event(5, Message = "Exception Handled: {0} {1}")]
        public void HandledExceptionAsInformation(string source, string exception)
        {
            this.WriteEvent(5, source, exception);
        }

        [Event(6, Level = EventLevel.Warning, Message = "Exception Handled: {0} {1}")]
        public void HandledExceptionAsWarning(string source, string exception)
        {
            this.WriteEvent(6, source, exception);
        }

        [NonEvent]
        public void RemoteForwardHostStart(EventTraceActivity eventTraceActivity)
        {
            diags.Write(nameof(RemoteForwardHostStart), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            RemoteForwardHostStart();
        }

        [Event(100, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward, Message = "Remote forward host started")]
        public void RemoteForwardHostStart()
        {
            this.WriteEvent(100);
        }

        [NonEvent]
        public void RemoteForwardHostStop(EventTraceActivity eventTraceActivity)
        {
            diags.Write(nameof(RemoteForwardHostStop), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            RemoteForwardHostStop();
        }

        [Event(101, Level = EventLevel.Informational, Keywords = Keywords.RemoteForward, Message = "Remote forward host stopped")]
        public void RemoteForwardHostStop()
        {
            this.WriteEvent(101);
        }

        [NonEvent]
        public void RemoteForwardHostStarting(EventTraceActivity eventTraceActivity)
        {
            diags.Write(nameof(RemoteForwardHostStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            RemoteForwardHostStarting();
        }

        [Event(102, Level = EventLevel.Verbose,
               Keywords = Keywords.RemoteForward, Message = "Remote forward host is starting")]
        public void RemoteForwardHostStarting()
        {

            this.WriteEvent(102);
        }

        [NonEvent]
        public void RemoteForwardHostStopping(EventTraceActivity eventTraceActivity)
        {
            diags.Write(nameof(RemoteForwardHostStopping), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            RemoteForwardHostStopping();
        }

        [Event(103, Level = EventLevel.Verbose,
               Keywords = Keywords.RemoteForward, Message = "Remote forward host is stopping")]
        public void RemoteForwardHostStopping()
        {
            this.WriteEvent(103);
        }

        [NonEvent]
        public void RemoteForwardHostStartFailure(EventTraceActivity eventTraceActivity, Exception exception)
        {
            diags.Write(nameof(RemoteForwardHostStartFailure), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { exception.Message } });
            RemoteForwardHostStartFailure(exception.GetType().FullName, exception.Message);
        }

        [Event(104,

            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward host failed to start with exception {0}, message \"{1}\"")]
        public void RemoteForwardHostStartFailure(string exceptionName, string exceptionMessage)
        {
            this.WriteEvent(104, exceptionName, exceptionMessage);
        }

        [NonEvent]
        public void RemoteForwardConfigUpdating(EventTraceActivity eventTraceActivity, Config configOld, Config configNew)
        {
            diags.Write(nameof(RemoteForwardConfigUpdating), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { configOld, configNew } });
            RemoteForwardConfigUpdating();
        }

        [Event(105,

            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward host configuration is being updated")]
        public void RemoteForwardConfigUpdating()
        {
            this.WriteEvent(105);
        }

        [NonEvent]
        public void RemoteForwardConfigUpdated(EventTraceActivity eventTraceActivity)
        {
            diags.Write(nameof(RemoteForwardConfigUpdated), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            RemoteForwardConfigUpdated();
        }


        [Event(106,

            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward host configuration has been updated")]
        public void RemoteForwardConfigUpdated()
        {
            this.WriteEvent(106);
        }

        [NonEvent]
        public void LocalForwardHostStart(EventTraceActivity eventTraceActivity)
        {
            diags.Write(nameof(LocalForwardHostStart), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            LocalForwardHostStart();
        }

        [Event(110,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward host has been started")]
        public void LocalForwardHostStart()
        {
            this.WriteEvent(110);
        }

        [NonEvent]
        public void LocalForwardHostStop(EventTraceActivity eventTraceActivity)
        {
            diags.Write(nameof(LocalForwardHostStop), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            LocalForwardHostStop();
        }

        [Event(111,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward host has been stopped")]
        public void LocalForwardHostStop()
        {
            this.WriteEvent(111);
        }


        [NonEvent]
        public void LocalForwardHostStarting(EventTraceActivity eventTraceActivity)
        {
            diags.Write(nameof(LocalForwardHostStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            LocalForwardHostStarting();
        }

        [Event(112,
            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward host starting")]
        public void LocalForwardHostStarting()
        {
            this.WriteEvent(112);
        }


        [NonEvent]
        public void LocalForwardHostStopping(EventTraceActivity eventTraceActivity)
        {
            diags.Write(nameof(LocalForwardHostStopping), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            LocalForwardHostStopping();
        }

        [Event(113,

            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward host stopping")]
        void LocalForwardHostStopping()
        {
            this.WriteEvent(113);
        }

        [NonEvent]
        public void LocalForwardConfigUpdated(EventTraceActivity eventTraceActivity)
        {
            diags.Write(nameof(LocalForwardConfigUpdated), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity });
            LocalForwardConfigUpdated();
        }

        [Event(114,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward configuration updated")]
        public void LocalForwardConfigUpdated()
        {
            this.WriteEvent(114);
        }


        [NonEvent]
        public void LocalForwardBridgeConnectionStart(EventTraceActivity bridgeActivity, string localEndpoint, HybridConnectionClient hybridConnectionClient)
        {
            diags.Write(nameof(LocalForwardBridgeConnectionStart), new DiagnosticsRecord { Level = EventLevel.Informational, Info = new { bridgeActivity, localEndpoint, hybridConnectionClient } });

            LocalForwardBridgeConnectionStart(
                localEndpoint,
                hybridConnectionClient.Address.ToString());
        }

        [Event(120,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge connection started {0} via {1}")]
        public void LocalForwardBridgeConnectionStart(string localEndpoint, string relayUri)
        {
            this.WriteEvent(120, localEndpoint, relayUri);
        }

        [NonEvent]

        public void LocalForwardBridgeConnectionStop(EventTraceActivity bridgeActivity, string endpointInfo, HybridConnectionClient hybridConnectionClient)
        {
            diags.Write(nameof(LocalForwardBridgeConnectionStop), new DiagnosticsRecord { Level = EventLevel.Informational, Info = new { bridgeActivity, tcpClient = endpointInfo, hybridConnectionClient } });
            LocalForwardBridgeConnectionStop(
                endpointInfo,
                hybridConnectionClient.Address.ToString());
        }

        [Event(121,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge connection stopped {0} via {1}")]
        public void LocalForwardBridgeConnectionStop(string localEndpoint, string relayUri)
        {
            this.WriteEvent(40331, localEndpoint, relayUri);
        }

        [NonEvent]
        public void LocalForwardBridgeConnectionFailed(EventTraceActivity bridgeActivity, Exception exception)
        {
            diags.Write(nameof(LocalForwardBridgeConnectionFailed), new DiagnosticsRecord { Level = EventLevel.Informational, Info = new { bridgeActivity,exception.Message } });

            LocalForwardBridgeConnectionFailed(exception.GetType().FullName, exception.Message);
        }

        [Event(122,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge connection failed with exception {0}, message {1}")]
        public void LocalForwardBridgeConnectionFailed(string exceptionName, string exceptionMessage)
        {
            this.WriteEvent(122, exceptionName, exceptionMessage);
        }

        [NonEvent]
        public void LocalForwardBridgeConnectionStarting(EventTraceActivity bridgeActivity, string localEndpoint, HybridConnectionClient hybridConnectionClient)
        {
            diags.Write(nameof(LocalForwardBridgeConnectionStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Info = new { bridgeActivity, localEndpoint, hybridConnectionClient } });

            LocalForwardBridgeConnectionStarting(localEndpoint, hybridConnectionClient.Address.ToString());
        }

        [Event(123,

            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge connection starting {0} via {1}")]
        public void LocalForwardBridgeConnectionStarting(string localAddress, string relayUri)
        {
            this.WriteEvent(123, localAddress, relayUri);
        }

        [NonEvent]
        public void LocalForwardBridgeStart(EventTraceActivity eventTraceActivity, IPAddress bindToAddress, LocalForward localForward)
        {
            diags.Write(nameof(LocalForwardBridgeStart), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { bindToAddress, localForward } });
            LocalForwardBridgeStart(bindToAddress.ToString(), localForward.RelayName);
        }

        [Event(130,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge started binding {0} to {1}")]
        public void LocalForwardBridgeStart(string bindToAddress, string relayName)
        {
            this.WriteEvent(130, bindToAddress, relayName);
        }

        [NonEvent]
        public void LocalForwardBridgeStop(EventTraceActivity eventTraceActivity, string tcpLocalForwardBridge, string relayUri)
        {
            diags.Write(nameof(LocalForwardBridgeStop), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { tcpLocalForwardBridge } });
            LocalForwardBridgeStop(tcpLocalForwardBridge, relayUri);
        }

        [Event(131,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge stopped; bound {0} to {1}")]
        public void LocalForwardBridgeStop(string bindToAddress, string relayUri)
        {
            this.WriteEvent(131, bindToAddress, relayUri);
        }

        [NonEvent]
        public void LocalForwardBridgeStarting(EventTraceActivity eventTraceActivity, LocalForward localForward)
        {
            diags.Write(nameof(LocalForwardBridgeStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { localForward } });
            LocalForwardBridgeStarting(localForward.RelayName);
        }

        [Event(132,

            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge starting for '{0}'")]
        public void LocalForwardBridgeStarting(string relayName)
        {
            this.WriteEvent(132, relayName);
        }

        [NonEvent]
        public void LocalForwardBridgeStopping(EventTraceActivity eventTraceActivity, string localForwardBridge)
        {
            diags.Write(nameof(LocalForwardBridgeStopping), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { localForwardBridge = localForwardBridge } });
            LocalForwardBridgeStopping(localForwardBridge);
        }

        [Event(133,

            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge stopping for '{0}'")]
        public void LocalForwardBridgeStopping(string endpoint)
        {
            this.WriteEvent(133, endpoint);
        }

        [NonEvent]
        public void LocalForwardBridgeStartFailure(EventTraceActivity eventTraceActivity, LocalForward localForward, Exception exception)
        {
            diags.Write(nameof(LocalForwardBridgeStartFailure), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { localForward, exception.Message } });
            LocalForwardBridgeStartFailure(exception.GetType().FullName, exception.Message);
        }

        [Event(134,

            Level = EventLevel.Error,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge failed to start with exception {0}, message {1}")]
        public void LocalForwardBridgeStartFailure(string exceptionName, string exceptionMessage)
        {
            this.WriteEvent(134, exceptionName, exceptionMessage);
        }

        [NonEvent]
        public void LocalForwardBridgeStopFailure(EventTraceActivity eventTraceActivity, string localForwardBridge, Exception exception)
        {
            diags.Write(nameof(LocalForwardBridgeStopFailure), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { localForwardBridge = localForwardBridge,exception.Message } });
            LocalForwardBridgeStopFailure(exception.GetType().FullName, exception.Message);
        }

        [Event(135,

            Level = EventLevel.Error,
            Keywords = Keywords.LocalForward,
            Message = "Local forward bridge failed to stop with exception {0}, message {1}")]
        public void LocalForwardBridgeStopFailure(string exceptionName, string exceptionMessage)
        {
            this.WriteEvent(135, exceptionName, exceptionMessage);
        }


        [NonEvent]
        public void RemoteForwardBridgeStart(EventTraceActivity eventTraceActivity, string uri)
        {
            diags.Write(nameof(RemoteForwardBridgeStart), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { uri } });
            RemoteForwardBridgeStart(uri);
        }

        [Event(140,

            Level = EventLevel.Informational,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge started for {0}")]
        public void RemoteForwardBridgeStart(string uri)
        {
            this.WriteEvent(140, uri);
        }

        [NonEvent]
        public void RemoteForwardBridgeStop(EventTraceActivity eventTraceActivity, string clientBridge)
        {
            diags.Write(nameof(RemoteForwardBridgeStop), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { clientBridge } });
            RemoteForwardBridgeStop(clientBridge);
        }

        [Event(141,

            Level = EventLevel.Informational,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge stopped for {0}")]
        public void RemoteForwardBridgeStop(string remoteForward)
        {
            this.WriteEvent(141, remoteForward);
        }

        [NonEvent]
        public void RemoteForwardBridgeStopping(EventTraceActivity eventTraceActivity, string clientBridge)
        {
            diags.Write(nameof(RemoteForwardBridgeStopping), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { clientBridge } });

            RemoteForwardBridgeStopping(clientBridge);
        }

        [Event(142,

            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge stopping for '{0}'")]
        public void RemoteForwardBridgeStopping(string remoteForward)
        {
            this.WriteEvent(142, remoteForward);
        }

        [NonEvent]
        public void RemoteForwardBridgeStopFailure(EventTraceActivity eventTraceActivity, Exception exception)
        {
            diags.Write(nameof(RemoteForwardBridgeStopFailure), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { exception.Message } });

            RemoteForwardBridgeStopFailure(exception.GetType().FullName, exception.Message);
        }

        [Event(143,

            Level = EventLevel.Error,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge failed to stop with exception {0}, message {1}")]
        public void RemoteForwardBridgeStopFailure(string exceptionName, string exceptionMessage)
        {
            this.WriteEvent(143, exceptionName, exceptionMessage);
        }

        [NonEvent]
        public void RemoteForwardBridgeStarting(EventTraceActivity eventTraceActivity, RemoteForwardHost remoteForwardHost, RemoteForward remoteForward)
        {
            diags.Write(nameof(RemoteForwardBridgeStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { remoteForward } });
            RemoteForwardBridgeStarting(remoteForwardHost.ToString(), remoteForward.ToString());
        }

        [Event(144,

            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge starting for '{0}' on '{1}'")]
        public void RemoteForwardBridgeStarting(string remoteForwardHost, string remoteForward)
        {
            this.WriteEvent(143, remoteForwardHost, remoteForward);
        }

        [NonEvent]
        public void RemoteForwardBridgeStartFailure(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, Exception exception)
        {
            diags.Write(nameof(RemoteForwardBridgeStartFailure), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri,exception.Message } });
            RemoteForwardBridgeStartFailure(hybridConnectionUri.ToString(), exception.GetType().FullName, exception.Message);
        }

        [Event(145,

            Level = EventLevel.Error,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge failed to start with exception {0}, message {1}")]
        public void RemoteForwardBridgeStartFailure(string hybridConnectionUri, string exceptionName, string exceptionMessage)
        {
            this.WriteEvent(143, hybridConnectionUri, exceptionName, exceptionMessage);
        }

        [NonEvent]
        public void RemoteForwardBridgeOnline(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, object tcpRemoteForwardBridge)
        {
            diags.Write(nameof(RemoteForwardBridgeOnline), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri, tcpRemoteForwardBridge } });
            RemoteForwardBridgeOnline(hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(146,

            Level = EventLevel.Informational,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge online {0} {1}")]
        public void RemoteForwardBridgeOnline(string hybridConnectionUri, string remoteForward)
        {
            this.WriteEvent(146, hybridConnectionUri, remoteForward);
        }

        [NonEvent]
        public void RemoteForwardBridgeOffline(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, object tcpRemoteForwardBridge)
        {
            diags.Write(nameof(RemoteForwardBridgeOffline), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri, tcpRemoteForwardBridge } });
            RemoteForwardBridgeOffline(hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(147,

            Level = EventLevel.Warning,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge offline {0} {1}")]
        public void RemoteForwardBridgeOffline(string hybridConnectionUri, string remoteForward)
        {
            this.WriteEvent(147, hybridConnectionUri, remoteForward);
        }

        [NonEvent]
        public void RemoteForwardBridgeConnecting(EventTraceActivity eventTraceActivity, Uri hybridConnectionUri, object tcpRemoteForwardBridge)
        {
            diags.Write(nameof(RemoteForwardBridgeConnecting), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { hybridConnectionUri, tcpRemoteForwardBridge } });

            RemoteForwardBridgeConnecting(hybridConnectionUri.ToString(), tcpRemoteForwardBridge.ToString());
        }

        [Event(148,

            Level = EventLevel.Informational,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward bridge connecting {0} {1}")]
        public void RemoteForwardBridgeConnecting(string hybridConnectionUri, string remoteForward)
        {
            this.WriteEvent(148, hybridConnectionUri, remoteForward);
        }

        [NonEvent]
        public void LocalForwardListenerStart(EventTraceActivity eventTraceActivity, string localEndpoint)
        {
            diags.Write(nameof(LocalForwardListenerStart), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { localEndpoint } });

            LocalForwardListenerStart(localEndpoint);
        }

        [Event(150,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener started {0}")]
        public void LocalForwardListenerStart(string localEndpoint)
        {
            this.WriteEvent(150, localEndpoint);
        }

        [NonEvent]
        public void LocalForwardListenerStop(EventTraceActivity eventTraceActivity, string localEndpoint)
        {
            diags.Write(nameof(LocalForwardListenerStop), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { localEndpoint } });

            LocalForwardListenerStop(localEndpoint);
        }

        [Event(151,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener stopped {0}")]
        public void LocalForwardListenerStop(string localEndpoint)
        {
            this.WriteEvent(151, localEndpoint);
        }


        [NonEvent]
        public void LocalForwardListenerStarting(EventTraceActivity eventTraceActivity, string listenEndpoint)
        {
            diags.Write(nameof(LocalForwardListenerStarting), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { listenEndpoint } });

            LocalForwardListenerStarting(listenEndpoint);
        }


        [Event(152,

            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener starting {0}")]
        public void LocalForwardListenerStarting(string listenEndpoint)
        {
            this.WriteEvent(152, listenEndpoint);
        }

        [NonEvent]
        public void LocalForwardListenerStopping(EventTraceActivity eventTraceActivity, string localEndpoint)
        {
            diags.Write(nameof(LocalForwardListenerStopping), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { localEndpoint } });
            LocalForwardListenerStopping(localEndpoint);
        }


        [Event(153,

            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener stopping {0}")]
        public void LocalForwardListenerStopping(string listenEndpoint)
        {
            this.WriteEvent(153, listenEndpoint);
        }

        [NonEvent]
        public void LocalForwardListenerStoppingFailed(EventTraceActivity eventTraceActivity, Exception exception)
        {
            diags.Write(nameof(LocalForwardListenerStoppingFailed), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { exception.Message } });
            LocalForwardListenerStoppingFailed(exception.GetType().FullName, exception.Message);
        }

        [Event(154,

            Level = EventLevel.Error,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener failed to stop with exception {0}, message {1}")]
        public void LocalForwardListenerStoppingFailed(string exceptionName, string exceptionMessage)
        {
            this.WriteEvent(154, exceptionName, exceptionMessage);
        }

        [NonEvent]
        public void LocalForwardListenerStartFailed(EventTraceActivity eventTraceActivity, Exception exception)
        {
            diags.Write(nameof(LocalForwardListenerStartFailed), new DiagnosticsRecord { Level = EventLevel.Error, Activity = eventTraceActivity.Activity, Info = new { exception.Message } });

            LocalForwardListenerStartFailed(exception.GetType().FullName, exception.Message);
        }

        [Event(155,

            Level = EventLevel.Error,
            Keywords = Keywords.LocalForward,
            Message = "Local forward listener failed to start with exception {0}, message {1}")]
        public void LocalForwardListenerStartFailed(string exceptionName, string exceptionMessage)
        {
            this.WriteEvent(155, exceptionName, exceptionMessage);
        }

        [NonEvent]
        public void LocalForwardSocketAccepted(EventTraceActivity eventTraceActivity, string localEndpoint)
        {
            diags.Write(nameof(LocalForwardSocketAccepted), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity, Info = new { localEndpoint } });

            LocalForwardSocketAccepted(localEndpoint);
        }

        [Event(160,

            Level = EventLevel.Verbose,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket accepted {0}")]
        public void LocalForwardSocketAccepted(string socket)
        {
            this.WriteEvent(160, socket);
        }

        [NonEvent]
        public void LocalForwardSocketError(EventTraceActivity eventTraceActivity, string endpoint, AggregateException exception)
        {
            diags.Write(nameof(LocalForwardSocketError), new DiagnosticsRecord { Level = EventLevel.Warning, Activity = eventTraceActivity.Activity, Info = new { endpoint,exception.Message } });
            LocalForwardSocketError(endpoint, exception.GetType().FullName, exception.Message);
        }

        [Event(161,

            Level = EventLevel.Warning,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket error {0} {1} {2} {3}")]
        public void LocalForwardSocketError(string socket, string exceptionName, string exceptionMessage)
        {
            this.WriteEvent(161, socket, exceptionName, exceptionMessage);
        }


        [NonEvent]
        public void LocalForwardSocketClosed(EventTraceActivity eventTraceActivity, string socket)
        {
            diags.Write(nameof(LocalForwardSocketClosed), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { socket } });

            LocalForwardSocketClosed("");
        }

        [Event(162,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket closed {0}")]
        public void LocalForwardSocketClosed(string socket)
        {
            this.WriteEvent(162, socket);
        }

        [NonEvent]
        public void LocalForwardSocketCloseFailed(EventTraceActivity eventTraceActivity, string socket, Exception exception)
        {
            diags.Write(nameof(LocalForwardSocketCloseFailed), new DiagnosticsRecord { Level = EventLevel.Warning, Activity = eventTraceActivity.Activity, Info = new { socket,exception.Message } });
            LocalForwardSocketCloseFailed(socket, exception.GetType().FullName, exception.Message);
        }

        [Event(163,

            Level = EventLevel.Warning,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket close failed {0} {1} {2} {3}")]
        public void LocalForwardSocketCloseFailed(string socket, string exceptionName, string exceptionMessage)
        {
            this.WriteEvent(163, socket, exceptionName, exceptionMessage);
        }

        [NonEvent]
        public void LocalForwardSocketComplete(EventTraceActivity eventTraceActivity, string endpoint)
        {
            diags.Write(nameof(LocalForwardSocketComplete), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { endpoint } });
            LocalForwardSocketComplete(endpoint);
        }

        [Event(164,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket complete {0}")]
        public void LocalForwardSocketComplete(string socket)
        {
            this.WriteEvent(164, socket);
        }


        [NonEvent]
        public void LocalForwardSocketAcceptLoopFailed(EventTraceActivity eventTraceActivity, Exception exception)
        {
            diags.Write(nameof(LocalForwardSocketAcceptLoopFailed), new DiagnosticsRecord { Level = EventLevel.Warning, Activity = eventTraceActivity.Activity, Info = new { exception.Message } });
            LocalForwardSocketAcceptLoopFailed(exception.GetType().FullName, exception.Message);
        }

        [Event(165,

            Level = EventLevel.Error,
            Keywords = Keywords.LocalForward,
            Message = "Local forward socket accept loop failed {0} {1} {2}")]
        public void LocalForwardSocketAcceptLoopFailed(string exceptionName, string exceptionMessage)
        {
            this.WriteEvent(160, exceptionName, exceptionMessage);
        }

        [NonEvent]
        public void LocalForwardConfigUpdating(EventTraceActivity eventTraceActivity, Config configOld, Config configNew)
        {
            diags.Write(nameof(LocalForwardConfigUpdating), new DiagnosticsRecord { Level = EventLevel.Informational, Activity = eventTraceActivity.Activity, Info = new { configOld, configNew } });
            LocalForwardConfigUpdating();
        }

        [Event(166,

            Level = EventLevel.Informational,
            Keywords = Keywords.LocalForward,
            Message = "Local forward config updating")]
        public void LocalForwardConfigUpdating()
        {
            this.WriteEvent(166);
        }

        [Event(167,

            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward HTTP request forwarded: {0}")]
        public void RemoteForwardHttpRequestForwarded(string message)
        {
            this.WriteEvent(167, message);
        }

        [NonEvent]
        public void RemoteForwardHttpRequestForwarded(EventTraceActivity eventTraceActivity, string message)
        {
            diags.Write(nameof(RemoteForwardHttpRequestForwarded), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            RemoteForwardHttpRequestForwarded(message);
        }

        [Event(168,
            Level = EventLevel.Verbose,
            Keywords = Keywords.RemoteForward,
            Message = "Remote forward socket accepted: {0}")]
        public void RemoteForwardTcpSocketAccepted(string message)
        {
            this.WriteEvent(168, message);
        }

        [NonEvent]
        public void RemoteForwardTcpSocketAccepted(EventTraceActivity eventTraceActivity, string message)
        {
            diags.Write(nameof(RemoteForwardTcpSocketAccepted), new DiagnosticsRecord { Level = EventLevel.Verbose, Activity = eventTraceActivity.Activity });
            RemoteForwardTcpSocketAccepted(message);
        }


        public class Keywords // This is a bitvector
        {
            public const EventKeywords RemoteForward = (EventKeywords)0x0001;
            public const EventKeywords LocalForward = (EventKeywords)0x0002;
        }


    }
}