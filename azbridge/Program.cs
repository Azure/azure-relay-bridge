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
            CommandLineSettings.Run = Program.Run;
            CommandLineApplication.Execute<CommandLineSettings>(args);
        }

        static async Task<int> Run(CommandLineSettings settings)
        {
            try
            {
                Console.WriteLine("Press Ctrl+C to stop");
                Config config = Config.LoadConfig(settings);

                SemaphoreSlim semaphore = new SemaphoreSlim(1);
                semaphore.Wait();

                Host host = new Host(settings.ConfigFile);
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
            return 0;
        }
    }
}
