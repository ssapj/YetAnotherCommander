using System;
using System.IO;
using System.Reflection;

namespace ssapj.YetAnotherCommander
{
    internal class ReadConfig
    {
        private static readonly Lazy<ReadConfig> lazy = new Lazy<ReadConfig>(() => new ReadConfig());

        public static ReadConfig Instance => lazy.Value;

        public Config Config { get; }

        private ReadConfig()
        {
            var bytes = File.ReadAllBytes(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "config.json"));
            this.Config = Utf8Json.JsonSerializer.Deserialize<Config>(bytes);
        }

    }
}

