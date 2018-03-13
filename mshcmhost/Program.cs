// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace mshcmhost
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLine;
    using Microsoft.HybridConnectionManager;
    using Microsoft.HybridConnectionManager.Configuration;

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineSettings>(args)
                .WithParsed<CommandLineSettings>(opts => Run(opts).GetAwaiter().GetResult());
        }

        static async Task Run(CommandLineSettings settings)
        {
            try
            {
                Console.WriteLine("Press Ctrl+C to stop");

                SemaphoreSlim semaphore = new SemaphoreSlim(1);
                semaphore.Wait();

                Host host = new Host(settings.ConfigurationFile);
                host.Start();
                Console.CancelKeyPress += (e, a) => semaphore.Release();
                await semaphore.WaitAsync();
                host.Stop();
            }
            catch(FileNotFoundException e)
            {
                Console.WriteLine("Configuration file not found:" + e.Message);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
