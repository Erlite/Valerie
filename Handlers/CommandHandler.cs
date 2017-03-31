using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using DiscordBot.Services;
using System.IO;
using System;
using DiscordBot.ModulesAddon;

namespace DiscordBot.Handlers
{
    public class CommandHandler
    {
        private IDependencyMap map;
        private DiscordSocketClient client;
        private CommandService cmds;
        //private LogService log;
        private MainHandler MainHandler;

        public async Task InitializeAsync(MainHandler MainHandler, IDependencyMap _map)
        {
            this.MainHandler = MainHandler;
            client = _map.Get<DiscordSocketClient>();
            //log = _map.Get<LogService>();
            cmds = new CommandService();
            //_map.Add(cmds);
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

        public Task HandleCommand(SocketMessage parameterMessage)
        {
            var msg = parameterMessage as SocketUserMessage;
            if (msg == null) return Task.CompletedTask;
            if (!(msg.Channel is ITextChannel)) return Task.CompletedTask;
            int argPos = 0;
            if (!(msg.HasStringPrefix(MainHandler.GetCommandPrefix(msg.Channel), ref argPos))) return Task.CompletedTask;
            var _ = HandleCommandAsync(msg, argPos);
            return Task.CompletedTask;
        }

        public async Task HandleCommandAsync(SocketUserMessage msg, int argPos)
        {
            var context = new CustomCommandContext(client, MainHandler, msg);
            var result = await cmds.ExecuteAsync(context, argPos, map);
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                if (result.ErrorReason != "null")
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
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