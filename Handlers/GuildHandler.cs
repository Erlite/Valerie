using Newtonsoft.Json;
using System.Collections.Generic;
using Rick.Interfaces;
using System.Threading.Tasks;
using System.IO;
using Rick.Classes;

namespace Rick.Handlers
{
    public class GuildHandler : IGuildInterface
    {
        public static Dictionary<ulong, GuildHandler> GuildConfigs { get; set; } = new Dictionary<ulong, GuildHandler>();

        [JsonIgnore]
        public const string configPath = "GuildConfig.json";

        [JsonProperty("GuildPrefix")]
        public string GuildPrefix { get; set; }

        [JsonProperty("WelcomeMessage")]
        public string WelcomeMessage { get; set; }

        [JsonProperty("ModChannelID")]
        public ulong ModChannelID { get; set; }

        [JsonProperty("MuteRoleID")]
        public ulong MuteRoleId { get; set; }

        [JsonProperty("CaseNumber")]
        public int CaseNumber { get; set; }

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

        [JsonProperty("AutoRespond")]
        public bool AutoRespond { get; set; }

        [JsonProperty("RequiredRoleID")]
        public List<ulong> RequiredRoleIDs { get; set; } = new List<ulong>();

        [JsonProperty("RequiredChannelIDs")]
        public List<ulong> RequiredChannelIDs { get; set; } = new List<ulong>();

        [JsonProperty("RequiredChannelNames")]
        public List<string> RequiredChannelNames { get; set; } = new List<string>();

        [JsonProperty("Tags")]
        public List<Tags> TagsList{ get; set; } = new List<Tags>();

        [JsonProperty("AfkList")]
        public Dictionary<ulong, string> AfkList { get; set; } = new Dictionary<ulong, string>();

        [JsonProperty("KarmaList")]
        public Dictionary<ulong, int> Karma { get; set; } = new Dictionary<ulong, int>();

        public static async Task SaveAsync<T>(string path, Dictionary<ulong, T> configs) where T : IGuildInterface
            => File.WriteAllText(path, await Task.Run(() => JsonConvert.SerializeObject(configs, Formatting.Indented)));

        public static async Task<Dictionary<ulong, T>> LoadServerConfigsAsync<T> (string path = configPath) where T : IGuildInterface, new()
        {
            if (File.Exists(path))
            {
                return
                    await
                        Task.Run(() => JsonConvert.DeserializeObject<Dictionary<ulong, T>>(File.ReadAllText(path))).
                             ConfigureAwait(false);
            }
            var newConfig = new Dictionary<ulong, T>();
            await SaveAsync(path, newConfig).ConfigureAwait(false);
            return newConfig;
        }
    }
}
