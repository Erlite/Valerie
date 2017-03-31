using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using DiscordBot.GuildHandlers;

namespace DiscordBot.Handlers
{
    public class MainHandler
    {
        public DiscordSocketClient Client;

        public Handlers.ConfigHandler ConfigHandler { get; private set; }
        public CommandHandler CommandHandler { get; private set; }
        public PermissionHandler PermissionHandler { get; private set; }
        public ExceptionHandler ExceptionHandler { get; private set; }

        //Guild Handlers
        private Dictionary<ulong, GuildHandler> guilds;

        public MainHandler(DiscordSocketClient Discord)
        {
            guilds = new Dictionary<ulong, GuildHandler>();
            Client = Discord;
            ConfigHandler = new Handlers.ConfigHandler();
            CommandHandler = new CommandHandler();
            PermissionHandler = new PermissionHandler(this);
            ExceptionHandler = new ExceptionHandler(this);
        }

        internal async Task GuildAvailableEvent(SocketGuild guild)
        {
            if (guilds.ContainsKey(guild.Id))
                await guilds[guild.Id].RenewGuildObject(guild);
            else
            {
                GuildHandler gh = new GuildHandler(this, guild);
                await gh.InitializeAsync();
                guilds[guild.Id] = gh;
            }
        }

        internal Task LeftGuildEvent(SocketGuild guild)
        {
            if (guilds.ContainsKey(guild.Id))
                guilds[guild.Id].DeleteGuildFolder();
            return Task.CompletedTask;
        }

        public async Task InitializeEarlyAsync(IDependencyMap map)
        {
            await ConfigHandler.InitializeAsync();
            await CommandHandler.InitializeAsync(this, map);
        }

        public async Task Close()
        {
            foreach (ulong id in guilds.Keys)
                await guilds[id].Close();
            await ConfigHandler.Close();
            await CommandHandler.Close();
        }

        public async Task ReloadGuildAsync(SocketGuild guild)
        {
            if (guilds.ContainsKey(guild.Id))
                await guilds[guild.Id].Close();
            GuildHandler gh = new GuildHandler(this, guild);
            await gh.InitializeAsync();
            guilds[guild.Id] = gh;
        }

        public DatabaseHandler GuildDatabaseHandler(IGuild guild)
        {
            if (guild == null)
                return null;
            return guilds[guild.Id].DatabaseHandler;
        }

        public TagHandler GuildTagHandler(IGuild guild)
        {
            if (guild == null)
                return null;
            return guilds[guild.Id].TagHandler;
        }

        public IgnoreHandler GuildIgnoreHandler(IGuild guild)
        {
            if (guild == null)
                return null;
            return guilds[guild.Id].IgnoreHandler;
        }

        public GuildHandlers.ConfigHandler GuildConfigHandler(IGuild guild)
        {
            if (guild == null)
                return null;
            return guilds[guild.Id].ConfigHandler;
        }

        public string GetCommandPrefix(IChannel channel)
        {
            if (channel is ITextChannel)
                return GuildConfigHandler((channel as ITextChannel).Guild).GetCommandPrefix();
            return ConfigHandler.GetDefaultCommandPrefix();
        }
    }
}