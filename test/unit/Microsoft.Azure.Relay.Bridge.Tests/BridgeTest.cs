
namespace Microsoft.Azure.Relay.Bridge.Test
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using Xunit;

    public class BridgeTest : IClassFixture<LaunchSettingsFixture>
    {
        private readonly LaunchSettingsFixture launchSettingsFixture;

        public BridgeTest(LaunchSettingsFixture launchSettingsFixture)
        {
            this.launchSettingsFixture = launchSettingsFixture;
        }

        [Fact]
        public void TcpBridge()
        {
            // set up the bridge first
            Config cfg = new Config
            {
                AzureRelayConnectionString = Utilities.GetConnectionString()
            };
            cfg.LocalForward.Add(new LocalForward
            {
                BindAddress = "127.0.97.1",
                BindPort = 29876,
                RelayName = "a1"
            });
            cfg.RemoteForward.Add(new RemoteForward
            {
                Host = "127.0.97.2",
                HostPort = 29877,
                RelayName = "a1"
            });
            Host host = new Host(cfg);
            host.Start();

            // now try to use it
            var l = new TcpListener(IPAddress.Parse("127.0.97.2"), 29877);
            l.Start();
            l.AcceptTcpClientAsync().ContinueWith((t) =>
            {
                var c = t.Result;
                using (var b = new StreamReader(c.GetStream()))
                {
                    var text = b.ReadLine();
                    using (var w = new StreamWriter(c.GetStream()))
                    {
                        w.WriteLine(text);
                        w.Flush();
                    }
                }
            });


            var s = new TcpClient();
            s.Connect("127.0.97.1", 29876);
            using (var w = new StreamWriter(s.GetStream()))
            {
                w.WriteLine("Hello!");
                w.Flush();
                using (var b = new StreamReader(s.GetStream()))
                {
                    Assert.Equal("Hello!", b.ReadLine());
                }
            }

            s.Dispose();
            l.Stop();
            host.Stop();
        }

        [Fact(Skip = "true")]
        public void TcpBridgeBadListener()
        {
            // set up the bridge first
            Config cfg = new Config
            {
                AzureRelayConnectionString = Utilities.GetConnectionString()
            };
            cfg.LocalForward.Add(new LocalForward
            {
                BindAddress = "127.0.97.1",
                BindPort = 29876,
                RelayName = "a1"
            });
            cfg.RemoteForward.Add(new RemoteForward
            {
                Host = "127.0.97.2",
                HostPort = 29877,
                RelayName = "a1"
            });
            Host host = new Host(cfg);
            host.Start();

            // now try to use it
            var l = new TcpListener(IPAddress.Parse("127.0.97.2"), 29877);
            l.Start();
            l.AcceptTcpClientAsync().ContinueWith((t) =>
            {
                t.Result.Client.Close(0);
                l.Stop();
            });


            var s = new TcpClient();
            s.Connect("127.0.97.1", 29876);
            s.NoDelay = true;
            s.Client.Blocking = true;
            using (var w = s.GetStream())
            {
                byte[] bytes = new byte[1024 * 1024];
                Assert.Throws<IOException>(() =>
                {
                    for (int i = 0; i < 5; i++)
                    {
                        w.Write(bytes, 0, bytes.Length);
                    }
                });
            }
            s.Dispose();
            host.Stop();
        }
                                    
    }
}
