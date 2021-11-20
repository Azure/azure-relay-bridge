// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge.Test
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using Microsoft.Azure.Relay.Bridge.Tests;
    using Xunit;

    public class BridgeTest : IClassFixture<LaunchSettingsFixture>
    {
        readonly LaunchSettingsFixture launchSettingsFixture;

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
                PortName = "test",
                RelayName = "a1"
            });
            cfg.RemoteForward.Add(new RemoteForward
            {
                Host = "127.0.97.2",
                HostPort = 29877,
                PortName = "test",
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

                using (var s = new TcpClient())
                {
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
                }

                l.Stop();
            }
            finally
            {
                host.Stop();
            }
        }

        [Fact]
        public void UdpBridge()
        {
            // set up the bridge first
            Config cfg = new Config
            {
                AzureRelayConnectionString = Utilities.GetConnectionString()
            };
            cfg.LocalForward.Add(new LocalForward
            {
                BindAddress = "127.0.97.1",
                BindPort = -29876,
                PortName = "testu",
                RelayName = "a2"
            });
            cfg.RemoteForward.Add(new RemoteForward
            {
                Host = "127.0.97.2",
                HostPort = -29877,
                PortName = "testu",
                RelayName = "a2"
            });
            Host host = new Host(cfg);
            host.Start();

            try
            {
                // now try to use it
                using (var l = new UdpClient(new IPEndPoint(IPAddress.Parse("127.0.97.2"), 29877)))
                {
                    l.ReceiveAsync().ContinueWith(async (t) =>
                    {
                        var c = t.Result;
                        var stream = c.Buffer;
                        using (var mr = new MemoryStream(stream))
                        {
                            using (var b = new StreamReader(mr))
                            {
                                var text = b.ReadLine();
                                using (var mw = new MemoryStream())
                                {
                                    using (var w = new StreamWriter(mw))
                                    {
                                        w.WriteLine(text);
                                        w.Flush();
                                        await l.SendAsync(mw.GetBuffer(), (int)mw.Length, c.RemoteEndPoint);
                                    }
                                }
                            }
                        }
                    });

                    using (var s = new UdpClient())
                    {
                        s.Connect("127.0.97.1", 29876);
                        using (MemoryStream mw = new MemoryStream())
                        {
                            using (var w = new StreamWriter(mw))
                            {
                                w.WriteLine("Hello!");
                                w.Flush();
                                s.Send(mw.GetBuffer(), (int)mw.Length);

                                IPEndPoint addr = null;
                                var buf = s.Receive(ref addr);
                                using (var mr = new MemoryStream(buf))
                                {
                                    using (var b = new StreamReader(mr))
                                    {
                                        Assert.Equal("Hello!", b.ReadLine());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                host.Stop();
            }
        }

#if !_WINDOWS
        [Fact(Skip="Unreliable")]
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
                RelayName = "a2",
                PortName = "test"
            };
            var rf = new RemoteForward
            {
                LocalSocket = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
                RelayName = "a2",
                PortName = "test"
            };
            cfg.LocalForward.Add(lf);
            cfg.RemoteForward.Add(rf);
            Host host = new Host(cfg);
            host.Start();

            try
            {
                // now try to use it
                using (var l = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP))
                {
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

                    using (var s = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP))
                    {
                        s.Connect(new UnixDomainSocketEndPoint(lf.BindLocalSocket));
                        using (var ns = new NetworkStream(s))
                        {
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
                        }
                        s.Close(0);
                        l.Close(0);
                    }
                }
            }
            finally
            {
                host.Stop();
            }
        }
#endif

        [Fact(Skip = "Unreliable")]
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

                using (var s = new TcpClient())
                {
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
                }
            }
            finally
            {
                host.Stop();
            }
        }
    }
}
