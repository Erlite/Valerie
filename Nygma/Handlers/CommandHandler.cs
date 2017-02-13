using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Addons.InteractiveCommands;
using Nygma.Utils;
using System.Linq;
using Discord;

namespace Nygma.Handlers
{
    public class CommandHandler
    {
        private DiscordSocketClient client;
        private CommandService commands;
        private IDependencyMap _map;
        private ConfigHandler config;

        public async Task InstallAsync(IDependencyMap map)
        {
            client = map.Get<DiscordSocketClient>();
            config = map.Get<ConfigHandler>();

            commands = new CommandService();
            map.Add(commands);
            map.Add(new InteractiveService(client));
            _map = map;

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            client.MessageReceived += HandleCommand;
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;
            if (message.Content == config.Prefix) return;
            if (message.Author.Id != config.OwnerID) return;

            int argPos = 0;
            if (!(message.HasStringPrefix(config.Prefix, ref argPos))) return;

            var context = new CommandContext(client, message);
            var Result = commands.Search(context, argPos);
            CommandInfo Command = null;
            if (Result.IsSuccess)
                Command = Result.Commands.FirstOrDefault().Command;

            await Task.Run(async () =>
            {
                var result = await commands.ExecuteAsync(context, argPos, _map, MultiMatchHandling.Best);
                if (!result.IsSuccess)
                {
                    if (result is ExecuteResult)
                    {
                        var exeresult = (ExecuteResult)result;
                        DefaultCommandError(exeresult, Result, context);
                    }
                    else if (result is PreconditionResult)
                    {
                        var preresult = (PreconditionResult)result;
                        UnmetPrecondition(result, context);
                    }
                    else if (result is ParseResult)
                    {
                        ParseFailed(result, context);
                    }
                    else if (result is SearchResult)
                    {

                    }
                    else
                    {
                        BadArgCount(result, Result, context);
                    }
                }
            });
        }

        private async void BadArgCount(IResult result, SearchResult res, CommandContext context)
        {
            var embed = new EmbedBuilder();
            embed.Color = Misc.RandColor();
            embed.Title = "Um.. Shit bro..It ain't working";
            embed.Description = "Enclose parameters in question marks";
            embed.ThumbnailUrl = context.User.AvatarUrl;

            embed.AddField(f =>
            {
                f.Name = "Bad Arg Count";
                f.Value = result.ErrorReason;
            });

            await context.Channel.SendMessageAsync("", false, embed);
        }

        private async void DefaultCommandError(ExecuteResult result, SearchResult res, CommandContext context)
        {
            ulong ExeptionGuild = config.LogGuild;
            ulong ExeptionChannel = config.LogChannel;
            if (ExeptionGuild != 0)
            {
                if (ExeptionChannel != 0)
                {
                    var Exembed = new EmbedBuilder();
                    Exembed.Color = Misc.RandColor();
                    var exchannel = client.GetGuild(ExeptionGuild).GetChannel(ExeptionChannel) as IMessageChannel;
                    Exembed.Title = string.Format("Command {0} Errored in **{1}**  **{2}**", res.Commands.FirstOrDefault().Command.Name, context.Guild.Name, context.Channel.Name);
                    Exembed.Description = string.Format("**User:** {0}\n" +
                        "**Stacktrace:**\n{1}", context.User, result.Exception.StackTrace.LimitLength(1000));
                    await exchannel.SendMessageAsync("", false, Exembed);
                }
            }

            var embed = new EmbedBuilder();
            embed.Color = Misc.RandColor();
            embed.Title = "Error executing command";
            embed.Description = string.Format("User {0} failed to execute command **{1}**.", context.User, res.Commands.FirstOrDefault().Command.Name);
            embed.ThumbnailUrl = context.User.AvatarUrl;

            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Error Reason";
                x.Value = result.ErrorReason;
            });

            embed.WithFooter(x =>
            {
                if (ExeptionChannel != 0)
                    x.Text = "Owner has been informed!";
                else
                    x.Text = "Report this to Bot Owner!";
            });

            await context.Channel.SendMessageAsync("", false, embed);
        }

        private async void UnmetPrecondition(IResult result, CommandContext context)
        {
            var embed = new EmbedBuilder();
            embed.Color = Misc.RandColor();
            embed.Title = "UnMet PreCondition!";
            embed.Description = result.ErrorReason.ToString();
            await context.Channel.SendMessageAsync("", false, embed);
        }

        private async void ParseFailed(IResult result, CommandContext context)
        {
            var embed = new EmbedBuilder();
            embed.Color = Misc.RandColor();
            embed.Title = "Parsing Failed!";
            embed.Description = result.ErrorReason.ToString();
            await context.Channel.SendMessageAsync("", false, embed);
        }
        #region HandleCommand
        //private async Task HandleCommand(SocketMessage msg)
        //{
        //    var userMessage = msg as SocketUserMessage;
        //    if (userMessage == null) return;
        //    if (userMessage.Author.Id != config.OwnerID) return;

        //    int argPos = 0;
        //    if (userMessage.HasStringPrefix(config.Prefix, ref argPos)) return;

        //    var context = new CommandContext(client, userMessage);
        //    var result = await commands.ExecuteAsync(context, argPos, _map, MultiMatchHandling.Best);
        //    var exe = (ExecuteResult)result;

        //    if (!result.IsSuccess)
        //    {
        //        switch (result.Error)
        //        {
        //            case CommandError.BadArgCount:
        //                await context.Channel.SendEmbedAsync(EmbedAsync.Error("Bad Arg Count", result.ErrorReason, exe.Exception.Source, exe.Exception.TargetSite.Name, exe.Exception.StackTrace));
        //                break;
        //            case CommandError.Exception:
        //                await context.Channel.SendEmbedAsync(EmbedAsync.Error("Exception", result.ErrorReason, exe.Exception.Source, exe.Exception.TargetSite.MethodHandle.Value.ToString(), exe.Exception.StackTrace));
        //                break;
        //            case CommandError.ParseFailed:
        //                await context.Channel.SendEmbedAsync(EmbedAsync.Error("Parse Failed", result.ErrorReason, exe.Exception.Source, exe.Exception.TargetSite.Name, exe.Exception.StackTrace));
        //                break;
        //            case CommandError.ObjectNotFound:
        //                await context.Channel.SendEmbedAsync(EmbedAsync.Error("Object Not Found", result.ErrorReason, exe.Exception.Source, exe.Exception.TargetSite.Name, exe.Exception.StackTrace));
        //                break;
        //            case CommandError.UnmetPrecondition:
        //                await context.Channel.SendEmbedAsync(EmbedAsync.Error("Unmet Precondition", result.ErrorReason, exe.Exception.Source, exe.Exception.TargetSite.Name, exe.Exception.StackTrace));
        //                break;
        //            case CommandError.MultipleMatches:
        //                await context.Channel.SendEmbedAsync(EmbedAsync.Error("Multiple Matches", result.ErrorReason, exe.Exception.Source, exe.Exception.TargetSite.Name, exe.Exception.StackTrace));
        //                break;
        //            case CommandError.UnknownCommand:
        //                await context.Channel.SendMessageAsync("");
        //                break;
        //        }
        //    }
        //}
    }
    #endregion
}