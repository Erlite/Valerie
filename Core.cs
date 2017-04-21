using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;
using Discord.Addons.InteractiveCommands;
using Discord.Net.Providers.WS4Net;
using System.IO;

namespace Rick
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();
        private DiscordSocketClient client;
        private ConfigHandler config;
        private CommandHandler handler;
        private GuildHandler GuildHandler;

        public async Task StartAsync()
        {
            #region Config Check
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Config"));
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config", "Config.json")))
            {
                ConsoleService.Log(LogSeverity.Info, "Config", "Config has been Loaded!");
                config = await ConfigHandler.UseCurrentAsync();
                //GuildHandler = await GuildHandler.UseCurrentAsync();
            }
            else
            {
                ConsoleService.Log(LogSeverity.Warning, "Config", "Config Directory created! Time to setup config!");
                config = await ConfigHandler.CreateNewAsync();
                GuildHandler = await GuildHandler.CreateNewAsync();
            }
#endregion

            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = WS4NetProvider.Instance,
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 9000,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            client.Log += (log) => Task.Run(() => ConsoleService.Log(log.Severity, log.Source, log.Exception?.ToString() ?? log.Message));

            var map = new DependencyMap();
            map.Add(client);
            map.Add(config);
            map.Add(new InteractiveService(client));
            map.Add(GuildHandler);

            handler = new CommandHandler(map);
            await handler.InstallAsync();

            await client.LoginAsync(TokenType.Bot, config.BotToken);
            await client.StartAsync();
            await Task.Delay(-1);
        }
    }
}