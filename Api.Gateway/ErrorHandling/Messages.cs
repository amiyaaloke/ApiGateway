namespace Api.Gateway.ErrorHandling
{
    public static class Messages
    {
        public const string Unauthorized = "{\"message\": \"Authorization has been denied for this request.\"}";

        public const string ContentsNotListed = "The contents of this directory cannot be listed.";

        public static string RequestPathNotFound(string requestPath)
        {
            return $"The {requestPath} path could not be found.";
        }

        public static string NoResponse(string urlPath)
        {
            return "{\"message\": \"Unable to get response from " + urlPath + ".\"}";
        }
    }
}
