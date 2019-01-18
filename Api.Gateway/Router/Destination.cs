using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Api.Gateway.ErrorHandling;

namespace Api.Gateway.Router
{
    public class Destination
    {
        private readonly IHttpClientFactory _clientFactory;
        public string Url { get; set; }
        public string Endpoint { get; set; }
        public string MemberId { get; set; }
        public bool RequiresAuthentication { get; set; }
        public bool RequiresMemberId { get; set; }

        public Destination(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<HttpResponseMessage> SendRequest(HttpRequest request)
        {
            string requestContent;
            using (var receiveStream = request.Body)
            {
                using (var readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    requestContent = readStream.ReadToEnd();
                }
            }

            using (var newRequest = new HttpRequestMessage(new HttpMethod(request.Method), CreateDestinationUrl(request)))
            {
                newRequest.Content = new StringContent(requestContent, Encoding.UTF8, request.ContentType);
                try
                {
                    var client = _clientFactory.CreateClient();
                    var response = await client.SendAsync(newRequest);
                    return response;
                }
                catch (Exception ex)
                {
                    // PARK-1781 To Do: Log Error Message
                    return ErrorHandler.ConstructErrorMessage(HttpStatusCode.ServiceUnavailable, Messages.NoResponse(Url));
                }
            }
        }

        private string CreateDestinationUrl(HttpRequest request)
        {
            var queryString = request.QueryString.ToString();
            if (RequiresMemberId)
            {
                queryString = AttachMemberIdToQueryString(queryString, MemberId);
            }

            var destinationUrl = Url + Endpoint + queryString;
            return destinationUrl;
        }

        private static string AttachMemberIdToQueryString(string queryString, string memberId)
        {
            var queryPrefix = string.IsNullOrEmpty(queryString) ? "?" : "&";
            return $"{queryString}{queryPrefix}memberId={memberId}";
        }
    }
}
