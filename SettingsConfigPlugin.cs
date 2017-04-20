namespace Bootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    public class LoggingSettingsPlugin
    {
        public bool Configure(JToken token, ILoggerFactory lf, ConfigurationBuilder builder)
        {
            
            if (token.Path != "logging")
            {
                return false;
            }

            
            var simple = token["simple"];
            var console = simple["console"];
            if (console.Value<bool>("enabled"))
            {
                lf.AddConsole((LogLevel)Enum.Parse(typeof(LogLevel), console.Value<string>("level")));
            }

            return true;
        }
    }

    public class SettingsConfigPlugin
    {
        public bool Configure(JToken token, ILoggerFactory lf, ConfigurationBuilder builder)
        {
            if (token.Path != "settings")
            {
                return false;
            }

            var types = token["plugins"].Value<JArray>();// [.Value<JObject>().Property("plugins");

            var objects = types.OfType<JObject>().Select(p => Activator.CreateInstance(Type.GetType(p.Value<string>("$type")))).ToList();

            //var methods = objects.Select(p => Extensions.GetAction<JObject, ConfigurationBuilder>(p)).ToList();


            foreach (dynamic del in objects)
            {
                foreach (var i in token["load"].Value<JArray>().OfType<JObject>())
                {
                    del.Configure(i, builder);
                }
            }
            return true;
        }
    }
}