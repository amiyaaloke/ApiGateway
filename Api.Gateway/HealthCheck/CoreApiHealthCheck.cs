using Api.Gateway.Configuration;

namespace Api.Gateway.HealthCheck
{
    public class CoreApiHealthCheck : AbstractHttpHealthCheck
    {
        private readonly IConfigurationSettings _config;

        public CoreApiHealthCheck(IConfigurationSettings config)
        {
            _config = config;
        }

        protected override string Url()
        {
            return _config.CoreApiHealthCheckUrl;
        }
    }
}
