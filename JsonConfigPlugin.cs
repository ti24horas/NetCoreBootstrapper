namespace Bootstrapper
{
    using Microsoft.Extensions.Configuration;

    using Newtonsoft.Json.Linq;

    public class JsonConfigPlugin
    {
        public void Configure(JObject token, ConfigurationBuilder builder)
        {
            if (token["$type"].Value<string>() != "json")
            {
                return;
            }

            builder.AddJsonFile(
                token.Value<string>("path"),
                token.Value<bool>("optional"),
                token.Value<bool>("reloadOnChange"));
        }
    }
}