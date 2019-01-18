using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Api.Gateway.Auth;
using Api.Gateway.ErrorHandling;

namespace Api.Gateway.Router
{
    public class Router
    {
        public string CoreApiUrl { get; set; }
        public List<Route> Routes { get; set; }
        public IAuthenticationService AuthenticationService;

        public Router(string routeConfigFilePath, IAuthenticationService authenticationService)
        {
            var loader = new JsonLoader();
            dynamic router = loader.LoadFromFile<dynamic>(routeConfigFilePath);

            CoreApiUrl = Convert.ToString(router.CoreApiUrl);
            Routes = loader.Deserialize<List<Route>>(Convert.ToString(router.routes));

            AuthenticationService = authenticationService;
        }

        public async Task<HttpResponseMessage> RouteRequest(HttpRequest request)
        {
            var requestPath = request.Path.ToString().TrimEnd('/');

            if(string.IsNullOrEmpty(requestPath))
            {
                return ErrorHandler.ConstructErrorMessage(HttpStatusCode.Forbidden, Messages.ContentsNotListed);
            }

            var route = GetRouteByFullRequestPath(requestPath);
            if(route == null)
            {
                route = GetRouteByServiceNameInRequestPath(requestPath);
                if (route == null)
                {
                    return ErrorHandler.ConstructErrorMessage(HttpStatusCode.NotFound, Messages.RequestPathNotFound(request.Path));
                }
            }

            var destination = route.Destination;
            destination.Url = destination.Url ?? CoreApiUrl;

            if (destination.RequiresAuthentication)
            {
                if (!AuthenticationService.Authenticate(request))
                {
                    return ErrorHandler.ConstructErrorMessage(HttpStatusCode.Unauthorized, Messages.Unauthorized);
                }
                if (destination.RequiresMemberId)
                {
                    destination.MemberId = AuthenticationService.GetMemberId().ToString();
                    if (string.IsNullOrEmpty(destination.MemberId))
                    {
                        return ErrorHandler.ConstructErrorMessage(HttpStatusCode.Unauthorized, Messages.Unauthorized);
                    }
                }
            }

            return await destination.SendRequest(request);
        }

        private Route GetRouteByFullRequestPath(string requestPath)
        {
            return Routes.FirstOrDefault(r => r.Endpoint.Equals(requestPath));
        }

        private Route GetRouteByServiceNameInRequestPath(string requestPath)
        {
            var serviceName = '/' + requestPath.Split('/')[1];
            var route = Routes.FirstOrDefault(r => r.Endpoint.Equals(serviceName));
            if (route != null)
            {
                route.Destination.Endpoint = RemoveRouteEndpointFromRequestPath(requestPath, route.Endpoint);
            }
            return route;
        }

        private static string RemoveRouteEndpointFromRequestPath(string requestPath, string routeEndpoint)
        {
            return requestPath.Replace(routeEndpoint, string.Empty);
        }
    }
}
