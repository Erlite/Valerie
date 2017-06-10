using Newtonsoft.Json;
using System.Collections.Generic;
using Rick.Interfaces;

namespace Rick.Models
{
    public class GuildModel : IServer
    {
        [JsonProperty("GuildPrefix")]
        public string GuildPrefix { get; set; } = "?>";

        [JsonProperty("WelcomeMessage")]
        public string WelcomeMessage { get; set; } = "Welcome to our server! All weapons stay outside!";

        [JsonProperty("ModChannelID")]
        public ulong ModChannelID { get; set; }

        [JsonProperty("MuteRoleID")]
        public ulong MuteRoleId { get; set; }

        [JsonProperty("CaseNumber")]
        public int CaseNumber { get; set; }

        [JsonProperty("ChatKarma")]
        public bool ChatKarma { get; set; }

        [JsonProperty("ChatterBot")]
        public bool ChatterBot { get; set; }

        [JsonProperty("JoinEvent")]
        public EventWrapper JoinEvent { get; set; } = new EventWrapper();

        [JsonProperty("LeaveEvent")]
        public EventWrapper LeaveEvent { get; set; } = new EventWrapper();

        [JsonProperty("UserBanned")]
        public EventWrapper UserBanned { get; set; } = new EventWrapper();

        [JsonProperty("RequiredRoleID")]
        public List<ulong> RequiredRoleIDs { get; set; } = new List<ulong>();

        [JsonProperty("RequiredChannelNames")]
        public List<string> RequiredChannelNames { get; set; } = new List<string>();

        [JsonProperty("Tags")]
        public List<TagsModel> TagsList { get; set; } = new List<TagsModel>();

        [JsonProperty("AfkList")]
        public Dictionary<ulong, string> AfkList { get; set; } = new Dictionary<ulong, string>();

        [JsonProperty("KarmaList")]
        public Dictionary<ulong, int> Karma { get; set; } = new Dictionary<ulong, int>();
    }
}
