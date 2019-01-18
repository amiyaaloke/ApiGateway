using Microsoft.Extensions.Configuration;

namespace Api.Gateway.Configuration
{
    public class ConfigurationSettings : IConfigurationSettings
    {
        private readonly IConfiguration _config;

        public ConfigurationSettings(IConfiguration config)
        {
            _config = config;
        }

        public string CoreApiHealthCheckUrl { get => _config["CoreApiHealthCheckUrl"]; }
    }
}
