
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
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                host = new Host();
                host.Start(args);
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
