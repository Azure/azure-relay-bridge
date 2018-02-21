
namespace mshcmhost
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.HybridConnectionManager;

    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run(args).Wait();
        }

        async Task Run(string[] args)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(1);
            semaphore.Wait();

            Host host = new Host();
            host.Start(args);
            Console.CancelKeyPress += (e, a) => semaphore.Release();
            await semaphore.WaitAsync();
            host.Stop();
        }
    }
}
