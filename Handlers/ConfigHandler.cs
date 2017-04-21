using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Rick.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace Rick.Handlers
{
    public class ConfigHandler 
    {
        public string BotToken { get; set; }
        public string DefaultPrefix { get; set; }
        public string BingAPIKey { get; set; }
        public bool MentionDefaultPrefix { get; set; }
        public bool DebugMode { get; set; }

        public bool MentionDefaultPrefixEnabled(SocketUserMessage m, DiscordSocketClient c, ref int ap)
        {
            if (!MentionDefaultPrefix)
                return false;
            return m.HasMentionPrefix(c.CurrentUser, ref ap);
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
            result.BotToken = Console.ReadLine();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bot DefaultPrefix: ");
            result.DefaultPrefix = Console.ReadLine();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bing API Key: ");
            result.BingAPIKey = Console.ReadLine();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enable Debug mode for commands? ");
            char debug = Console.ReadLine().ToLower()[0];
            switch (debug)
            {
                case 'y': result.DebugMode = true; break;
                case 'n': result.DebugMode = false; break;
                default: result.DebugMode = false; break;
            }

            ConsoleService.Log(LogSeverity.Info, "Config", "Enable Bot mention? (Y/N) Blank = N: ");
            char input = Console.ReadLine().ToLower()[0];
            switch (input)
            {
                case 'y': result.MentionDefaultPrefix = true; break;
                case 'n': result.MentionDefaultPrefix = false; break;
                default: result.MentionDefaultPrefix = false; break;
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
