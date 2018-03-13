
namespace mshcmsvc
{
    using System;
    using System.IO;
    using System.ServiceProcess;
    using Microsoft.HybridConnectionManager;

    public partial class HybridConnectionService : ServiceBase
    {
        Host host;

        public HybridConnectionService()
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
