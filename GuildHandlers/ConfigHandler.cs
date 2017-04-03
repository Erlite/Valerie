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

        public string GetWelcomeMessage()
        {
            return (string)config["WelcomeMessage"];
        }
        
        public ulong GetModChannel()
        {
            return (ulong)config["ModChannelID"];
        }

        public MultiWrapper GetAutoRespond()
        {
            return new MultiWrapper((JObject)config["AutoRespond"]);
        }

        public EventWrapper EventsLogging()
        {
            return new EventWrapper((JObject)config["EventsLog"]);
        }

        // Classes
        public class MultiWrapper
        {
            public MultiWrapper(JObject obj)
            {
                IsEnabled = (bool)obj["Enabled"];
                TextChannels = obj["TextChannels"].ToObject<List<string>>();
            }

            public bool IsEnabled { get; private set; }
            public List<string> TextChannels { get; private set; }
        }

        public class EventWrapper
        {
            public EventWrapper(JObject obj)
            {
                JoinLog = (bool)obj["JoinLog"];
                LeaveLog = (bool)obj["LeaveLog"];
                BanLog = (bool)obj["BanLog"];
                TextChannel = (ulong)obj["EventTextChannel"];
            }

            public bool JoinLog { get; private set; }
            public bool LeaveLog { get; private set; }
            public bool BanLog { get; private set; }
            public ulong TextChannel { get; private set; }
        }
    }
}
