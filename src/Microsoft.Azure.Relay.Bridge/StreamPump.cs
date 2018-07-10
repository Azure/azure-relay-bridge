// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class StreamPump
    {
        public static async Task RunAsync(Stream source, Stream target, Action shutdownAction, CancellationToken cancellationToken)
        {
            try
            {
                var buffer = new byte[65536];
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead;

                    bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    
                    if (bytesRead == 0)
                    {
                        shutdownAction?.Invoke();
                        return;
                    }
                    await target.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                }
            }
            catch (Exception e)
            {
                BridgeEventSource.Log.HandledExceptionAsWarning(source, e);
                throw;
            }
        }
    }
}