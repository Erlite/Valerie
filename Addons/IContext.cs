using System;
using Discord;
using Valerie.Models;
using System.Net.Http;
using Valerie.Helpers;
using Valerie.Services;
using Valerie.Handlers;
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
        public IUserMessage Message { get; }
        public IDiscordClient Client { get; }
        public HttpClient HttpClient { get; }
        public GuildHelper GuildHelper { get; }
        public IMessageChannel Channel { get; }
        public IDocumentSession Session { get; }        
        public GuildHandler GuildHandler { get; }
        public MethodHelper MethodHelper { get; }
        public ConfigHandler ConfigHandler { get; }
        public RedditService RedditService { get; }

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
            RedditService = ServiceProvider.GetRequiredService<RedditService>();
            Server = ServiceProvider.GetRequiredService<GuildHandler>().GetGuild(Guild.Id);
            Session = ServiceProvider.GetRequiredService<IDocumentStore>().OpenSession();
        }
    }
}