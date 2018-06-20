#if NET462
namespace azbridge
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
#if USE_MDT_EVENTSOURCE
using Microsoft.Diagnostics.Tracing;
#else
    using System.Diagnostics.Tracing;
#endif
    using System.IO;
    using System.ServiceProcess;
    using System.Threading;
    using Microsoft.Azure.Relay.Bridge;
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using Microsoft.Extensions.Logging;

    public partial class RelayBridgeService : ServiceBase
    {
        Host host;
        ILogger logger = null;

        public RelayBridgeService()
        {
            InitializeComponent();
        }

        public Host Host { get => host; }

        protected override void OnStart(string[] args)
        {
            CommandLineSettings.Run(args, Run);
        }

        int Run(CommandLineSettings settings)
        {
            string svcConfigFileName =
                (Environment.OSVersion.Platform == PlatformID.Unix) ?
                    $"/etc/azbridge/azbridge_config.svc.yml" :
             Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
                 $"Mirosoft\\Azure Relay Bridge\\azbridge_config.svc.yml");

            if (File.Exists(svcConfigFileName))
            {
                settings.ConfigFile = svcConfigFileName;
            }
            Config config = Config.LoadConfig(settings);
            var loggerFactory = new LoggerFactory();
            if (!settings.Quiet.HasValue || !settings.Quiet.Value)
            {
                // add file logging support here
                // loggerFactory.AddFile();
            }
            logger = loggerFactory.CreateLogger("azbridgesvc");
            DiagnosticListener.AllListeners.Subscribe(new SubscriberObserver(logger));
                
            host = new Host(config);
            host.Start();
            return 0;
        }

        protected override void OnStop()
        {
            try
            {
                host?.Stop();
            }
            finally
            {
                host = null;
            }
        }
    }
                    
    
}
#endif