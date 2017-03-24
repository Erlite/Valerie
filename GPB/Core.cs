using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.IO;
using GPB.Services;
using GPB.Handlers;

namespace GPB
{
    class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private ConfigHandler config;
        private LogService log;
        private CommandHandler handler;
        private DependencyMap map;
        private GithubService git;


        public async Task StartAsync()
        {            
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 10000,
                AlwaysDownloadUsers = true
            });

            ConsoleService.TitleCard("Oreos", DiscordConfig.Version);
            client.Log += (l) => Task.Run(() => ConsoleService.Log(l.Severity, l.Source, l.Exception?.ToString() ?? l.Message));

            #region Config
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Config"));
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config", "Config.json")))
            {
                ConsoleService.Log(LogSeverity.Info, "Config", "Config has been Loaded!");
                config = await ConfigHandler.UseCurrentAsync();
            }
            else
            {
                ConsoleService.Log(LogSeverity.Warning, "Config", "Config Directory created! Time to setup config!");
                config = await ConfigHandler.CreateNewAsync();
            }

            #endregion

            log = new LogService(client, config);
            await log.LoadConfigurationAsync();

            map = new DependencyMap();
            map.Add(client);
            map.Add(config);
            map.Add(log);
            map.Add(git);

            handler = new CommandHandler(map);
            await handler.InstallAsync();

            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}