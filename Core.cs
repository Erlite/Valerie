using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;
using Discord.Addons.InteractiveCommands;
using Discord.Net.Providers.WS4Net;

namespace Rick
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();
        private DiscordSocketClient client;
        private CommandHandler handler;
        private GuildHandler GuildModel;

        public async Task StartAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = WS4NetProvider.Instance,
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 100000,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                HandlerTimeout = 5000
            });
            
            client.Log += (log) => Task.Run(() => ConsoleService.Log(log.Severity, log.Source, log.Exception?.ToString() ?? log.Message));

            var map = new DependencyMap();
            map.Add(client);
            map.Add(new GuildHandler());
            map.Add(new BotHandler());
            map.Add(new InteractiveService(client));
            map.Add(new EventService(client, GuildModel));

            handler = new CommandHandler(map);
            await handler.ConfigureAsync();

            client.GuildAvailable += EventService.CreateGuildConfigAsync;
            client.LeftGuild += EventService.RemoveGuildConfigAsync;
            client.Ready += EventService.OnReady;

            GuildHandler.GuildConfigs = await GuildHandler.LoadServerConfigsAsync<GuildHandler>();
            BotHandler.BotConfig = await BotHandler.LoadConfigAsync();

            ConsoleService.TitleCard($"{BotHandler.BotConfig.BotName} v{BotHandler.BotVersion}");
            await MethodService.ProgramUpdater();
            
            await client.LoginAsync(TokenType.Bot, BotHandler.BotConfig.BotToken);
            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}