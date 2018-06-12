// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace azbridge
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Azure.Relay.Bridge;
    using Microsoft.Azure.Relay.Bridge.Configuration;

    class Program
    {
        static void Main(string[] args)
        {
            CommandLineSettings.Run(args, Run);
        }

        static int Run(CommandLineSettings settings)
        {
            try
            {
                Config config = Config.LoadConfig(settings);
                if (config.LocalForward.Count == 0 &&
                     config.RemoteForward.Count == 0)
                {
                    CommandLineSettings.Help();
                    Console.WriteLine("You must specify at least one -L or -R forwarder.");
                    return 2;
                }

                Console.WriteLine("Press Ctrl+C to stop");
                SemaphoreSlim semaphore = new SemaphoreSlim(1);
                semaphore.Wait();

                Host host = new Host(config);
                host.Start();
                Console.CancelKeyPress += (e, a) => semaphore.Release();
                semaphore.Wait();
                host.Stop();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Configuration file not found:" + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return 0;
        }
    }
}
