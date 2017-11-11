using Discord;
using Discord.Commands;
using System;
using Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

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
            ConfigHandler = ServiceProvider.GetRequiredService<ConfigHandler>();
            ServerHandler = ServiceProvider.GetRequiredService<ServerHandler>();
            Config = ServiceProvider.GetRequiredService<ConfigHandler>().GetConfigAsync().GetAwaiter().GetResult();
            Server = ServiceProvider.GetRequiredService<ServerHandler>().GetServerAsync(Guild.Id).GetAwaiter().GetResult();
        }
    }
}