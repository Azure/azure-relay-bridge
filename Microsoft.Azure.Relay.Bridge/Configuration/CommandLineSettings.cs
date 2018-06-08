// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class CommandLineSettings
    {
        [Option(CommandOptionType.NoValue, LongName = "addhosts")]
        public bool? AddHosts { get; set; }
        [Option(CommandOptionType.NoValue, LongName = "cleanhosts")]
        public bool? CleanHosts { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "b")]
        public string BindAddress { get; set; }
        [Option(CommandOptionType.NoValue, ShortName = "C")]
        public bool? Compression { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "E")]
        public Uri EndpointUri { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "F")]
        public string ConfigFile { get; set; }
        [Option(CommandOptionType.NoValue, ShortName = "g")]
        public bool? GatewayPorts { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "K")]
        public string SharedAccessKeyName { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "k")]
        public string SharedAccessKey { get; set; }
        [Option(CommandOptionType.MultipleValue, ShortName = "L")]
        public IEnumerable<string> LocalForward { get; set; }
        [Option(CommandOptionType.MultipleValue, ShortName = "o")]
        public IEnumerable<string> Option { get; set; }
        [Option(CommandOptionType.NoValue, ShortName = "q")]
        public bool? Quiet { get; set; }
        [Option(CommandOptionType.MultipleValue, ShortName = "R")]
        public IEnumerable<string> RemoteForward { get; set; }
        [Option(CommandOptionType.SingleValue, ShortName = "S")]
        public string Signature { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "x")]
        public string ConnectionString { get; internal set; }

        [Option(CommandOptionType.NoValue, ShortName = "v")]
        public bool Verbose { get; internal set; }


        public static void Run(string[] args, Func<CommandLineSettings, int> callback)
        {
            var app = new CommandLineApplication<CommandLineSettings>(true);
            app.ModelFactory = () => new CommandLineSettings();
            app.Conventions.UseDefaultConventions();
            app.Parse(args);                                    
            callback(app.Model);
        }
    }
}
