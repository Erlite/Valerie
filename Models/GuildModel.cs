using Newtonsoft.Json;
using System.Collections.Generic;
using Rick.Interfaces;
using System.Threading.Tasks;
using System.IO;

namespace Rick.Models
{
    public class GuildModel : IGuildConfig
    {
        [JsonProperty("GuildPrefix")]
        public string GuildPrefix { get; set; } = ">";

        [JsonProperty("ModChannelID")]
        public ulong ModChannelID { get; set; } = 0;

        [JsonProperty("JoinLogs")]
        public bool JoinLogs { get; set; }

        [JsonProperty("LeaveLogs")]
        public bool LeaveLogs { get; set; }

        [JsonProperty("NameChanges")]
        public bool NameChangesLogged { get; set; }

        [JsonProperty("NickChanges")]
        public bool NickChangesLogged { get; set; }

        [JsonProperty("UserBanned")]
        public bool UserBannedLogged { get; set; }

        [JsonProperty("MessageRecieve")]
        public bool MessageRecieve { get; set; }

        [JsonProperty("RequiredRoleID")]
        public ulong[] RequiredRoleID { get; set; } = new ulong[] { 1234567890, 0987654321 };

        [JsonProperty("RequiredChannelIDs")]
        public ulong[] RequiredChannelIDs { get; set; } = new ulong[] { 1234567890, 0987654321 };

        [JsonProperty("RequiredChannelNames")]
        public string[] RequiredChannelNames { get; set; } = new string[] { "Spam", "NSFW" };

        [JsonProperty("Tags")]
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string> { };

        public static Dictionary<ulong, GuildModel> GuildConfig { get; set; } = new Dictionary<ulong, GuildModel>();

        public static async Task SaveAsync<T>(string path, Dictionary<ulong, T> configs)
            => File.WriteAllText(path, await Task.Run(() => JsonConvert.SerializeObject(configs, Formatting.Indented)));

        //public GuildModel(string commandPrefix, ulong mod, bool joins, bool leaves, bool names, bool nicks, bool ban, bool msg, ulong[] roles, ulong[] ChnIds, string[] ChnNames, Dictionary<string, string> tags)
        //{
        //    GuildPrefix = commandPrefix;
        //    ModChannelID = mod;
        //    JoinLogs = joins;
        //    LeaveLogs = leaves;
        //    NameChangesLogged = names;
        //    NickChangesLogged = nicks;
        //    UserBannedLogged = ban;
        //    MessageRecieve = msg;
        //    RequiredRoleID = roles;
        //    RequiredChannelIDs = ChnIds;
        //    RequiredChannelNames = ChnNames;
        //    Tags = tags;
        //}
    }
}
