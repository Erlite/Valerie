using System.Threading.Tasks;
using System.IO;
using Discord.WebSocket;
using Discord.Commands;
using Newtonsoft.Json;
using Discord;
using GPB.Services;
using System;

namespace GPB.Handlers
{
    public class ConfigHandler
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public ulong OwnerID { get; set; }
        public ulong MuteRID { get; set; }
        public ulong ModLog { get; set; }
        public string WelcomeMessage { get; set; }
        public bool MentionPrefix { get; set; }

        public bool MentionPrefixEnabled(SocketUserMessage m, DiscordSocketClient c, ref int ap)
        {
            if (!MentionPrefix)
                return false;
            return m.HasMentionPrefix(c.CurrentUser, ref ap);
        }

        public async Task SaveAsync()
        {
            using (var configStream = File.OpenWrite(Path.Combine(Directory.GetCurrentDirectory(), "Config", "Config.json")))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    var save = JsonConvert.SerializeObject(this);
                    await configWriter.WriteAsync(save);
                }
            }
        }

        public static async Task<ConfigHandler> UseCurrentAsync()
        {
            ConfigHandler result;
            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Config", "Config.json")))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var deserializedConfig = await configReader.ReadToEndAsync();
                    result = JsonConvert.DeserializeObject<ConfigHandler>(deserializedConfig);
                    return result;
                }
            }
        }

        public static async Task<ConfigHandler> CreateNewAsync()
        {
            ConfigHandler result;
            result = new ConfigHandler();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bot Token: ");
            result.Token = Console.ReadLine();
            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bot Prefix: ");
            result.Prefix = Console.ReadLine();
            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Owner ID: ");
            result.OwnerID = ulong.Parse(Console.ReadLine());
            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Mute Role ID: ");
            result.MuteRID = ulong.Parse(Console.ReadLine());
            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Mod Log Channel ID: ");
            result.ModLog = ulong.Parse(Console.ReadLine());
            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Welcome Message: ");
            result.WelcomeMessage = Console.ReadLine();
            ConsoleService.Log(LogSeverity.Info, "Config", "Enable Bot mention? (Y/N) Blank = N: ");
            char input = Console.ReadLine().ToLower()[0];
            switch (input)
            {
                case 'y': result.MentionPrefix = true; break;
                case 'n': result.MentionPrefix = false; break;
                default: result.MentionPrefix = false; break;
            }

            string directory = Directory.GetCurrentDirectory();

            using (var configStream = File.Create(Path.Combine(Directory.GetCurrentDirectory(), "Config", "Config.json")))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    var save = JsonConvert.SerializeObject(result, Formatting.Indented);
                    await configWriter.WriteAsync(save);
                }
            }
            return result;
        }
    }
}