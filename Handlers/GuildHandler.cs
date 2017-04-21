using Discord.WebSocket;
using Newtonsoft.Json;
using Rick.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace Rick.Handlers
{
    public class GuildHandler
    {
        private DiscordSocketClient client;
        //private LogService LogService;

        public string GuildPrefix { get; set; } = "?>";
        public ulong ModChannelID { get; set; } = 1234567890;
        public bool JoinLogs { get; set; }
        public bool LeaveLogs { get; set; }
        public bool NameChangesLogged { get; set; }
        public bool NickChangesLogged { get; set; }
        public bool UserBannedLogged { get; set; }
        //public bool ClientLatency { get; set; }
        public bool MessageRecieve { get; set; }
        public ulong[] RequiredRoleID { get; set; } = new ulong[] { 1234567890, 0987654321 };
        public ulong[] RequiredChannelIDs { get; set; } = new ulong[] { 1234567890, 0987654321 };
        public string[] RequiredChannelNames { get; set; } = new string[] { "Spam", "NSFW"};
        public static Dictionary<ulong, GuildHandler> GuildConfig { get; set; } = new Dictionary<ulong, GuildHandler>();


        public static async Task SaveAsync<T>(string path, Dictionary<ulong, T> configs)
            => File.WriteAllText(path, await Task.Run(() => JsonConvert.SerializeObject(configs, Formatting.Indented)));

        public async Task CreateGuildConfigAsync(SocketGuild Guild)
        {
            //await Guild.DefaultChannel.SendMessageAsync($"Thank you for inviting me to **{Guild.Name}**. I've set up your config!");
            var newConfig = new GuildHandler();
            GuildConfig.Add(Guild.Id, newConfig);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json");
            await SaveAsync(path, GuildConfig);
        }

        public async Task RemoveGuildConfigAsync(SocketGuild Guild)
        {
            ConsoleService.Log(LogSeverity.Warning, Guild.Name, "Config Deleted!");
            if (GuildConfig.ContainsKey(Guild.Id))
            {
                GuildConfig.Remove(Guild.Id);
            }
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json");
            await SaveAsync(path, GuildConfig);
        }

    }
}
