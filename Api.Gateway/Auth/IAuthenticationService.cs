using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Api.Gateway.Auth
{
    public interface IAuthenticationService
    {
        bool Authenticate(HttpRequest request);

        Guid? GetMemberId();
    }
}
