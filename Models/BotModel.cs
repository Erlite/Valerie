using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Rick.Interfaces;
using System.Collections.Generic;

namespace Rick.Models
{
    public class BotModel : IClient
    {
        [JsonProperty("BotToken")]
        public string BotToken { get; set; }

        [JsonProperty("BotName")]
        public string BotName { get; set; }

        [JsonProperty("DefaultPrefix")]
        public string DefaultPrefix { get; set; }

        [JsonProperty("CommandsUsed")]
        public int CommandsUsed { get; set; }

        [JsonProperty("MessagesReceived")]
        public int MessagesReceived { get; set; }

        [JsonProperty("DebugMode")]
        public bool DebugMode { get; set; }

        [JsonProperty("ClientLatency")]
        public bool ClientLatency { get; set; }

        [JsonProperty("AutoUpdate")]
        public bool AutoUpdate { get; set; }

        [JsonProperty("MentionDefaultPrefix")]
        public bool MentionDefaultPrefix { get; set; }

        [JsonProperty("APIKeys")]
        public APIKeysWrapper APIKeys { get; set; } = new APIKeysWrapper();

        [JsonProperty("TwitterWrapper")]
        public TwitterWrapper Twitter { get; set; } = new TwitterWrapper();

        [JsonProperty("Blacklist")]
        public Dictionary<ulong, string> Blacklist { get; set; } = new Dictionary<ulong, string>();

        [JsonProperty("EvalImports")]
        public List<string> EvalImports { get; set; } = new List<string>();

        [JsonProperty("Games")]
        public List<string> Games { get; set; } = new List<string>();

        public bool MentionPrefix(SocketUserMessage m, DiscordSocketClient c, ref int ap)
        {
            if (!MentionDefaultPrefix)
                return false;
            return m.HasMentionPrefix(c.CurrentUser, ref ap);
        }

    }
}
