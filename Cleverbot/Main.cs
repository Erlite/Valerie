using Newtonsoft.Json.Linq;

namespace Cleverbot
{
    public static class Main
    {
        public static Models.CleverbotResponse Talk(string message, string state = null)
        {
            string jsonStr = Common.ApiCall(message, state);
            if (jsonStr == null)
                return null;
            JToken resp = JObject.Parse(jsonStr);
            return new Models.CleverbotResponse(resp);
        }

        public static Models.CleverbotResponse Talk(string message, Models.CleverbotResponse previousResponse)
        {
            if (previousResponse != null)
                return Talk(message, previousResponse.CleverbotState);
            else
                return Talk(message);
        }

        public static void SetAPIKey(string apiKey)
        {
            Common.apiKey = apiKey;
        }
    }
}