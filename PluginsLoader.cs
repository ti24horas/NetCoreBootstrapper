namespace Bootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    public class Runner
    {
        public void Run(JToken token, ILoggerFactory factory, IConfigurationRoot cfg)
        {
            var services = new ServiceCollection();
            services.AddSingleton(factory).AddSingleton(cfg);
            
            var logger = factory.CreateLogger<Runner>();

            foreach (dynamic p in token["plugins"])
            {
                logger.LogTrace($"loading loader {p.type.ToString()}");
                var type = Type.GetType((string)p.type);
                dynamic o = Activator.CreateInstance(type);

                //var configurer = Extensions.GetAction<JToken, IServiceCollection, IConfigurationRoot>(o);
                o.Configure(token, services, cfg);
                //configurer(token, services, cfg);
            }

            var container = services.BuildServiceProvider();
            var run = container.GetRequiredService<Action>();
            run();
        }
    }

    public class Loader
    {
        public void Configure(JToken token, IServiceCollection services, IConfigurationRoot cfg)
        {
            services.AddSingleton<Action>(
                s => () =>
                    {
                        var log = s.GetService<ILoggerFactory>();
                        log.CreateLogger("main").LogInformation("Hi, running");
                        Console.WriteLine("Running");
                    });
        }
    }

    public class ServicesLoader
    {
        public void Run(JToken section, ILoggerFactory factory, IConfigurationRoot cfg)
        {
            var runnerName = section["runner"]["$type"].Value<string>();

            dynamic runner = Activator.CreateInstance(Type.GetType(runnerName));

            runner.Run(section, factory, cfg);
        }
    }

    internal class PluginsLoader
    {
        public void RegisterPlugins(JToken section, List<object> registerer)
        {
            if (section.Path != "plugins")
            {
                return;
            }

            var o = (JArray)section;

            foreach (var item in o.OfType<JObject>())
            {
                var type = Type.GetType(item["$type"].Value<string>());

                if (type != null)
                {
                    registerer.Add(Activator.CreateInstance(type));
                }
            }
        }
    }
}