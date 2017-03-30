using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using GPB.Services;
using System.IO;

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
            string Dir = Directory.GetCurrentDirectory();
            using (var stream = File.Create(Path.Combine(Directory.GetCurrentDirectory(), "Config", "Exception.txt")))
            {
                using (var Account = new StreamWriter(stream))
                {
                    await Account.WriteAsync(result.Exception.StackTrace);
                }
            }
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
                x.Value = result.Exception.StackTrace.LimitLength(1980);
            });
            await context.Channel.SendMessageAsync("", embed:embed);
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