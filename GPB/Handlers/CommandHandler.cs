using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using GPB.Services;

namespace GPB.Handlers
{
    public class CommandHandler
    {
        private IDependencyMap map;
        private DiscordSocketClient client;
        private CommandService cmds;
        private ConfigHandler config;
        private LogService log;

        public CommandHandler(IDependencyMap _map)
        {
            client = _map.Get<DiscordSocketClient>();
            config = _map.Get<ConfigHandler>();
            log = _map.Get<LogService>();
            cmds = new CommandService();
            //_map.Add(cmds);
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
            if (message == null) return;
            if (!(message.Channel is IGuildChannel)) return;
            int argPos = 0;
            if (!(message.HasStringPrefix(config.Prefix, ref argPos) || config.MentionPrefixEnabled(message, client, ref argPos))) return;
            var context = new CommandContext(client, message);
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
            ulong ExeptionChannel = log.ServerLogChannelId;
            if (ExeptionChannel != 0)
            {
                var Exembed = new EmbedBuilder();
                Exembed.Color = new Color(150, 16, 25);
                var exchannel = client.GetChannel(ExeptionChannel) as ITextChannel;
                Exembed.Title = string.Format("Command {0} Errored in **{1}**  **{2}**", res.Commands.FirstOrDefault().Command.Name, context.Guild.Name, context.Channel.Name);
                Exembed.Description = string.Format("**User:** {0}\n**Stacktrace:**\n{1}", context.User, result.Exception.StackTrace);
                await exchannel.SendMessageAsync("", false, Exembed);
            }

            var embed = new EmbedBuilder();
            embed.Color = new Color(150, 16, 25);
            embed.Title = "Error executing command";
            embed.Description = string.Format("User {0} failed to execute command **{1}**.", context.User, res.Commands.FirstOrDefault().Command.Name);
            embed.ThumbnailUrl = context.User.GetAvatarUrl();
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Error Reason";
                x.Value = result.ErrorReason;
            });
            embed.WithFooter(x =>
            {
                if (ExeptionChannel != 0)
                    x.Text = "Posted the error in Bot logs!";
                else
                    x.Text = "Report this to Bot Owner!";
            });
            await context.Channel.SendMessageAsync("", false, embed);
        }

        private async void BadArgCount(IResult result, SearchResult res, CommandContext context)
        {
            var embed = new EmbedBuilder();
            embed.Color = new Color(150, 16, 25);
            embed.Title = "Um.. Shit bro..It ain't working";
            embed.Description = "Enclose parameters in question marks";
            embed.ThumbnailUrl = context.User.GetAvatarUrl();

            embed.AddField(f =>
            {
                f.Name = "Bad Arg Count";
                f.Value = result.ErrorReason;
            });

            await context.Channel.SendMessageAsync("", false, embed);
        }

        private async void UnmetPrecondition(IResult result, CommandContext context)
        {
            var embed = new EmbedBuilder();
            embed.Color = new Color(150, 16, 25);
            embed.Title = "UnMet PreCondition!";
            embed.Description = result.ErrorReason.ToString();
            await context.Channel.SendMessageAsync("", false, embed);
        }

        private async void ParseFailed(IResult result, CommandContext context)
        {
            var embed = new EmbedBuilder();
            embed.Color = new Color(150, 16, 25);
            embed.Title = "Parsing Failed!";
            embed.Description = result.ErrorReason.ToString();
            await context.Channel.SendMessageAsync("", false, embed);
        }
    }
}