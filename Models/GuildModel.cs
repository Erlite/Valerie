using Newtonsoft.Json;
using System.Collections.Generic;
using Rick.Interfaces;
using System.Threading.Tasks;
using System.IO;

namespace Rick.Models
{
    public class GuildModel : IGuildConfig
    {
        [JsonIgnore]
        public const string configPath = "GuildConfig.json";

        [JsonProperty("GuildPrefix")]
        public string GuildPrefix { get; set; } = ">";

        [JsonProperty("WelcomeMessage")]
        public string WelcomeMessage { get; set; }

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

        [JsonProperty("Responses")]
        public Dictionary<string, string> Responses { get; set; } = new Dictionary<string, string> { };

        public static Dictionary<ulong, GuildModel> GuildConfigs { get; set; } = new Dictionary<ulong, GuildModel>();

        public static async Task SaveAsync<T>(string path, Dictionary<ulong, T> configs) where T : IGuildConfig
            => File.WriteAllText(path, await Task.Run(() => JsonConvert.SerializeObject(configs, Formatting.Indented)));

        public static async Task<Dictionary<ulong, T>> LoadServerConfigsAsync<T> (string path = configPath) where T : IGuildConfig, new()
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
