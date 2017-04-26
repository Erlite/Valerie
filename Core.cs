using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;
using Discord.Addons.InteractiveCommands;
using Discord.Net.Providers.WS4Net;
using Rick.Models;

namespace Rick
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();
        private DiscordSocketClient client;
        private CommandHandler handler;
        private GuildModel GuildModel;

        public async Task StartAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = WS4NetProvider.Instance,
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 10000,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });
            
            client.Log += (log) => Task.Run(() => ConsoleService.Log(log.Severity, log.Source, log.Exception?.ToString() ?? log.Message));

            var map = new DependencyMap();
            map.Add(client);
            map.Add(new GuildModel());
            map.Add(new InteractiveService(client));
            map.Add(new EventService(client, GuildModel));
            map.Add(new BotModel());
            handler = new CommandHandler(map);
            await handler.InstallAsync();

            client.GuildAvailable += EventService.CreateGuildConfigAsync;
            client.LeftGuild += EventService.RemoveGuildConfigAsync;
            client.Ready += EventService.OnReady;
            client.MessageReceived += EventService.LogMessagesAsync;

            GuildModel.GuildConfigs = await GuildModel.LoadServerConfigsAsync<GuildModel>();
            BotModel.BotConfig = await BotModel.LoadConfigAsync();

            ConsoleService.TitleCard($"{BotModel.BotConfig.BotName} v{BotModel.BotVersion}");
            await MethodService.ProgramUpdater();
            
            await client.LoginAsync(TokenType.Bot, BotModel.BotConfig.BotToken);
            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}