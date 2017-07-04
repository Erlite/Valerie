﻿using System;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Rick.Extensions;
using Rick.Enums;

namespace Rick.Handlers
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

        public async Task ConfigureCommandsAsync()
        {
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        async Task HandleMessagesAsync(SocketMessage Message)
        {
            var Guild = (Message.Channel as SocketGuildChannel).Guild;
            var Msg = Message as SocketUserMessage;
            var BotConfig = ConfigHandler.IConfig;
            var GuildConfig = GuildHandler.GuildConfigs[Guild.Id];

            if (Msg == null ||
                !(Msg.Channel is SocketGuildChannel) ||
                Msg.Author.IsBot) return;

            int argPos = 0;

            var Context = new SocketCommandContext(Client, Msg);

            if (!(Msg.HasStringPrefix(BotConfig.Prefix, ref argPos) || Msg.HasStringPrefix(GuildConfig.Prefix, ref argPos))) return;

            var Result = await CommandService.ExecuteAsync(Context, argPos, Provider, MultiMatchHandling.Best);

            var service = CommandService.Search(Context, argPos);
            CommandInfo Command = null;

            if (service.IsSuccess)
                Command = service.Commands.FirstOrDefault().Command;
            if (Result.IsSuccess)
                return;

            await Controllers.Events.AddToCommand(Message).ConfigureAwait(false);

            string ErrorMsg = null;
            string Remarks = null;

            if (string.IsNullOrWhiteSpace(Command.Remarks) || Command.Remarks == null)
                Remarks = "This command doesn't require any parameters.";
            else
                Remarks = Command.Remarks;

            Embed embed = null;

            switch (Result)
            {
                case SearchResult SR:
                    break;

                case ParseResult PR:
                    ErrorMsg = $"**Command Usage:** {ConfigHandler.IConfig.Prefix}{Command.Name} {string.Join(" ", Command.Parameters.Select(x => x.Name))}\n" +
                        $"**Example:** {Remarks}\n" +
                        $"**More Info:** To get more information about a command use: {ConfigHandler.IConfig.Prefix}Help CommandName\n";
                    embed = EmbedExtension.Embed(EmbedColors.Maroon, $"{Command.Name} Parameters not provided!",
                        new Uri(Client.CurrentUser.GetAvatarUrl()), Description: $"{Format.Bold("ERROR:")} {ErrorMsg}");
                    break;

                case PreconditionResult PCR:
                    ErrorMsg = PCR.ErrorReason;
                    embed = EmbedExtension.Embed(EmbedColors.Maroon, "Unmet Precondition Error was thrown",
                        new Uri(Client.CurrentUser.GetAvatarUrl()), Description: $"{Format.Bold("ERROR:")} {ErrorMsg}");
                    break;

                case TypeReaderResult TRR:
                    ErrorMsg = TRR.ErrorReason;
                    embed = EmbedExtension.Embed(EmbedColors.Maroon, "TypeReader Error was thrown",
                        new Uri(Client.CurrentUser.GetAvatarUrl()), Description: $"{Format.Bold("ERROR:")} {ErrorMsg}");
                    break;

                case ExecuteResult ER:
                    var exeresult = (ExecuteResult)Result;
                    DefaultCommandError(exeresult, service, Context);
                    break;
            }

            if (ErrorMsg != null)
                await Context.Channel.SendMessageAsync("", embed: embed);
        }

        async void DefaultCommandError(ExecuteResult result, SearchResult res, SocketCommandContext context)
        {
            if (ConfigHandler.IConfig.IsDebugEnabled)
            {
                string Name = $"Error Executing Command || Command Name: {res.Commands.FirstOrDefault().Command.Name}";
                string Description = $"**Error Reason:**\n{result.ErrorReason}\n\n**Target Site:**\n{result.Exception.TargetSite}\n\n**Stack Trace:**";
                string StackTrace = result.Exception.StackTrace;
                var embed = EmbedExtension.Embed(EmbedColors.Red, Name,
                    new Uri(Client.CurrentUser.GetAvatarUrl()), Description: Description, FooterText: StackTrace);
                await context.Channel.SendMessageAsync("", embed: embed);
            }
            else
                await context.Channel.SendMessageAsync($"{string.Concat(Format.Bold("ERROR: "), result.ErrorReason)}");
        }
    }
}
