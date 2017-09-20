using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Services;
using Valerie.Handlers.Config;

namespace Valerie.Handlers
{
    class CommandHandler
    {
        IServiceProvider Provider;
        DiscordSocketClient Client;
        CommandService CommandService;

        public CommandHandler(IServiceProvider IServiceProvider)
        {
            Provider = IServiceProvider;
            Client = Provider.GetService<DiscordSocketClient>();

            Client.MessageReceived += HandleMessagesAsync;
            CommandService = Provider.GetService<CommandService>();
        }

        public async Task ConfigureCommandsAsync() => await CommandService.AddModulesAsync(Assembly.GetEntryAssembly());

        async Task HandleMessagesAsync(SocketMessage Message)
        {
            var Msg = Message as SocketUserMessage;
            int argPos = 0;
            if (Msg == null || Msg.Author.IsBot || BotConfig.Config.UsersBlacklist.ContainsKey(Msg.Author.Id)) return;

            var Context = new ValerieContext(Client, Msg as IUserMessage, Provider);
            if (!(Msg.HasStringPrefix(BotConfig.Config.Prefix, ref argPos) || Msg.HasStringPrefix(Context.Config.Prefix, ref argPos))) return;

            var Result = await CommandService.ExecuteAsync(Context, argPos, Provider, MultiMatchHandling.Best);
        }
    }
}
