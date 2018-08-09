
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

            try
            {
                // now try to use it
                var l = new TcpListener(IPAddress.Parse("127.0.97.2"), 29877);
                l.Start();
                l.AcceptTcpClientAsync().ContinueWith((t) =>
                {
                    var c = t.Result;
                    var stream = c.GetStream();
                    using (var b = new StreamReader(stream))
                    {
                        var text = b.ReadLine();
                        using (var w = new StreamWriter(stream))
                        {
                            w.WriteLine(text);
                            w.Flush();
                        }
                    }
                });


                var s = new TcpClient();
                s.Connect("127.0.97.1", 29876);
                var sstream = s.GetStream();
                using (var w = new StreamWriter(sstream))
                {
                    w.WriteLine("Hello!");
                    w.Flush();
                    using (var b = new StreamReader(sstream))
                    {
                        Assert.Equal("Hello!", b.ReadLine());
                    }
                }

                s.Dispose();
                l.Stop();
            }
            finally
            {
                host.Stop();
            }
        }

#if !NETFRAMEWORK
        [Fact]
        public void SocketBridge()
        {
            // not yet supported on Windows.
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return;

            // set up the bridge first
            Config cfg = new Config
            {
                AzureRelayConnectionString = Utilities.GetConnectionString(),
                ExitOnForwardFailure = true
            };
            var lf = new LocalForward
            {
                BindLocalSocket = Path.Combine(Path.GetTempPath(),Path.GetRandomFileName()),
                RelayName = "a2"
            };
            var rf = new RemoteForward
            {
                LocalSocket = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
                RelayName = "a2"
            };
            cfg.LocalForward.Add(lf);
            cfg.RemoteForward.Add(rf);
            Host host = new Host(cfg);
            host.Start();


            try
            {
                // now try to use it
                var l = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                l.Bind(new UnixDomainSocketEndPoint(rf.LocalSocket));
                l.Listen(5);

                l.AcceptAsync().ContinueWith((t) =>
                {
                    var c = t.Result;
                    using (var b = new StreamReader(new NetworkStream(c)))
                    {
                        var text = b.ReadLine();
                        using (var w = new StreamWriter(new NetworkStream(c)))
                        {
                            w.WriteLine(text);
                            w.Flush();
                        }
                    }
                });


                var s = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                s.Connect(new UnixDomainSocketEndPoint(lf.BindLocalSocket));
                var ns = new NetworkStream(s);
                using (var w = new StreamWriter(ns))
                {
                    w.WriteLine("Hello!");
                    w.Flush();
                    using (var b = new StreamReader(ns))
                    {
                        string line = b.ReadLine();
                        Assert.Equal("Hello!", line);
                    }
                }
                s.Close(0);
                l.Close(0);
            }
            finally
            {
                host.Stop();
            }
        }
#endif

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

            try
            {
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

            }
            finally
            {
                host.Stop();
            }
        }
                                    
    }
}
