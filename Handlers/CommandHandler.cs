using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using Rick.Services;
using Discord.Addons.InteractiveCommands;
using System;
using Microsoft.Extensions.DependencyInjection;
using Rick.Enums;

namespace Rick.Handlers
{
    public class CommandHandler
    {
        private IServiceProvider Provider;
        private DiscordSocketClient client;
        private CommandService cmds;
        private BotHandler BotHandler;
        private GuildHandler GuildHandler;
        private EventService EventHandler;
        private InteractiveService Interactive;

        public CommandHandler(IServiceProvider prod)
        {
            Provider = prod;
            client = Provider.GetService<DiscordSocketClient>();
            BotHandler = Provider.GetService<BotHandler>();
            GuildHandler = Provider.GetService<GuildHandler>();
            EventHandler = Provider.GetService<EventService>();
            Interactive = Provider.GetService<InteractiveService>();

            client.MessageReceived += HandleCommandsAsync;
            cmds = Provider.GetService<CommandService>();
        }

        public async Task ConfigureAsync()
        {
            await cmds.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandsAsync(SocketMessage msg)
        {
            var gld = (msg.Channel as SocketGuildChannel).Guild;
            var message = msg as SocketUserMessage;

            if (message == null || !(message.Channel is IGuildChannel) || message.Author.IsBot || GuildHandler.GuildConfigs[gld.Id].GuildPrefix == null) return;
            int argPos = 0;
            var context = new SocketCommandContext(client, message);

            string GuildPrefix = GuildHandler.GuildConfigs[gld.Id].GuildPrefix;
            if (!(message.HasStringPrefix(BotHandler.BotConfig.DefaultPrefix, ref argPos) || BotHandler.BotConfig.MentionPrefix(message, client, ref argPos) || message.HasStringPrefix(GuildPrefix, ref argPos))) return;

            var result = await cmds.ExecuteAsync(context, argPos, Provider, MultiMatchHandling.Best);
            var service = cmds.Search(context, argPos);
            CommandInfo Command = null;

            if (service.IsSuccess)
                Command = service.Commands.FirstOrDefault().Command;
            if (result.IsSuccess)
                return;

            string ErrorMsg = null;
            Embed embed = null;
            switch(result)
            {
                case SearchResult search:
                    break;
                case ParseResult parse:
                    ErrorMsg = $"User failed to provide required paramaters for {Command.Name}!\n**Command Usage:** {BotHandler.BotConfig.DefaultPrefix}{Command.Name} {string.Join(", ", Command.Parameters.Select(x => x.Name))}\n"+ 
                        $"You can get more info on how to use {Command.Name} by using:\n{BotHandler.BotConfig.DefaultPrefix}Help {Command.Name}";
                    embed = EmbedService.Embed(EmbedColors.Maroon, "Parsing Failed!", client.CurrentUser.GetAvatarUrl(), null, ErrorMsg);
                    break;
                case PreconditionResult pre:
                    ErrorMsg = pre.ErrorReason;
                    embed = EmbedService.Embed(EmbedColors.Maroon, "Unmet Precondition Error", client.CurrentUser.GetAvatarUrl(), Description: ErrorMsg);
                    break;
                case ExecuteResult exe:
                    var exeresult = (ExecuteResult)result;
                    DefaultCommandError(exeresult, service, context);
                    break;
            }

            if (ErrorMsg != null)
                await context.Channel.SendMessageAsync("", embed: embed);
        }

        private async void DefaultCommandError(ExecuteResult result, SearchResult res, SocketCommandContext context)
        {
            if (BotHandler.BotConfig.DebugMode)
            {
                string Name = $"Error Executing Command || Command Name: {res.Commands.FirstOrDefault().Command.Name}";
                string Description = $"**Error Reason:**\n{result.ErrorReason}\n\n**Target Site:**\n{result.Exception.TargetSite}\n\n**Stack Trace:**";
                string StackTrace = result.Exception.StackTrace;
                var embed = EmbedService.Embed(EmbedColors.Red, Name, client.CurrentUser.GetAvatarUrl(), Description: Description, FooterText: StackTrace);
                await context.Channel.SendMessageAsync("", embed: embed);
            }
            else
                await context.Channel.SendMessageAsync($"{string.Concat(Format.Bold("ERROR: "), result.ErrorReason)}");
        }
    }
}