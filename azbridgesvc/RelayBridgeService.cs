
namespace mshcmsvc
{
    using System;
    using System.IO;
    using System.ServiceProcess;
    using Microsoft.Azure.Relay.Bridge;

    public partial class RelayBridgeService : ServiceBase
    {
        Host host;

        public RelayBridgeService()
        {
            InitializeComponent();
            host = new Host(null);
        }

        public Host Host { get => host; }

        protected override void OnStart(string[] args)
        {
            try
            {
                host.Start();
            }
            catch (Exception e)
            {
                this.ExitCode = e.HResult;
                throw;
            }
        }

        protected override void OnStop()
        {
            host.Stop();
        }
    }
}
