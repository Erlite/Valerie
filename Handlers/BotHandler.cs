﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Rick.Interfaces;
using Rick.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Rick.Enums;

namespace Rick.Handlers
{
    public class BotHandler : IBotInterface
    {
        public static IBotInterface BotConfig { get; set; }

        public static string Data = Path.Combine(Directory.GetCurrentDirectory(), "Data");

        [JsonIgnore]
        public const string configPath = "Data/BotConfig.json";

        [JsonIgnore]
        public const double BotVersion = 41.0;

        [JsonProperty("BotToken")]
        public string BotToken { get; set; }

        [JsonProperty("BotName")]
        public string BotName { get; set; }

        [JsonProperty("DefaultPrefix")]
        public string DefaultPrefix { get; set; }

        [JsonProperty("BingAPIKey")]
        public string BingAPIKey { get; set; }

        [JsonProperty("MashapeAPIKey")]
        public string MashapeAPIKey { get; set; }

        [JsonProperty("GoogleAPIKey")]
        public string GoogleAPIKey { get; set; }

        [JsonProperty("SearchEngineID")]
        public string SearchEngineID { get; set; }

        [JsonProperty("CleverBotAPIKey")]
        public string CleverBotAPIKey { get; set; }

        [JsonProperty("SteamAPIKey")]
        public string SteamAPIKey { get; set; }

        [JsonProperty("GiphyAPIKey")]
        public string GiphyAPIKey { get; set; } = "dc6zaTOxFJmzC";

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

        [JsonProperty("EvalImports")]
        public List<string> EvalImports { get; set; } = new List<string>();

        [JsonProperty("Games")]
        public List<string> Games { get; set; } = new List<string>();

        public bool MentionPrefix(SocketUserMessage m, DiscordSocketClient c, ref int ap)
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

            ConsoleService.Log(LogType.Info, LogSource.Configuration, "Enter Bot's Token: ");
            result.BotToken = Console.ReadLine();

            ConsoleService.Log(LogType.Info, LogSource.Configuration,"Enter Bot Name: ");
            result.BotName = Console.ReadLine();

            ConsoleService.Log(LogType.Info, LogSource.Configuration,"Enter Bot DefaultPrefix: ");
            result.DefaultPrefix = Console.ReadLine();

            ConsoleService.Log(LogType.Info, LogSource.Configuration,"Enter Bing API Key: ");
            result.BingAPIKey = Console.ReadLine();

            ConsoleService.Log(LogType.Info, LogSource.Configuration,"Enter Mashape API Key: ");
            result.MashapeAPIKey = Console.ReadLine();

            ConsoleService.Log(LogType.Info, LogSource.Configuration,"Enter Google API Key: ");
            result.GoogleAPIKey = Console.ReadLine();

            ConsoleService.Log(LogType.Info, LogSource.Configuration, "Yes = Y || No = N");

            ConsoleService.Log(LogType.Info, LogSource.Configuration,"Enable autoupdate? ");
            char update = Console.ReadLine().ToLower()[0];
            switch (update)
            {
                case 'y': result.AutoUpdate = true; break;
                case 'n': result.AutoUpdate = false; break;
                default: result.AutoUpdate = false; break;
            }

            ConsoleService.Log(LogType.Info, LogSource.Configuration,"Enable Debug mode for commands? ");
            char debug = Console.ReadLine().ToLower()[0];
            switch (debug)
            {
                case 'y': result.DebugMode = true; break;
                case 'n': result.DebugMode = false; break;
                default: result.DebugMode = false; break;
            }

            ConsoleService.Log(LogType.Info, LogSource.Configuration,"Enable Bot mention Prefix? ");
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

        public static async Task SaveAsync(IBotInterface botConfig)
            => File.WriteAllText(configPath, await Task.Run(() => JsonConvert.SerializeObject(botConfig, Formatting.Indented)));

        public static void DirectoryCheck()
        {
            var Data = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            if (!Directory.Exists(Data))
                Directory.CreateDirectory(Data);
        }
    }
}
