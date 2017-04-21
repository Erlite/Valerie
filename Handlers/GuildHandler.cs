using Discord.WebSocket;
using Newtonsoft.Json;
using Rick.Services;
using System.IO;
using System.Threading.Tasks;

namespace Rick.Handlers
{
    public class GuildHandler
    {
        private DiscordSocketClient client;
        private LogService LogService;

        public ulong ModChannelID { get; set; }
        public bool JoinLogs { get; set; }
        public bool LeaveLogs { get; set; }
        public bool NameChangesLogged { get; set; }
        public bool NickChangesLogged { get; set; }
        public bool UserBannedLogged { get; set; }
        public bool ClientLatency { get; set; }
        public bool MessageRecieve { get; set; }
        public ulong[] RequiredRoleID { get; set; } = new ulong[] { 1234567890, 0987654321 };
        public ulong[] RequiredChannelIDs { get; set; } = new ulong[] { 1234567890, 0987654321 };
        public string[] RequiredChannelNames { get; set; } = new string[] { "Spam", "NSFW"};

        public async Task<bool> SaveConfigurationAsync()
        {
            var config = new GuildHandler();
            var serializedConfig = JsonConvert.SerializeObject(config, Formatting.Indented);

            using (var configStream = File.OpenWrite(Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json")))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    await configWriter.WriteAsync(serializedConfig);
                    return true;
                }
            }
        }

        public  static async Task<GuildHandler> UseCurrentAsync()
        {
            GuildHandler result;
            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json")))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var deserializedConfig = await configReader.ReadToEndAsync();
                    result = JsonConvert.DeserializeObject<GuildHandler>(deserializedConfig);

                    return result;
                }
            }
        }

        public static async Task<GuildHandler> CreateNewAsync()
        {
            GuildHandler result;
            result = new GuildHandler();

            string directory = Directory.GetCurrentDirectory();

            using (var configStream = File.Create(Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json")))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    var save = JsonConvert.SerializeObject(result, Formatting.Indented);
                    await configWriter.WriteAsync(save);
                }
            }
            return result;
        }

        public async Task<bool> LoadGuildConfigAsync()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json"))) return false;

            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json")))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var serializedConfig = await configReader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<GuildHandler>(serializedConfig);
                    if (config == null) return false;

                    if (JoinLogs) LogService.EnableJoinLogging();
                    if (LeaveLogs) LogService.EnableLeaveLogging();
                    if (NameChangesLogged) LogService.EnableNameChangeLogging();
                    if (NickChangesLogged) LogService.EnableNickChangeLogging();
                    if (UserBannedLogged) LogService.EnableUserBannedLogging();
                    if (ClientLatency) LogService.EnableLatencyMonitor();

                    return true;
                }
            }
        }

    }
}
