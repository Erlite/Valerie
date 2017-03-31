using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.IO;
using DiscordBot.Services;
using DiscordBot.Handlers;
using Discord.Addons.InteractiveCommands;
using Discord.Net.Providers.WS4Net;

namespace DiscordBot
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();
        private DiscordSocketClient client;
        private MainHandler MainHandler;
        //private LogService log;
        //private CommandHandler handler;
        //private DependencyMap map;


        public async Task StartAsync()
        {
            ConsoleService.TitleCard("Rick", DiscordConfig.Version);
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = WS4NetProvider.Instance,
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 10000,
                AlwaysDownloadUsers = true
            });

            client.Log += (l) => Task.Run(() => ConsoleService.Log(l.Severity, l.Source, l.Exception?.ToString() ?? l.Message));

            //log = new LogService(client, config);
            //await log.LoadConfigurationAsync();
            var map = new DependencyMap();
            map.Add(client);

            MainHandler = new MainHandler(client);
            client.GuildAvailable += MainHandler.GuildAvailableEvent;
            client.LeftGuild += MainHandler.LeftGuildEvent;

            await MainHandler.InitializeEarlyAsync(map);

            await client.LoginAsync(TokenType.Bot, MainHandler.ConfigHandler.GetBotToken());
            await client.StartAsync();
            await Task.Delay(-1);
        }
    }
}