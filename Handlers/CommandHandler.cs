using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Handlers.Config;
using Valerie.Services;

namespace Valerie.Handlers
{
    class CommandHandler
    {
        BotConfig BotConfig;
        IServiceProvider Provider;
        DiscordSocketClient Client;
        CommandService CommandService;

        public CommandHandler(DiscordSocketClient SocketClient, CommandService Commands, BotConfig Config)
        {
            Client = SocketClient;
            CommandService = Commands;
            BotConfig = Config;
            Client.MessageReceived += HandleMessagesAsync;
        }

        public async Task InitializeAsync(IServiceProvider IServiceProvider)
        {
            Provider = IServiceProvider;
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        async Task HandleMessagesAsync(SocketMessage Message)
        {
            if (!(Message is SocketUserMessage Msg)) return;
            if (Msg.Source != MessageSource.User | Msg.Author.IsBot || BotConfig.Config.UsersBlacklist.ContainsKey(Msg.Author.Id)) return;
            int argPos = 0;
            var Context = new ValerieContext(Client, Msg as IUserMessage, Provider);
            if (!(Msg.HasStringPrefix(BotConfig.Config.Prefix, ref argPos) || Msg.HasStringPrefix(Context.Config.Prefix, ref argPos))) return;
            var Result = await CommandService.ExecuteAsync(Context, argPos, Provider, MultiMatchHandling.Best);
            BotConfig.Config.CommandsUsed += 1;
            BotConfig.SaveAsync(BotConfig.Config);
            if (!Result.Error.HasValue && Result.Error.Value != CommandError.UnknownCommand)
                Logger.Write(Logger.Status.ERR, Logger.Source.Client, $"{Result}");
        }
    }
}
