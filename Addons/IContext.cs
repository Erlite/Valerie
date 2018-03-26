using System;
using Discord;
using Valerie.Models;
using Valerie.Handlers;
using System.Net.Http;
using Discord.Commands;
using Discord.WebSocket;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Microsoft.Extensions.DependencyInjection;

namespace Valerie.Addons
{
    public class IContext : ICommandContext
    {
        public IUser User { get; }
        public IGuild Guild { get; }
        public Random Random { get; }
        public GuildModel Server { get; }
        public ConfigModel Config { get; }        
        public IDiscordClient Client { get; }
        public HttpClient HttpClient { get; }
        public IUserMessage Message { get; }
        public IMessageChannel Channel { get; }
        public GuildHandler GuildHandler { get; }
        public IDocumentSession Session { get; }
        public ConfigHandler ConfigHandler { get; }

        public IContext(IDiscordClient ClientParam, IUserMessage MessageParam, IServiceProvider ServiceProvider)
        {
            Client = ClientParam as DiscordSocketClient;
            User = MessageParam.Author as SocketUser;            
            Message = MessageParam as SocketUserMessage;            
            Channel = MessageParam.Channel as SocketTextChannel;
            Guild = (MessageParam.Channel as IGuildChannel).Guild as SocketGuild;
            Random = ServiceProvider.GetRequiredService<Random>();
            HttpClient = ServiceProvider.GetRequiredService<HttpClient>();
            GuildHandler = ServiceProvider.GetRequiredService<GuildHandler>();
            Config = ServiceProvider.GetRequiredService<ConfigHandler>().Config;
            ConfigHandler = ServiceProvider.GetRequiredService<ConfigHandler>();
            Session = ServiceProvider.GetRequiredService<IDocumentStore>().OpenSession();
            Server = ServiceProvider.GetRequiredService<GuildHandler>().GetServer(Guild.Id);
        }
    }
}