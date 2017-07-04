using Newtonsoft.Json;
using System.Collections.Generic;
using Rick.Wrappers;
using Rick.Interfaces;

namespace Rick.Models
{
    public class GuildModel : IServer
    {
        [JsonProperty("Prefix")]
        public string Prefix { get; set; } = "<>";

        [JsonProperty("WelcomeMessages")]
        public List<string> WelcomeMessages { get; set; } = new List<string>();

        [JsonProperty("MuteRoleID")]
        public ulong MuteRoleID { get; set; }

        [JsonProperty("AdminCases")]
        public int AdminCases { get; set; }

        [JsonProperty("NoInvites")]
        public bool NoInvites { get; set; }

        [JsonProperty("IsKarmaEnabled")]
        public bool IsKarmaEnabled { get; set; }

        [JsonProperty("JoinLog")]
        public Wrapper JoinEvent { get; set; } = new Wrapper();

        [JsonProperty("LeaveLog")]
        public Wrapper LeaveEvent { get; set; } = new Wrapper();

        [JsonProperty("AdminLog")]
        public Wrapper AdminLog { get; set; } = new Wrapper();

        [JsonProperty("Chatterbot")]
        public Wrapper Chatterbot { get; set; } = new Wrapper();

        [JsonProperty("Tags")]
        public List<TagsModel> TagsList { get; set; } = new List<TagsModel>();

        [JsonProperty("AFKList")]
        public Dictionary<ulong, string> AFKList { get; set; } = new Dictionary<ulong, string>();

        [JsonProperty("KarmaList")]
        public Dictionary<ulong, int> KarmaList { get; set; } = new Dictionary<ulong, int>();

        [JsonProperty("AssignableRoles")]
        public List<string> AssignableRoles { get; set; } = new List<string>();
    }
}
