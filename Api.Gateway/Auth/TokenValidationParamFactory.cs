using System;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Api.Gateway.Auth
{
    public class TokenValidationParamFactory
    {
        public static TokenValidationParameters Create(IConfiguration configuration)
        {
            var settingsSection = configuration.GetSection("Authentication");
            var settings = settingsSection.Get<AuthenticationConfig>();

            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuers = settings.JwtValidIssuers,
                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudiences = settings.JwtAllowedAudiences,
                // Validate the token expiry
                ValidateLifetime = true,
                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.FromSeconds(settings.JwksTimeoutThreshold),
                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                    SecurityKeysProvider.GetSecurityKeys(settings.JwksResource)
            };
            return tokenValidationParameters;
        }
    }
}
