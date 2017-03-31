using Discord;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DiscordBot.Interfaces;
using DiscordBot.Services;

namespace DiscordBot.GuildHandlers
{
    public class ConfigHandler : IGuildHandler
    {
        private GuildHandler GuildHandler;
        private JObject config;
        public ConfigHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
            config = null;
        }

        public async Task InitializeAsync()
        {
            await LoadAsync();
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                config = JObject.Parse(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}config.json"));
            });
        }

        public string GetCommandPrefix()
        {
            return (string)config["CommandPrefix"];
        }

        public WrapperCSA GetAutoRespond()
        {
            return new WrapperCSA((JObject)config["AutoRespond"]);
        }

        public class WrapperCSA
        {
            public WrapperCSA(JObject obj) { IsEnabled = (bool)obj["Enabled"]; TextChannels = obj["TextChannels"].ToObject<List<string>>(); }
            public bool IsEnabled { get; private set; }
            public List<string> TextChannels { get; private set; }
        }
    }
}
