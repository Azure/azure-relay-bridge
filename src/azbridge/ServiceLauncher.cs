// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if _WINDOWS
namespace azbridge
{
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Threading;
    using System.Threading.Tasks;

    static class ServiceLauncher
    {
        const string ServiceName = "azbridgesvc";
        const string DisplayName = "Azure Relay Bridge Service";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        internal static async Task RunAsync(CommandLineSettings settings)
        {
            string svcConfigFileName =
                (Environment.OSVersion.Platform == PlatformID.Unix) ?
                    $"/etc/azbridge/azbridge_config.svc.yml" :
             Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                 $"Microsoft\\Azure Relay Bridge\\azbridge_config.svc.yml");

            if (File.Exists(svcConfigFileName))
            {
                settings.ConfigFile = svcConfigFileName;
            }
            else
            {
                svcConfigFileName = Path.Combine(AppContext.BaseDirectory, "azbridge_config.svc.yml");
                if (File.Exists(svcConfigFileName))
                {
                    settings.ConfigFile = svcConfigFileName;
                }
            }
            Config config = Config.LoadConfig(settings);

            LogLevel logLevel = LogLevel.Error;
            if (!settings.Quiet.HasValue || !settings.Quiet.Value)
            {
                if (config.LogLevel != null)
                {
                    switch (config.LogLevel.ToUpper())
                    {
                        case "QUIET":
                            logLevel = LogLevel.None;
                            break;
                        case "FATAL":
                            logLevel = LogLevel.Critical;
                            break;
                        case "ERROR":
                            logLevel = LogLevel.Error;
                            break;
                        case "INFO":
                            logLevel = LogLevel.Information;
                            break;
                        case "VERBOSE":
                            logLevel = LogLevel.Trace;
                            break;
                        case "DEBUG":
                        case "DEBUG1":
                        case "DEBUG2":
                        case "DEBUG3":
                            logLevel = LogLevel.Debug;
                            break;
                    }
                }
            }
            else
            {
                logLevel = LogLevel.None;
            }
            var loggerFactory = new LoggerFactory();
            if (!string.IsNullOrEmpty(config.LogFileName))
            {
                loggerFactory.AddFile(config.LogFileName, logLevel);
            }
            var logger = loggerFactory.CreateLogger("azbridge");
            DiagnosticListener.AllListeners.Subscribe(new SubscriberObserver(logger));

            IHost host = Host.CreateDefaultBuilder()
                .UseWindowsService(options =>
                {
                    options.ServiceName = ServiceName;
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(logger);
                    services.AddSingleton(config);
                    services.AddHostedService<RelayBridgeService>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    // See: https://github.com/dotnet/runtime/issues/47303
                    logging.AddConfiguration(
                        context.Configuration.GetSection("Logging"));
                })
                .Build();

            await host.RunAsync();
        }

        /// <summary>
        /// Installs the service
        /// </summary>
        internal static void InstallService()
        {
            if (IsInstalled())
                return;
            var filePath = Path.Combine(AppContext.BaseDirectory, "azbridge.exe");
            ShellExecute(Environment.SystemDirectory + @"\sc.exe", "create \"" + ServiceName + "\" binPath= \"" + filePath + " -svc\" start= auto DisplayName= \"" + DisplayName + "\"");
        }

        internal static void UninstallService()
        {
            if (!IsInstalled())
                return;

            ShellExecute(Environment.SystemDirectory + @"\sc.exe", "delete \"" + ServiceName + "\"");
        }

        public static void ShellExecute(string command, string parameters)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(command, parameters)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            if ( process.ExitCode != 0 )
            {
                throw new Exception($"Process failed with exit code {process.ExitCode}");
            }
        }

        internal static void StartService()
        {
            if (!IsInstalled())
                return;
#if _WINDOWS
#pragma warning disable CA1416 // Validate platform compatibility
            using (ServiceController controller =
                new ServiceController(ServiceName))
            {
                if (controller.Status != ServiceControllerStatus.Running)
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running,
                        TimeSpan.FromSeconds(10));
                }
            }
#pragma warning restore CA1416 // Validate platform compatibility
#else
             return true;
#endif
        }

        internal static bool IsInstalled()
        {
#if _WINDOWS
#pragma warning disable CA1416 // Validate platform compatibility
            using (ServiceController controller = new ServiceController(ServiceName))
            {
                try
                {
                    ServiceControllerStatus status = controller.Status;
                }
                catch
                {
                    return false;
                }
                return true;
            }
#pragma warning restore CA1416 // Validate platform compatibility
#else
             return true;
#endif
        }

        internal static bool IsRunning()
        {
#if _WINDOWS
#pragma warning disable CA1416 // Validate platform compatibility

            using (var controller = new ServiceController(ServiceName))
            {
                if (!IsInstalled())
                    return false;
                return (controller.Status == ServiceControllerStatus.Running);
            }
#pragma warning restore CA1416 // Validate platform compatibility
#else
             return true;
#endif
        }


        internal static void StopService()
        {
#if _WINDOWS
#pragma warning disable CA1416 // Validate platform compatibility
            if (!IsInstalled())
                return;
            using (ServiceController controller =
                new ServiceController(ServiceName))
            {
                try
                {
                    if (controller.Status != ServiceControllerStatus.Stopped)
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped,
                             TimeSpan.FromSeconds(10));
                    }
                }
                catch
                {
                    throw;
                }
            }
        }
#pragma warning restore CA1416 // Validate platform compatibility
#endif
    }
}
#endif