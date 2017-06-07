using Newtonsoft.Json;
using Rick.Interfaces;
using Rick.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Rick.Enums;
using Rick.Models;

namespace Rick.Handlers
{
    public class BotHandler
    {
        public static IClient BotConfig { get; set; }

        public static string Data = Path.Combine(Directory.GetCurrentDirectory(), "Data");

        public const string configPath = "Data/BotConfig.json";

        public const double BotVersion = 41.0;

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
            BotModel result = new BotModel();

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

        public static async Task SaveAsync(IClient botConfig)
            => File.WriteAllText(configPath, await Task.Run(() => JsonConvert.SerializeObject(botConfig, Formatting.Indented)));

        public static void DirectoryCheck()
        {
            var Data = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            if (!Directory.Exists(Data))
                Directory.CreateDirectory(Data);
        }
    }
}
