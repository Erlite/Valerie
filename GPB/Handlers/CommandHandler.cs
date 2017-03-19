using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord.WebSocket;
using Discord.Commands;
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
            _map.Add(cmds);
            map = _map;
        }

        public async Task InstallAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await cmds.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null)
                return;

            var context = new SocketCommandContext(client, msg);

            int argPos = 0;
            bool hasStringPrefix = config.Prefix == null ? false : msg.HasStringPrefix(config.Prefix, ref argPos);

            if (hasStringPrefix || msg.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var result = await cmds.ExecuteAsync(context, argPos);

                if (!result.IsSuccess)
                {
                    if (result is ExecuteResult r)
                        Console.WriteLine(r.Exception.ToString());
                    else if (result.Error == CommandError.UnknownCommand)
                        await context.Channel.SendMessageAsync("Command not recognized");
                    else
                        await context.Channel.SendMessageAsync(result.ToString());
                }
            }
        }
    }
}