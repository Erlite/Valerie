using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using Rick.Services;
using Rick.Models;
using Discord.Addons.InteractiveCommands;
using Rick.Attributes;

namespace Rick.Handlers
{
    public class CommandHandler
    {
        private IDependencyMap map;
        private DiscordSocketClient client;
        private CommandService cmds;
        private BotModel config;
        private GuildModel model;
        private EventService Logger;
        private InteractiveService Interactive;

        public CommandHandler(IDependencyMap _map)
        {
            client = _map.Get<DiscordSocketClient>();
            config = _map.Get<BotModel>();
            model = _map.Get<GuildModel>();
            Logger = _map.Get<EventService>();
            Interactive = _map.Get<InteractiveService>();
            client.MessageReceived += HandleCommandsAsync;
            //cmds = _map.Get<CommandService>();
            cmds = new CommandService();
            map = _map;
        }

        public async Task ConfigureAsync()
        {
            await cmds.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandsAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null || !(message.Channel is IGuildChannel) || message.Author.IsBot) return;
            int argPos = 0;
            //var context = new CommandContext(client, message);
            var context = new SocketCommandContext(client, message);
            var gld = context.Guild as SocketGuild;
            if (!(message.HasStringPrefix(BotModel.BotConfig.DefaultPrefix, ref argPos) || config.MentionDefaultPrefixEnabled(message, client, ref argPos) || message.HasStringPrefix(GuildModel.GuildConfigs[gld.Id].GuildPrefix, ref argPos))) return;
            var result = await cmds.ExecuteAsync(context, argPos, map, MultiMatchHandling.Best);

            if (result.IsSuccess)
                return;

            string ErrorMsg = null;
            switch(result)
            {
                case SearchResult search:
                    break;
                case ParseResult parse:
                    ErrorMsg = $":x: Failed to provide required parameters! {parse.ErrorReason}";
                    break;
                case PreconditionResult pre:
                    ErrorMsg = pre.ErrorReason;
                    break;
                case ExecuteResult exe:
                    ErrorMsg = $":x: Failed to execute command! If this continues please contact bot developer!";
                    break;
            }
        }



        #region
        //public async Task InstallAsync()
        //{
        //    client.MessageReceived += HandleCommandAsync;
        //    await cmds.AddModulesAsync(Assembly.GetEntryAssembly());
        //}

        //public async Task HandleCommandAsync(SocketMessage m)
        //{
        //    var message = m as SocketUserMessage;            
        //    if (message == null || !(message.Channel is IGuildChannel) || message.Author.IsBot) return;
        //    int argPos = 0;
        //    var context = new CommandContext(client, message);
        //    var gld = context.Guild as SocketGuild;
        //    if (!(message.HasStringPrefix(BotModel.BotConfig.DefaultPrefix, ref argPos) || config.MentionDefaultPrefixEnabled(message, client, ref argPos) || message.HasStringPrefix(GuildModel.GuildConfigs[gld.Id].GuildPrefix, ref argPos))) return;
        //    var Result = cmds.Search(context, argPos);
        //    CommandInfo Command = null;
        //    if (Result.IsSuccess)
        //        Command = Result.Commands.FirstOrDefault().Command;

        //    await Task.Run(async () =>
        //    {
        //        var result = await cmds.ExecuteAsync(context, argPos, map, MultiMatchHandling.Best);
        //        if (!result.IsSuccess)
        //        {
        //            if (result is ExecuteResult)
        //            {
        //                var exeresult = (ExecuteResult)result;
        //                DefaultCommandError(exeresult, Result, context);
        //            }
        //            else if (result is PreconditionResult)
        //            {
        //                var preresult = (PreconditionResult)result;
        //                UnmetPrecondition(result, context);
        //            }
        //            else if (result is ParseResult)
        //            {
        //                ParseFailed(result, context);
        //            }
        //            else if (result is SearchResult)
        //            {

        //            }
        //            else
        //            {
        //                BadArgCount(result, Result, context);
        //            }
        //        }
        //    });
        //}

        //private async void DefaultCommandError(ExecuteResult result, SearchResult res, CommandContext context)
        //{
        //    if (BotModel.BotConfig.DebugMode)
        //    {
        //        var embed = new EmbedBuilder();
        //        embed.Color = new Color(150, 16, 25);
        //        embed.WithAuthor(x =>
        //        {
        //            x.Name = $"Error Executing Command || Command Name: {res.Commands.FirstOrDefault().Command.Name}";
        //            x.IconUrl = client.CurrentUser.GetAvatarUrl();
        //        });
        //        embed.Description = $"**Error Reason:**\n{result.ErrorReason}\n\n**Target Site:**\n{result.Exception.TargetSite}\n\n**Stack Trace:**";
        //        embed.WithFooter(x =>
        //        {
        //            x.Text = result.Exception.StackTrace;
        //        });
        //        await context.Channel.SendMessageAsync("", embed: embed);
        //    }
        //    else
        //        await context.Channel.SendMessageAsync($"{string.Concat(Format.Bold("ERROR: "), result.ErrorReason)}");
        //}

        //private async void BadArgCount(IResult result, SearchResult res, CommandContext context)
        //{
        //    var embed = new EmbedBuilder()
        //        .WithColor(new Color(150, 16, 25))
        //        .WithAuthor(x =>
        //        {
        //            x.IconUrl = client.CurrentUser.GetAvatarUrl();
        //            x.Name = "Bad argument count!";
        //        })
        //        .WithDescription($"Enclose parameters in question marks\n{result.ErrorReason}");
        //    await context.Channel.SendMessageAsync("", false, embed);
        //}

        //private async void UnmetPrecondition(IResult result, CommandContext context)
        //{
        //    var embed = new EmbedBuilder()
        //        .WithColor(new Color(150, 16, 25))
        //        .WithAuthor(x =>
        //        {
        //            x.IconUrl = client.CurrentUser.GetAvatarUrl();
        //            x.Name = "Unmet Precondition Error";
        //        })
        //        .WithDescription(result.ErrorReason);
        //    await context.Channel.SendMessageAsync("", false, embed);
        //}

        //private async void ParseFailed(IResult result, CommandContext context)
        //{
        //    var embed = new EmbedBuilder()
        //        .WithColor(new Color(150, 16, 25))
        //        .WithAuthor(x =>
        //        {
        //            x.IconUrl = client.CurrentUser.GetAvatarUrl();
        //            x.Name = "Parsing Failed";
        //        })
        //        .WithDescription(result.ErrorReason);
        //    await context.Channel.SendMessageAsync("", false, embed);
        //}
        #endregion
    }
}