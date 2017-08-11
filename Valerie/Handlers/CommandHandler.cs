using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Handlers.ConfigHandler;
using Valerie.Services.Logger;
using Valerie.Services.Logger.Enums;
using Valerie.Handlers.GuildHandler;

namespace Valerie.Handlers
{
    public class CommandHandler
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

        public async Task ConfigureCommandsAsync() =>
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly());

        async Task HandleMessagesAsync(SocketMessage Message)
        {
            var Msg = Message as SocketUserMessage;
            var GuildConfig = ServerDB.GuildConfig((Msg.Channel as SocketGuildChannel).Guild.Id);
            int argPos = 0;

            if (Msg == null || Msg.Author.IsBot || BotDB.Config.Blacklist.ContainsKey(Msg.Author.Id)) return;
            var Context = new SocketCommandContext(Client, Msg);

            if (!(Msg.HasStringPrefix(BotDB.Config.Prefix, ref argPos) || Msg.HasStringPrefix(GuildConfig.Prefix, ref argPos))) return;
            await BotDB.UpdateConfigAsync(ConfigHandler.Enum.ConfigValue.CommandUsed);

            var Result = await CommandService.ExecuteAsync(Context, argPos, Provider, MultiMatchHandling.Best);
            var Service = CommandService.Search(Context, argPos);

            CommandInfo Command = null;
            string ErrorMsg = null;

            if (Result.IsSuccess)
                return;

            if (Service.IsSuccess)
                Command = Service.Commands.FirstOrDefault().Command;

            switch (Result)
            {
                case SearchResult SR: break;
                case ExecuteResult ER:
                    Log.Write(Status.ERR, Source.Client, ER.ErrorReason + "\n" + ER.Exception.StackTrace);
                    break;
                case PreconditionResult PdR:
                    ErrorMsg = PdR.ErrorReason;
                    Log.Write(Status.ERR, Source.Client, $"[{Context.Guild.Name} | {Command.Name}] {PdR.ErrorReason} ");
                    break;
                case ParseResult PR:
                    ErrorMsg = $"{Format.Bold("Usage:")} {BotDB.Config.Prefix}{Command.Name} {string.Join(" ", Command.Parameters.Select(x => x.Name))}\n" +
                        $"{Format.Bold("Summary:")} {Command.Summary}";
                    break;
            }

            await Msg.Channel.SendMessageAsync(ErrorMsg);
        }
    }
}
