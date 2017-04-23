using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Rick.Interfaces;
using Rick.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Rick.Models
{
    public class BotModel : IBotInterface
    {
        public static IBotInterface BotConfig { get; set; }
        [JsonIgnore]
        public const string configPath = "BotConfig.json";

        [JsonProperty]
        public string BotToken { get; set; }

        [JsonProperty]
        public string BotName { get; set; }

        [JsonProperty]
        public string DefaultPrefix { get; set; }

        [JsonProperty]
        public string BingAPIKey { get; set; }

        [JsonProperty]
        public bool MentionDefaultPrefix { get; set; }

        [JsonProperty]
        public bool DebugMode { get; set; }

        [JsonProperty]
        public bool ClientLatency { get; set; }

        [JsonProperty]
        public Dictionary<ulong, string> Blacklist { get; set; } = new Dictionary<ulong, string>();

        public bool MentionDefaultPrefixEnabled(SocketUserMessage m, DiscordSocketClient c, ref int ap)
        {
            if (!MentionDefaultPrefix)
                return false;
            return m.HasMentionPrefix(c.CurrentUser, ref ap);
        }

        public static async Task<BotModel> LoadConfigAsync()
        {
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<BotModel>(json);
            }
            var newConfig = await CreateNewAsync();
            return newConfig;
        }

        public static async Task<BotModel> CreateNewAsync()
        {
            BotModel result;
            result = new BotModel();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bot Token: ");
            result.BotToken = Console.ReadLine();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bot Name: ");
            result.BotName = Console.ReadLine();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bot DefaultPrefix: ");
            result.DefaultPrefix = Console.ReadLine();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bing API Key: ");
            result.BingAPIKey = Console.ReadLine();

            ConsoleService.NewLine("Yes = y and No = n ");
            ConsoleService.Log(LogSeverity.Info, "Config", "Enable Debug mode for commands? ");
            char debug = Console.ReadLine().ToLower()[0];
            switch (debug)
            {
                case 'y': result.DebugMode = true; break;
                case 'n': result.DebugMode = false; break;
                default: result.DebugMode = false; break;
            }

            ConsoleService.Log(LogSeverity.Info, "Config", "Enable Bot mention? ");
            char input = Console.ReadLine().ToLower()[0];
            switch (input)
            {
                case 'y': result.MentionDefaultPrefix = true; break;
                case 'n': result.MentionDefaultPrefix = false; break;
                default: result.MentionDefaultPrefix = false; break;
            }

            string directory = Directory.GetCurrentDirectory();

            using (var configStream = File.Create(Path.Combine(Directory.GetCurrentDirectory(), configPath)))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    var save = JsonConvert.SerializeObject(result, Formatting.Indented);
                    await configWriter.WriteAsync(save);
                }
            }
            return result;
        }

        public void SaveFile()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(configPath, json);
        }

        public static async Task SaveAsync(string path, IBotInterface botConfig)
            => File.WriteAllText(path, await Task.Run(() => JsonConvert.SerializeObject(botConfig, Formatting.Indented)));
    }
}
