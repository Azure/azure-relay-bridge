// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System.Threading.Tasks;

    interface IRemoteForwarder
    {
        string PortName { get; }
        Task HandleConnectionAsync(HybridConnectionStream hybridConnectionStream);
    }
}