﻿using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Cleverbot.Models;

namespace Cleverbot
{
    public static class Main
    {
        public static async Task<CleverbotResponse> TalkAsync(string Message, string State = null)
        {
            string JsonString = await Common.ApiCallAsync(Message, State);
            if (JsonString == null)
                return null;
            JToken Response = JObject.Parse(JsonString);
            return new CleverbotResponse(Response);
        }

        public static async Task<CleverbotResponse> TalkAsync(string Message, CleverbotResponse PrevResponse)
        {
            return await TalkAsync(Message, PrevResponse.CleverbotState) ?? await TalkAsync(Message);
        }

        public static void SetAPIKey(string apiKey)
        {
            Common.apiKey = apiKey;
        }
    }
}