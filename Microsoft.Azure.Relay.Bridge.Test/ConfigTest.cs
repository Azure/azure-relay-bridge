

namespace Microsoft.Azure.Relay.Bridge.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class ConfigTest
    {

        string CreateConfig()
        {
            string myFile = Path.GetTempFileName();
            using (var myFileStream = File.OpenWrite(myFile))
            {
                using (var textWriter = new StreamWriter(myFileStream, Encoding.UTF8))
                {
                    textWriter.Write(
                       @"{" +
                        "   \"connections\" : {" +
                        "         \"RemoteForwarders\" : [ " +
                        "           { \"connectionString\":\"Endpoint=sb://test\", \"hostName\":\"test1.example.com\", \"port\": 81 }," +
                        "           { \"connectionString\":\"Endpoint=sb://test\", \"hostName\":\"test2.example.com\", \"port\": 82 }," +
                        "           { \"connectionString\":\"Endpoint=sb://test\", \"hostName\":\"test3.example.com\", \"port\": 83 }" +
                        "         ]," +
                        "         \"LocalForwarders\" : [ " +
                        "           { \"connectionString\":\"Endpoint=sb://test\", \"hostName\":\"test1.example.com\", \"port\": 81 }," +
                        "           { \"connectionString\":\"Endpoint=sb://test\", \"hostName\":\"test2.example.com\", \"port\": 82 }," +
                        "           { \"connectionString\":\"Endpoint=sb://test\", \"hostName\":\"test3.example.com\", \"port\": 83 }" +
                        "         ]," +
                        "   }" +
                        "}");
                }
            }
            return myFile;
        }

        [Fact]
        public void ConfigFileTest()
        {
            var configFileName = CreateConfig();
            Config connectionConfig = null;

            using (var reader = new StreamReader(configFileName, true))
            {
                var jr = new JsonTextReader(reader);
                JObject config = JObject.Load(jr);
                if (config.ContainsKey("connections"))
                {
                    connectionConfig = config["connections"].ToObject<Config>();
                }
            }


            Assert.Equal(3, connectionConfig.LocalForward.Count);
            Assert.Equal(3, connectionConfig.RemoteForward.Count);

            Assert.Equal("Endpoint=sb://test", connectionConfig.LocalForward[0].ConnectionString.ToString());
            Assert.Equal("test1.example.com", connectionConfig.LocalForward[0].Host);
            Assert.Equal(81, connectionConfig.LocalForward[0].HostPort);
            Assert.Equal("Endpoint=sb://test", connectionConfig.LocalForward[1].ConnectionString.ToString());
            Assert.Equal("test2.example.com", connectionConfig.LocalForward[1].Host);
            Assert.Equal(82, connectionConfig.LocalForward[1].HostPort);
            Assert.Equal("Endpoint=sb://test", connectionConfig.LocalForward[2].ConnectionString.ToString());
            Assert.Equal("test3.example.com", connectionConfig.LocalForward[2].Host);
            Assert.Equal(83, connectionConfig.LocalForward[2].HostPort);

            Assert.Equal("Endpoint=sb://test", connectionConfig.RemoteForward[0].ConnectionString.ToString());
            Assert.Equal("test1.example.com", connectionConfig.RemoteForward[0].Host);
            Assert.Equal(81, connectionConfig.RemoteForward[0].Port);
            Assert.Equal("Endpoint=sb://test", connectionConfig.RemoteForward[1].ConnectionString.ToString());
            Assert.Equal("test2.example.com", connectionConfig.RemoteForward[1].Host);
            Assert.Equal(82, connectionConfig.RemoteForward[1].Port);
            Assert.Equal("Endpoint=sb://test", connectionConfig.RemoteForward[2].ConnectionString.ToString());
            Assert.Equal("test3.example.com", connectionConfig.RemoteForward[2].Host);
            Assert.Equal(83, connectionConfig.RemoteForward[2].Port);

            File.Delete(configFileName);
        }

        [Fact]
        public void ConfigSaveLoadFileTest()
        {
            var configFileName = CreateConfig();
            Config connectionConfig =  Host.LoadConfig(configFileName);

            connectionConfig.RemoteForward.Add(new RemoteForward
            {
                // ConnectionString = "sb://foo",
                Host = "localhost",
                Port = 8081
            });
            Host.SaveConfig(configFileName, connectionConfig);

            Config connectionConfig2 = Host.LoadConfig(configFileName);
            Assert.NotNull(connectionConfig2.RemoteForward.FirstOrDefault(i => i.ConnectionString.Equals("sb://foo")));


        }
    }
}
