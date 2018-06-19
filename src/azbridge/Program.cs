// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace azbridge
{
    using System;
    using System.IO;
    using System.Threading;
    using Microsoft.Azure.Relay.Bridge;
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using System.Diagnostics;
#if USE_MDT_EVENTSOURCE
    using Microsoft.Diagnostics.Tracing;
#else
    using System.Diagnostics.Tracing;
#endif
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    partial class Program
    {
        static ILogger logger = null;

        static void Main(string[] args)
        {
            CommandLineSettings.Run(args, (c)=>Run(c,args));
        }

        static int Run(CommandLineSettings settings, string[] args)
        {
            try
            {
#if NET462
                if (settings.ServiceInstall.HasValue && settings.ServiceInstall.Value)
                {
                    ServiceLauncher.InstallService();
                    return 0;
                }
                else if (settings.ServiceUninstall.HasValue && settings.ServiceUninstall.Value)
                {
                    ServiceLauncher.UninstallService();
                    return 0;
                }
                else if (settings.ServiceRun.HasValue && settings.ServiceRun.Value)
                {
                    ServiceLauncher.Run(args);
                    return 0;
                }
#endif
                Config config = Config.LoadConfig(settings);
                if (config.LocalForward.Count == 0 &&
                     config.RemoteForward.Count == 0)
                {
                    CommandLineSettings.Help();
                    Console.WriteLine("You must specify at least one -L or -R forwarder.");
                    return 2;
                }

                var loggerFactory = new LoggerFactory();
                if (!settings.Quiet.HasValue || !settings.Quiet.Value)
                {
                    loggerFactory.AddConsole();
                }
                logger = loggerFactory.CreateLogger("azbridge");
                DiagnosticListener.AllListeners.Subscribe(new SubscriberObserver(logger));


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
