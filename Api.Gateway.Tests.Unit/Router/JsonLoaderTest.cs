using NUnit.Framework;
using System;
using System.Collections.Generic;
using Api.Gateway.Router;

namespace Api.Gateway.Tests.Unit.Router
{
    public class JsonLoaderTest
    {
        private JsonLoader _loader;
        private dynamic _router;

        [OneTimeSetUp]
        public void Setup()
        {
            _loader = new JsonLoader();

            _router = _loader.LoadFromFile<dynamic>(@"Router\routes_test.json");
        }

        [Test]
        public void GivenRouteFileIsSupplied_RoutesAreExtractedFromTheFile()
        {
            var routes = _loader.Deserialize<List<Route>>(Convert.ToString(_router.routes));

            Assert.AreEqual("http://backendserviceurl.com", Convert.ToString(_router.backendServiceApiUrl));
            Assert.AreEqual(2, routes.Count);

            int routeIndex = 0;
            Assert.AreEqual("http://newbackendserviceurl.com", Convert.ToString(routes[routeIndex].Destination.Url));
            Assert.AreEqual("/rootEndpoint/endpoint1", Convert.ToString(routes[routeIndex].Endpoint));
            Assert.AreEqual("/endpoint1", Convert.ToString(routes[routeIndex].Destination.Endpoint));
            Assert.AreEqual(false, Convert.ToBoolean(routes[routeIndex].Destination.RequiresAuthentication));
            Assert.AreEqual(false, Convert.ToBoolean(routes[routeIndex].Destination.RequiresMemberId));

            routeIndex = 1;
            Assert.AreEqual(null, routes[routeIndex].Destination.Url);
            Assert.AreEqual("/rootEndpoint", Convert.ToString(routes[routeIndex].Endpoint));
            Assert.AreEqual(null, routes[routeIndex].Destination.Endpoint);
            Assert.AreEqual(true, Convert.ToBoolean(routes[routeIndex].Destination.RequiresAuthentication));
            Assert.AreEqual(true, Convert.ToBoolean(routes[routeIndex].Destination.RequiresMemberId));
        }
    }
}