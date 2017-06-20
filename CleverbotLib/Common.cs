using System.Net;

namespace Rick.CleverbotLib
{
    internal static class Common
    {
        public static string apiKey = "";
        public static string wrapperName = "CleverbotLib";

        private static string apiBaseUrl = "http://www.cleverbot.com";
        private static string resource = "getreply";

        public static string ApiCall(string message, string state = null)
        {
            string apiUrl = $"{apiBaseUrl}/{resource}?key={apiKey}&wrapper={wrapperName}&input={message}";
            if (state != null)
                apiUrl += $"&cs={state}";

            try
            {
                return new WebClient().DownloadString(apiUrl);
            }
            catch (WebException e) { handleWebException(e); return null; }

        }

        public static void handleWebException(WebException e)
        {
            HttpWebResponse errorResp = e.Response as HttpWebResponse;
            switch (errorResp.StatusCode)
            {
                case (HttpStatusCode)401:
                    throw new Exceptions.CleverbotApiException("Invalid or missing Cleverbot API key!");
                case (HttpStatusCode)404:
                    throw new Exceptions.CleverbotApiException("API not found. Contact library developer.");
                case (HttpStatusCode)413:
                    throw new Exceptions.CleverbotApiException("Request was too large! Keep requests under 16kb.");
                case (HttpStatusCode)502:
                case (HttpStatusCode)504:
                    throw new Exceptions.CleverbotApiException("Unable to get reply from API server. Contact Cleverbot staff.");
                case (HttpStatusCode)503:
                    throw new Exceptions.CleverbotApiException("Too many requests from a single IP or API key.");
            }
        }
    }
}
