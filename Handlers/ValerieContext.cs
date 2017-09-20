using System;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Valerie.Handlers.Server;
using Valerie.Handlers.Server.Models;

namespace Valerie.Handlers
{
    public class ValerieContext : ICommandContext
    {
        public IUser User { get; }
        public IGuild Guild { get; }
        public IUserMessage Message { get; }
        public IDiscordClient Client { get; }
        public IMessageChannel Channel { get; }
        public IServiceProvider Provider { get; set; }
        public ServerModel Config { get; set; }

        public ValerieContext(IDiscordClient DiscordClient, IUserMessage UserMessage, IServiceProvider ServiceProvider)
        {
            User = UserMessage.Author;
            Guild = (UserMessage.Channel as IGuildChannel).Guild;
            Message = UserMessage;
            Client = DiscordClient;
            Channel = UserMessage.Channel;
            Provider = ServiceProvider;
            Config = Provider.GetService<ServerConfig>().LoadConfig(Guild.Id);
        }
    }
}