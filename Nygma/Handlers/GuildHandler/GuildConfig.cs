using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Discord;
using Nygma.Utils;
using System.Collections.Generic;


namespace Nygma.Handlers.GuildHandler
{
    class GuildConfig
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
            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Guilds", "Config.json")))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var deserializedConfig = await configReader.ReadToEndAsync();

                    result = JsonConvert.DeserializeObject<ConfigHandler>(deserializedConfig);
                    return result;
                }
            }
        }
    }
}
