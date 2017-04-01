using System.Threading.Tasks;
using System.Reflection;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using System;
using DiscordBot.ModulesAddon;
using System.Linq;

namespace DiscordBot.Handlers
{
    public class CommandHandler
    {
        private IDependencyMap map;
        private DiscordSocketClient client;
        private CommandService cmds;
        private MainHandler MainHandler;

        public async Task InitializeAsync(MainHandler MainHandler, IDependencyMap _map)
        {
            this.MainHandler = MainHandler;
            client = _map.Get<DiscordSocketClient>();
            cmds = new CommandService();
            map = _map;

            cmds.AddTypeReader<int?>(new NullableTypeReader<int>(int.TryParse));
            await cmds.AddModulesAsync(Assembly.GetEntryAssembly());

            client.MessageReceived += HandleCommand;
        }

        public Task Close()
        {
            client.MessageReceived -= HandleCommand;
            return Task.CompletedTask;
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            var msg = parameterMessage as SocketUserMessage;
            if (msg == null) return;
            if (!(msg.Channel is ITextChannel)) return;
            int argPos = 0;
            if (!(msg.HasStringPrefix(MainHandler.GetCommandPrefix(msg.Channel), ref argPos))) return;
            var context = new CustomCommandContext(client, MainHandler, msg);
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

        private async void DefaultCommandError(ExecuteResult result, SearchResult res, CustomCommandContext context)
        {
            if (MainHandler.ConfigHandler.DebugMode())
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
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        private async void BadArgCount(IResult result, SearchResult res, CustomCommandContext context)
        {
            var embed = new EmbedBuilder();
            embed.Title = "Um.. Shit bro..It ain't working";
            embed.Description = "Enclose parameters in question marks";
           

            embed.AddField(f =>
            {
                f.Name = "Bad Arg Count";
                f.Value = result.ErrorReason;
            });

            await context.Channel.SendMessageAsync("", false, embed);
        }

        private async void UnmetPrecondition(IResult result, CustomCommandContext context)
        {
            var embed = new EmbedBuilder();

            embed.Title = "UnMet PreCondition!";
            embed.Description = result.ErrorReason.ToString();

            await context.Channel.SendMessageAsync("", false, embed);
        }

        private async void ParseFailed(IResult result, CustomCommandContext context)
        {
            var embed = new EmbedBuilder();

            embed.Title = "Parsing Failed!";
            embed.Description = result.ErrorReason.ToString();
            await context.Channel.SendMessageAsync("", false, embed);
        }
    } 


    public class NullableTypeReader<T> : TypeReader
    where T : struct
    {
        public delegate bool TryParse<W>(string str, out T value);
        public TryParse<T> tryParseFunc;
        public NullableTypeReader(TryParse<T> parseFunc)
        {
            tryParseFunc = parseFunc;
        }

        public override Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            T value;
            if (tryParseFunc(input, out value))
                return Task.FromResult(TypeReaderResult.FromSuccess(new Nullable<T>(value)));
            return Task.FromResult(TypeReaderResult.FromSuccess(new Nullable<T>()));
        }
    }
}