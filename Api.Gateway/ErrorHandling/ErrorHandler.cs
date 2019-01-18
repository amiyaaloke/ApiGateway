using System.Net;
using System.Net.Http;

namespace Api.Gateway.ErrorHandling
{
    public static class ErrorHandler
    {
        public static HttpResponseMessage ConstructErrorMessage(HttpStatusCode code, string error)
        {
            var errorMessage = new HttpResponseMessage
            {
                StatusCode = code,
                Content = new StringContent(error)
            };
            return errorMessage;
        }
    }
}
