using Newtonsoft.Json;
using Rick.Interfaces;

namespace Rick.Models
{
    public class BotConfigModel : IBotConfig
    {
        [JsonProperty("BotToken")]
        public string BotToken { get; set; }

        [JsonProperty("DefaultPrefix")]
        public string DefaultPrefix { get; set; }

        [JsonProperty("BingAPIKey")]
        public string BingAPIKey { get; set; }

        [JsonProperty("DebugMode")]
        public bool DebugMode { get; set; }

        [JsonProperty("ClientLatency")]
        public bool ClientLatency { get; set; }

        public BotConfigModel(string botToken,  string Prefix, string BingKey, bool Debug, bool latency)
        {
            BotToken = botToken;
            DefaultPrefix = Prefix;
            BingAPIKey = BingKey;
            DebugMode = Debug;
            ClientLatency = latency;
        }

    }
}
