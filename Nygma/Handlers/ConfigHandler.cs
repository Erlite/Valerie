using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Discord;
using Nygma.Utils;
using System.Collections.Generic;

namespace Nygma.Handlers
{
    public class ConfigHandler
    {
        public string UserToken { get; set; }
        public string BotToken { get; set; }
        public string Prefix { get; set; }
        public string BotName { get; set; }
        public string Perms { get; set; }
        public string Game { get; set; }
        public bool MsgLog { get; set; }
        public bool Welcome { get; set; }
        public string WelcomeMsg { get; set; }
        public string GAPI { get; set; }
        public string BAPI { get; set; }
        public string MAPI { get; set; }
        public string YAPI { get; set; }
        public ulong OwnerID { get; set; }
        public ulong ClientID { get; set; }
        public ulong LogGuild { get; set; }
        public ulong LogChannel { get; set; }

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

            //Basics
            IConsole.Log(LogSeverity.Info, "Config", "Enter User Token: ");
            result.UserToken = Console.ReadLine();
            IConsole.Log(LogSeverity.Info, "Config", "Enter Bot Token: ");
            result.BotToken = Console.ReadLine();
            IConsole.Log(LogSeverity.Info, "Config", "Enter Prefix: ");
            result.Prefix = Console.ReadLine();
            IConsole.Log(LogSeverity.Info, "Config", "Enter App Name: ");
            result.BotName = Console.ReadLine();
            IConsole.Log(LogSeverity.Info, "Config", "Enter Bots Perm: ");
            result.Perms = Console.ReadLine();
            IConsole.Log(LogSeverity.Info, "Config", "Enter Game: ");
            result.Game = Console.ReadLine();
            IConsole.Log(LogSeverity.Info, "Config", "Enable Message Logs? ");
            char enabled = Console.ReadLine().ToLower()[0];
            switch (enabled)
            {
                case 'y': result.MsgLog = true; break;
                case 'n': result.MsgLog = false; break;
                default: result.MsgLog = false; break;
            }
            IConsole.Log(LogSeverity.Info, "Config", "Enable Welcome Message? ");
            char input = Console.ReadLine().ToLower()[0];
            IConsole.Log(LogSeverity.Info, "Config", "Enter Welcome Message: ");
            result.WelcomeMsg = Console.ReadLine();
            switch (input)
            {
                case 'y': result.Welcome = true; break;
                case 'n': result.Welcome = false; break;
                default: result.Welcome = false; break;
            }

            //Keys
            IConsole.Log(LogSeverity.Info, "Config", "Enter Google API Key: ");
            result.GAPI = Console.ReadLine();
            IConsole.Log(LogSeverity.Info, "Config", "Enter Bing API Key: ");
            result.BAPI = Console.ReadLine();
            IConsole.Log(LogSeverity.Info, "Config", "Enter Mashape API Key: ");
            result.MAPI = Console.ReadLine();
            IConsole.Log(LogSeverity.Info, "Config", "Enter Giphy API Key: ");
            result.YAPI = Console.ReadLine();

            //Ulongs
            IConsole.Log(LogSeverity.Info, "Config", "Enter Owner ID: ");
            result.OwnerID = ulong.Parse(Console.ReadLine());
            IConsole.Log(LogSeverity.Info, "Config", "Enter Client ID: ");
            result.ClientID = ulong.Parse(Console.ReadLine());
            IConsole.Log(LogSeverity.Info, "Config", "Enter Log Guild ID: ");
            result.LogGuild = ulong.Parse(Console.ReadLine());
            IConsole.Log(LogSeverity.Info, "Config", "Enter Log Channl ID: ");
            result.LogChannel = ulong.Parse(Console.ReadLine());

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