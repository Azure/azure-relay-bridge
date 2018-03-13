

namespace Microsoft.HybridConnectionManager.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.HybridConnectionManager.Configuration;
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
                        "         \"targets\" : [ " +
                        "           { \"connectionString\":\"Endpoint=sb://test\", \"hostName\":\"test1.example.com\", \"port\": 81 }," +
                        "           { \"connectionString\":\"Endpoint=sb://test\", \"hostName\":\"test2.example.com\", \"port\": 82 }," +
                        "           { \"connectionString\":\"Endpoint=sb://test\", \"hostName\":\"test3.example.com\", \"port\": 83 }" +
                        "         ]," +
                        "         \"listeners\" : [ " +
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
            ConnectionConfig connectionConfig = null;

            using (var reader = new StreamReader(configFileName, true))
            {
                var jr = new JsonTextReader(reader);
                JObject config = JObject.Load(jr);
                if (config.ContainsKey("connections"))
                {
                    connectionConfig = config["connections"].ToObject<ConnectionConfig>();
                }
            }


            Assert.Equal(3, connectionConfig.Listeners.Count);
            Assert.Equal(3, connectionConfig.Targets.Count);

            Assert.Equal("Endpoint=sb://test", connectionConfig.Listeners[0].ConnectionString);
            Assert.Equal("test1.example.com", connectionConfig.Listeners[0].HostName);
            Assert.Equal(81, connectionConfig.Listeners[0].Port);
            Assert.Equal("Endpoint=sb://test", connectionConfig.Listeners[1].ConnectionString);
            Assert.Equal("test2.example.com", connectionConfig.Listeners[1].HostName);
            Assert.Equal(82, connectionConfig.Listeners[1].Port);
            Assert.Equal("Endpoint=sb://test", connectionConfig.Listeners[2].ConnectionString);
            Assert.Equal("test3.example.com", connectionConfig.Listeners[2].HostName);
            Assert.Equal(83, connectionConfig.Listeners[2].Port);

            Assert.Equal("Endpoint=sb://test", connectionConfig.Targets[0].ConnectionString);
            Assert.Equal("test1.example.com", connectionConfig.Targets[0].HostName);
            Assert.Equal(81, connectionConfig.Targets[0].Port);
            Assert.Equal("Endpoint=sb://test", connectionConfig.Targets[1].ConnectionString);
            Assert.Equal("test2.example.com", connectionConfig.Targets[1].HostName);
            Assert.Equal(82, connectionConfig.Targets[1].Port);
            Assert.Equal("Endpoint=sb://test", connectionConfig.Targets[2].ConnectionString);
            Assert.Equal("test3.example.com", connectionConfig.Targets[2].HostName);
            Assert.Equal(83, connectionConfig.Targets[2].Port);

            File.Delete(configFileName);
        }

        [Fact]
        public void ConfigSaveLoadFileTest()
        {
            var configFileName = CreateConfig();
            ConnectionConfig connectionConfig =  Host.LoadConfig(configFileName);

            connectionConfig.Targets.Add(new ConnectionTarget
            {
                ConnectionString = "sb://foo",
                HostName = "localhost",
                Port = 8081
            });
            Host.SaveConfig(configFileName, connectionConfig);

            ConnectionConfig connectionConfig2 = Host.LoadConfig(configFileName);
            Assert.NotNull(connectionConfig2.Targets.FirstOrDefault(i => i.ConnectionString.Equals("sb://foo")));


        }
    }
}
