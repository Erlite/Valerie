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

namespace Rick.Handlers
{
    public class BotHandler : IBotInterface
    {
        public static IBotInterface BotConfig { get; set; }

        [JsonIgnore]
        public const string configPath = "BotConfig.json";

        [JsonIgnore]
        public const double BotVersion = 15;

        [JsonProperty("BotToken")]
        public string BotToken { get; set; }

        [JsonProperty("BotName")]
        public string BotName { get; set; }

        [JsonProperty("BotGame")]
        public string BotGame { get; set; }

        [JsonProperty("DefaultPrefix")]
        public string DefaultPrefix { get; set; }

        [JsonProperty("BingAPIKey")]
        public string BingAPIKey { get; set; }

        [JsonProperty("MashepeKey")]
        public string MashapeKey { get; set; }

        [JsonProperty("DebugMode")]
        public bool DebugMode { get; set; }

        [JsonProperty("ClientLatency")]
        public bool ClientLatency { get; set; }

        [JsonProperty("AutoUpdate")]
        public bool AutoUpdate { get; set; }

        [JsonProperty("MentionDefaultPrefix")]
        public bool MentionDefaultPrefix { get; set; }

        [JsonProperty("Blacklist")]
        public Dictionary<ulong, string> Blacklist { get; set; } = new Dictionary<ulong, string>();

        [JsonProperty("OwnerAfk")]
        public Dictionary<ulong, string> OwnerAfk { get; set; } = new Dictionary<ulong, string>();

        [JsonProperty("EvalImports")]
        public List<string> EvalImports { get; set; } = new List<string>();

        public bool MentionDefaultPrefixEnabled(SocketUserMessage m, DiscordSocketClient c, ref int ap)
        {
            if (!MentionDefaultPrefix)
                return false;
            return m.HasMentionPrefix(c.CurrentUser, ref ap);
        }

        public static async Task<BotHandler> LoadConfigAsync()
        {
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<BotHandler>(json);
            }
            var newConfig = await CreateNewAsync();
            return newConfig;
        }

        public static async Task<BotHandler> CreateNewAsync()
        {
            BotHandler result;
            result = new BotHandler();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bot Token: ");
            result.BotToken = Console.ReadLine();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bot Name: ");
            result.BotName = Console.ReadLine();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bot DefaultPrefix: ");
            result.DefaultPrefix = Console.ReadLine();

            ConsoleService.Log(LogSeverity.Info, "Config", "Enter Bing API Key: ");
            result.BingAPIKey = Console.ReadLine();

            ConsoleService.Log(LogSeverity.Info, "Config", "Yes = y and No = n");

            ConsoleService.Log(LogSeverity.Info, "Config", "Enable autoupdate? ");
            char update = Console.ReadLine().ToLower()[0];
            switch (update)
            {
                case 'y': result.AutoUpdate = true; break;
                case 'n': result.AutoUpdate = false; break;
                default: result.AutoUpdate = false; break;
            }

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

        public static async Task SaveAsync(string path, IBotInterface botConfig)
            => File.WriteAllText(path, await Task.Run(() => JsonConvert.SerializeObject(botConfig, Formatting.Indented)));
    }
}
