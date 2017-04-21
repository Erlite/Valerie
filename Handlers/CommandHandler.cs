using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using System.IO;
using Rick.Services;
using Rick.Models;

namespace Rick.Handlers
{
    public class CommandHandler
    {
        private IDependencyMap map;
        private DiscordSocketClient client;
        private CommandService cmds;
        private BotConfigHandler config;

        public CommandHandler(IDependencyMap _map)
        {
            client = _map.Get<DiscordSocketClient>();
            config = _map.Get<BotConfigHandler>();
            cmds = new CommandService();
            map = _map;
        }

        public async Task InstallAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await cmds.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommandAsync(SocketMessage m)
        {
            var message = m as SocketUserMessage;            
            if (message == null || !(message.Channel is IGuildChannel) || message.Author.IsBot) return;
            int argPos = 0;
            var context = new CommandContext(client, message);
            var gld = context.Guild as SocketGuild;
            if (!(message.HasStringPrefix(config.DefaultPrefix, ref argPos) || config.MentionDefaultPrefixEnabled(message, client, ref argPos) || message.HasStringPrefix(GuildModel.GuildConfig[gld.Id].GuildPrefix, ref argPos))) return;
            var Result = cmds.Search(context, argPos);
            CommandInfo Command = null;
            if (Result.IsSuccess)
                Command = Result.Commands.FirstOrDefault().Command;

            await Task.Run(async () =>
            {
                var result = await cmds.ExecuteAsync(context, argPos, map, MultiMatchHandling.Best);
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

        private async void DefaultCommandError(ExecuteResult result, SearchResult res, CommandContext context)
        {
            if (config.DebugMode)
            {
                var embed = new EmbedBuilder();
                embed.Color = new Color(150, 16, 25);
                embed.Title = "Error Executing Command";
                embed.Description = string.Format($"**Guild Name:** {context.Guild.Name}\n**Command Name:** {res.Commands.FirstOrDefault().Command.Name}");
                embed.WithAuthor(x =>
                {
                    x.Name = context.User.ToString();
                    x.IconUrl = context.User.GetAvatarUrl();
                });
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Error Reason";
                    x.Value = result.ErrorReason;
                });
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Target Site";
                    x.Value = result.Exception.TargetSite;
                });
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Stacktrace";
                    x.Value = result.Exception.StackTrace;
                });
                await context.Channel.SendMessageAsync("", embed: embed);
            }
            else
                await context.Channel.SendMessageAsync($"{string.Concat(Format.Bold("ERROR: "), result.ErrorReason)}");
        }

        private async void BadArgCount(IResult result, SearchResult res, CommandContext context)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(150, 16, 25))
                .WithAuthor(x =>
                {
                    x.IconUrl = client.CurrentUser.GetAvatarUrl();
                    x.Name = "Bad argument count!";
                })
                .WithDescription($"Enclose parameters in question marks\n{result.ErrorReason}");
            await context.Channel.SendMessageAsync("", false, embed);
        }

        private async void UnmetPrecondition(IResult result, CommandContext context)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(150, 16, 25))
                .WithAuthor(x =>
                {
                    x.IconUrl = client.CurrentUser.GetAvatarUrl();
                    x.Name = "Unmet Precondition Error";
                })
                .WithDescription(result.ErrorReason);
            await context.Channel.SendMessageAsync("", false, embed);
        }

        private async void ParseFailed(IResult result, CommandContext context)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(150, 16, 25))
                .WithAuthor(x =>
                {
                    x.IconUrl = client.CurrentUser.GetAvatarUrl();
                    x.Name = "Parsing Failed";
                })
                .WithDescription(result.ErrorReason);
            await context.Channel.SendMessageAsync("", false, embed);
        }
    }
}