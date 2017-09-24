# pragma warning disable 4014, 1998

using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Services;
using Valerie.Handlers.Config;

namespace Valerie.Handlers
{
    class CommandHandler
    {
        BotConfig Config;
        IServiceProvider Provider;
        DiscordSocketClient Client;
        CommandService CommandService;

        public CommandHandler(DiscordSocketClient SocketClient, CommandService Commands, BotConfig BotConfig)
        {
            Config = BotConfig;
            Client = SocketClient;
            CommandService = Commands;
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
            int argPos = 0;
            var Context = new ValerieContext(Client, Msg as IUserMessage, Provider);
            Context.ValerieConfig.MessagesReceived++;
            if (!(Msg.HasStringPrefix(Context.ValerieConfig.Prefix, ref argPos) || Msg.HasStringPrefix(Context.Config.Prefix, ref argPos)) ||
                Msg.Source != MessageSource.User || Msg.Author.IsBot || Context.ValerieConfig.UsersBlacklist.ContainsKey(Msg.Author.Id)) return;
            var Result = await CommandService.ExecuteAsync(Context, argPos, Provider, MultiMatchHandling.Best);
            Context.ValerieConfig.CommandsUsed++;
            _ = Config.SaveAsync(Context.ValerieConfig);

            switch (Result.Error)
            {
                case CommandError.Exception: Logger.Write(Status.ERR, Source.Exception, Result.ErrorReason); break;
                case CommandError.UnmetPrecondition: await Context.Channel.SendMessageAsync(Result.ErrorReason); break;
                case CommandError.Unsuccessful: Logger.Write(Status.ERR, Source.UnSuccesful, Result.ErrorReason); break;
            }
        }
    }
}