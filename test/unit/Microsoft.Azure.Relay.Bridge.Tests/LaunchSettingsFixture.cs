using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Azure.Relay.Bridge.Tests
{
    public class LaunchSettingsFixture : IDisposable
    {
        public LaunchSettingsFixture()
        {
            string filename = "Properties\\launchSettings.json";
            if (File.Exists(filename))
            {
                using (var file = File.OpenText(filename))
                {
                    var reader = new JsonTextReader(file);
                    var jObject = JObject.Load(reader);

                    var variables = jObject
                        .GetValue("profiles")
                        //select a proper profile here
                        .SelectMany(profiles => profiles.Children())
                        .SelectMany(profile => profile.Children<JProperty>())
                        .Where(prop => prop.Name == "environmentVariables")
                        .SelectMany(prop => prop.Value.Children<JProperty>())
                        .ToList();

                    foreach (var variable in variables)
                    {
                        Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                    }
                }
            }
        }

        public void Dispose()
        {
            // ... clean up
        }
    }
}
