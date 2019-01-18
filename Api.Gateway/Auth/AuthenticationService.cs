using System;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Api.Gateway.Auth
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configuration;

        private ClaimsPrincipal _claimsPrincipal; 

        private const string MemberClaimTypeName = "memberid";

        private readonly TokenValidationParameters _tokenValidationParameters;

        public AuthenticationService(IConfiguration configuration)
        {
            _configuration = configuration;
            _tokenValidationParameters = TokenValidationParamFactory.Create(_configuration);
        }

        public bool Authenticate(HttpRequest request)
        {          
            try
            {
                var accessToken = GetAccessTokenWithoutBearer(request);
                var securityTokenHandler = new JwtSecurityTokenHandler();
                _claimsPrincipal = securityTokenHandler.ValidateToken(accessToken, _tokenValidationParameters, out var rawValidatedToken);
                if (_claimsPrincipal != null && rawValidatedToken != null)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                //PARK-1781 To Do: Log Error Message
            }
            return false;
        }

        public Guid? GetMemberId()
        {
            var activeUser = (from claim in _claimsPrincipal.Claims
                where claim.Type == MemberClaimTypeName
                select claim.Value).FirstOrDefault();

            if (Guid.TryParse(activeUser, out var memberId))
            {
                return memberId;
            }
            return null;
        }

        private static string GetAccessTokenWithoutBearer(HttpRequest request)
        {
            var tokenHeader = request.Headers["Authorization"].ToString();
            var token = tokenHeader.Replace("Bearer", string.Empty).Trim();
            return token;
        }
    }
}
