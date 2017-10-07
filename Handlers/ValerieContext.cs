using System;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Valerie.Handlers.Config;
using Valerie.Handlers.Config.Models;
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
        public ServerModel Config { get; set; }
        public ConfigModel ValerieConfig { get; set; }
        public IServiceProvider Provider { get; }

        public ValerieContext(IDiscordClient DiscordClient, IUserMessage UserMessage, IServiceProvider ServiceProvider)
        {
            Provider = ServiceProvider;
            User = UserMessage.Author;
            Guild = (UserMessage.Channel as IGuildChannel).Guild;
            Message = UserMessage;
            Client = DiscordClient;
            Channel = UserMessage.Channel;
            Config = Provider.GetRequiredService<ServerConfig>().LoadConfig(Guild.Id);
            ValerieConfig = BotConfig.Config;
        }
    }
}