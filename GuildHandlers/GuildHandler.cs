using Discord;
using Discord.WebSocket;
using DiscordBot.Handlers;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBot.GuildHandlers
{
    public class GuildHandler
    {
        public MainHandler MainHandler { get; private set; }
        public IGuild Guild { get; private set; }
        public ConfigHandler ConfigHandler { get; private set; }
        public TagHandler TagHandler { get; private set; }
        public AutoRespondHandler Autorespond { get; private set; }

        public GuildHandler(MainHandler MainHandler, SocketGuild Guild)
        {
            this.MainHandler = MainHandler;
            this.Guild = Guild;
            ConfigHandler = new ConfigHandler(this);
            TagHandler = new TagHandler(this);
            Autorespond = new AutoRespondHandler(this);
        }

        public async Task InitializeAsync()
        {
            Check();
            await PrivateInitializeAsync();
        }

        private async Task PrivateInitializeAsync()
        {
            await ConfigHandler.InitializeAsync();
            await TagHandler.InitializeAsync();
            await Autorespond.InitializeAsync();
        }

        public async Task Close()
        {
            await ConfigHandler.Close();
            await TagHandler.Close();
            await Autorespond.Close();
        }

        public async Task RenewGuildObject(SocketGuild Guild)
        {
            this.Guild = Guild;
            await Guild.DownloadUsersAsync();
        }
         
        public void Check()
        {
            if (!Directory.Exists($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}"))
            {
                Directory.CreateDirectory($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}");
                foreach (string newPath in Directory.GetFiles($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}Default", "*.*", SearchOption.TopDirectoryOnly))
                    File.Copy(newPath, newPath.Replace($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}Default", $"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}"));
            }
        }

        public void DeleteGuildFolder()
        {
            if (!Directory.Exists($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}"))
                Directory.Delete($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}", true);
        }
    }
}
