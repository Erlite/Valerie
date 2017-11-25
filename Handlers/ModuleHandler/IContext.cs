using System;
using Discord;
using System.Net.Http;
using Discord.Commands;
using Valerie.JsonModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Microsoft.Extensions.DependencyInjection;

namespace Valerie.Handlers.ModuleHandler
{
    public class IContext : ICommandContext
    {
        public IUser User { get; }
        public IGuild Guild { get; }
        public Random Random { get; }
        public ConfigModel Config { get; }
        public ServerModel Server { get; }
        public IDiscordClient Client { get; }
        public HttpClient HttpClient { get; }
        public IUserMessage Message { get; }
        public IMessageChannel Channel { get; }
        public IDocumentSession Session { get; }
        public ConfigHandler ConfigHandler { get; }
        public ServerHandler ServerHandler { get; }

        public IContext(IDiscordClient ClientParam, IUserMessage MessageParam, IServiceProvider ServiceProvider)
        {
            Client = ClientParam;
            Message = MessageParam;
            User = MessageParam.Author;
            Channel = MessageParam.Channel;
            Guild = (MessageParam.Channel as IGuildChannel).Guild;
            Random = ServiceProvider.GetRequiredService<Random>();
            HttpClient = ServiceProvider.GetRequiredService<HttpClient>();
            Config = ServiceProvider.GetRequiredService<ConfigHandler>().Config;
            ConfigHandler = ServiceProvider.GetRequiredService<ConfigHandler>();
            ServerHandler = ServiceProvider.GetRequiredService<ServerHandler>();
            Session = ServiceProvider.GetRequiredService<IDocumentStore>().OpenSession();
            Server = ServiceProvider.GetRequiredService<ServerHandler>().GetServer(Guild.Id);
        }
    }
}