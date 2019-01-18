using System.Collections.Generic;

namespace Api.Gateway.Auth
{
    public class AuthenticationConfig
    {
        public string JwksResource { get; set;  }
        public int JwksTimeoutThreshold { get; set; }
        public IEnumerable<string> JwtValidIssuers { get; set; }
        public IEnumerable<string> JwtAllowedAudiences { get; set; }
    }
}