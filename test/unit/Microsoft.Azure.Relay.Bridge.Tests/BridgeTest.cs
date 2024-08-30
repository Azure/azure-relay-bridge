// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge.Test
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using Microsoft.Azure.Relay.Bridge.Tests;
    using Xunit;


    public class BridgeTest : IClassFixture<LaunchSettingsFixture>
    {
#if _WINDOWS
        private const string relayA1 = "a1.win";
        private const string relayA2 = "a2.win";
        private const string relayA3 = "a3.win";
        private const string relayHttp = "http.win";
#elif _LINUX
        private const string relayA1 = "a1.linux";
        private const string relayA2 = "a2.linux";
        private const string relayA3 = "a3.linux";
        private const string relayHttp = "http.linux";
#elif _OSX
        private const string relayA1 = "a1.osx";
        private const string relayA2 = "a2.osx";
        private const string relayA3 = "a3.osx";
        private const string relayHttp = "http.osx";
#endif

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
                RelayName = relayA1
            });
            cfg.RemoteForward.Add(new RemoteForward
            {
                Host = "127.0.97.2",
                HostPort = 29877,
                PortName = "test",
                RelayName = relayA1
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

        [Fact(Skip = "Unreliable")]
        public void TcpBridgeNoAuth()
        {
            // set up the bridge first
            Config cfg = new Config
            {
                AzureRelayConnectionString = Utilities.GetConnectionString()
            };
            cfg.LocalForward.Add(new LocalForward
            {
                BindAddress = "127.0.97.3",
                BindPort = 29876,
                PortName = "test",
                RelayName = relayA3,
                NoAuthentication = true
            });
            cfg.RemoteForward.Add(new RemoteForward
            {
                Host = "127.0.97.4",
                HostPort = 29877,
                PortName = "test",
                RelayName = relayA3
            });
            Host host = new Host(cfg);
            host.Start();

            try
            {
                // now try to use it
                var l = new TcpListener(IPAddress.Parse("127.0.97.4"), 29877);
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
                    s.Connect("127.0.97.3", 29876);
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
                RelayName = relayA2
            });
            cfg.RemoteForward.Add(new RemoteForward
            {
                Host = "127.0.97.2",
                HostPort = -29877,
                PortName = "testu",
                RelayName = relayA2
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

#if _LINUX
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
                RelayName = relayA2,
                PortName = "test"
            };
            var rf = new RemoteForward
            {
                LocalSocket = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
                RelayName = relayA2,
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

      
        [Fact]
        public async Task HttpBridge()
        {
            // set up the bridge first
            Config cfg = new Config
            {
                AzureRelayConnectionString = Utilities.GetConnectionString()
            };
            cfg.RemoteForward.Add(new RemoteForward
            {
                Host = "127.0.97.2",
                HostPort = 29877,
                PortName = "http",
                RelayName = relayHttp,
                Http = true
            });
            Host host = new Host(cfg);
            host.Start();

            try
            {
                RelayConnectionStringBuilder csb = new RelayConnectionStringBuilder(Utilities.GetConnectionString());
                var httpEndpoint = new UriBuilder(csb.Endpoint) { Scheme = "https", Port = 443, Path=relayHttp }.Uri;
                var httpSasToken = (await TokenProvider.CreateSharedAccessSignatureTokenProvider(csb.SharedAccessKeyName, csb.SharedAccessKey).GetTokenAsync(httpEndpoint.AbsoluteUri, TimeSpan.FromHours(1))).TokenString;

                using (var l = new HttpListener())
                {
                    l.Prefixes.Add("http://127.0.97.2:29877/");
                    l.Start();

                    var plainHandler = (Task<HttpListenerContext> t) =>
                    {
                        var c = t.Result;
                        using (var b = new StreamReader(c.Request.InputStream))
                        {
                            var text = b.ReadLine();
                            using (var w = new StreamWriter(c.Response.OutputStream))
                            {
                                w.WriteLine(text);
                                w.Flush();
                            }
                            c.Response.Close();
                        }
                    };

                    var localAuthHandler = (Task<HttpListenerContext> t) =>
                    {
                        var c = t.Result;
                        var auth = c.Request.Headers["Authorization"];
                        if (!auth.Equals("Bearer bearbear") )
                        {
                            c.Response.StatusCode = 401;
                            c.Response.Close();
                            return;
                        }
                        using (var b = new StreamReader(c.Request.InputStream))
                        {
                            var text = b.ReadLine();
                            using (var w = new StreamWriter(c.Response.OutputStream))
                            {
                                w.WriteLine(text);
                                w.Flush();
                            }
                            c.Response.Close();
                        }
                    };

                    var testMessage = "Hello!";

                    // with Authorization header
                    using (var c = new HttpClient())
                    {
                        c.DefaultRequestHeaders.Add("Authorization", httpSasToken);

                        // listen for exactly one request
                        _ = l.GetContextAsync().ContinueWith(plainHandler);

                        var r = await c.PostAsync(httpEndpoint, new StringContent(testMessage));
                        Assert.True(r.IsSuccessStatusCode);
                        var result = await r.Content.ReadAsStringAsync();
                        Assert.Equal(testMessage, result.Trim());
                        r.Dispose();

                        // listen for exactly one request
                        _ = l.GetContextAsync().ContinueWith(plainHandler);

                        var mtv = MediaTypeHeaderValue.Parse("application/cloudevents+json;charset=utf-8;foo=bar");
                        var r2 = await c.PostAsync(httpEndpoint, new StringContent(testMessage, mtv));
                        Assert.True(r2.IsSuccessStatusCode);
                        var result2 = await r2.Content.ReadAsStringAsync();
                        Assert.Equal(testMessage, result2.Trim());
                        r2.Dispose();
                    }
                    // with ServiceBusAuthorization header
                    using (var c = new HttpClient())
                    {
                        c.DefaultRequestHeaders.Add("ServiceBusAuthorization", httpSasToken);
                        c.DefaultRequestHeaders.Add("Authorization", "Bearer bearbear");

                        // listen for exactly one request
                        _ = l.GetContextAsync().ContinueWith(localAuthHandler);

                        var r = await c.PostAsync(httpEndpoint, new StringContent(testMessage));
                        Assert.True(r.IsSuccessStatusCode);
                        var result = await r.Content.ReadAsStringAsync();
                        Assert.Equal(testMessage, result.Trim());
                        r.Dispose();

                        // listen for exactly one request
                        _ = l.GetContextAsync().ContinueWith(localAuthHandler);

                        var mtv = MediaTypeHeaderValue.Parse("application/cloudevents+json;charset=utf-8;foo=bar");
                        var r2 = await c.PostAsync(httpEndpoint, new StringContent(testMessage, mtv));
                        Assert.True(r2.IsSuccessStatusCode);
                        var result2 = await r2.Content.ReadAsStringAsync();
                        Assert.Equal(testMessage, result2.Trim());
                        r2.Dispose();
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
