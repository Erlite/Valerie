using System;
using Discord;
using Valerie.Models;
using Valerie.Helpers;
using Valerie.Handlers;
using System.Net.Http;
using Discord.Commands;
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
        public GuildHelper GuildHelper { get; }
        public IMessageChannel Channel { get; }
        public GuildHandler GuildHandler { get; }
        public IDocumentSession Session { get; }
        public ConfigHandler ConfigHandler { get; }
        public MethodHelper MethodHelper { get; }

        public IContext(IDiscordClient ClientParam, IUserMessage MessageParam, IServiceProvider ServiceProvider)
        {
            Client = ClientParam;
            Message = MessageParam;
            User = MessageParam.Author;            
            Channel = MessageParam.Channel;
            Guild = (MessageParam.Channel as IGuildChannel).Guild;
            Random = ServiceProvider.GetRequiredService<Random>();
            HttpClient = ServiceProvider.GetRequiredService<HttpClient>();
            GuildHelper = ServiceProvider.GetRequiredService<GuildHelper>();
            GuildHandler = ServiceProvider.GetRequiredService<GuildHandler>();
            Config = ServiceProvider.GetRequiredService<ConfigHandler>().Config;
            ConfigHandler = ServiceProvider.GetRequiredService<ConfigHandler>();
            MethodHelper = ServiceProvider.GetRequiredService<MethodHelper>();
            Server = ServiceProvider.GetRequiredService<GuildHandler>().GetGuild(Guild.Id);
            Session = ServiceProvider.GetRequiredService<IDocumentStore>().OpenSession();
        }
    }
}