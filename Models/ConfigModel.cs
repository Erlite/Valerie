using Rick.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using Rick.Wrappers;

namespace Rick.Models
{
    public class ConfigModel : IConfig
    {
        [JsonProperty("Prefix")]
        public string Prefix { get; set; } = "--";

        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("CommandsUsed")]
        public int CommandsUsed { get; set; }

        [JsonProperty("MessagesReceived")]
        public int MessagesReceived { get; set; }

        [JsonProperty("CensoredWords")]
        public string CensoredWords { get; set; }

        [JsonProperty("IsDebugEnabled")]
        public bool IsDebugEnabled { get; set; }

        [JsonProperty("IsUpdateEnabled")]
        public bool IsUpdateEnabled { get; set; }

        [JsonProperty("IsMentionEnabled")]
        public bool IsMentionEnabled { get; set; }

        [JsonProperty("APIKeys")]
        public APIsWrapper APIKeys { get; set; } = new APIsWrapper();

        [JsonProperty("Blacklist")]
        public Dictionary<ulong, string> Blacklist { get; set; } = new Dictionary<ulong, string>();

        [JsonProperty("EvalImports")]
        public List<string> EvalImports { get; set; } = new List<string>();

        [JsonProperty("Games")]
        public List<string> Games { get; set; } = new List<string>();
    }
}
