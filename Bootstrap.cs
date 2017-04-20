namespace Bootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class Bootstrap
    {
        public static void Run(string filename)
        {
            var mainFile = LoadToken(GetObject(filename)) as JObject;

            var lf = new LoggerFactory();

            var plugins = new List<dynamic>();
            
            var pluginLoader = new PluginsLoader();
            var serviceLoader = new ServicesLoader();
            var mainCfg = new ConfigurationBuilder();

            pluginLoader.RegisterPlugins(mainFile.Property("plugins").Value, plugins);
            
            var remaining = (from x in plugins
                             select new { plugin = x }).ToList();

            foreach (var p in mainFile.Properties().Where(a => !new[] { "plugins", "services" }.Contains(a.Name)))
            {
                var done = remaining.Select(s => new { done = (bool)s.plugin.Configure(p.Value, lf, mainCfg), s.plugin }).ToList();
                foreach (var item in done)
                {
                    Console.WriteLine($"plugin {item.plugin.GetType().Name} has {(item.done ? "done" : "not done")}");
                }

                remaining = done.Where(s => !s.done).Select(s => new { s.plugin }).ToList();
            }

            serviceLoader.Run(mainFile["services"], lf, mainCfg.Build());
            lf.Dispose();
        }

        private static JToken LoadToken(JToken o)
        {
            if (o.Type != JTokenType.Object)
            {
                return o;
            }

            foreach (var sub in o as JObject)
            {
                if (sub.Value.Type == JTokenType.Object)
                {
                    o[sub.Key] = LoadToken(sub.Value as JObject);
                }
            }

            var prop = o.First as JProperty;
            if (prop == null || prop.Name != "$include")
            {
                return o;
            }
            var result = GetObject(prop.Value.ToString());
            if (result.Type == JTokenType.Object)
            {
                return LoadToken(result as JObject);
            }
            return result;
        }

        private static JToken GetObject(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
            {
                return JToken.Load(new JsonTextReader(new StreamReader(fs)));
            }
        }
    }
}