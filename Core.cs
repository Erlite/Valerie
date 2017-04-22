using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;
using Discord.Addons.InteractiveCommands;
using Discord.Net.Providers.WS4Net;
using System.IO;
using Rick.Models;

namespace Rick
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();
        private DiscordSocketClient client;
        private BotConfigHandler config;
        private CommandHandler handler;
        private GuildModel Model;

        public async Task StartAsync()
        {
            #region Config Check
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Config"));
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config", "Config.json")))
                config = await BotConfigHandler.UseCurrentAsync();
            else
                config = await BotConfigHandler.CreateNewAsync();
            #endregion

            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = WS4NetProvider.Instance,
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 10000,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            client.Log += (log) => Task.Run(() => ConsoleService.Log(log.Severity, log.Source, log.Exception?.ToString() ?? log.Message));

            var map = new DependencyMap();
            map.Add(client);
            map.Add(config);
            map.Add(Model);
            map.Add(new InteractiveService(client));
            map.Add(new EventService(client, Model));
            //map.Add(Logger);

            client.GuildAvailable += CreateGuildConfigAsync;
            client.LeftGuild += RemoveGuildConfigAsync;

            GuildModel.GuildConfigs = await GuildModel.LoadServerConfigsAsync<GuildModel>();
            handler = new CommandHandler(map);
            await handler.InstallAsync();

            await client.LoginAsync(TokenType.Bot, config.BotToken);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task CreateGuildConfigAsync(SocketGuild Guild)
        {
            var CreateConfig = new GuildModel();
            GuildModel.GuildConfigs.Add(Guild.Id, CreateConfig);
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs).ConfigureAwait(false);
        }

        private async Task RemoveGuildConfigAsync(SocketGuild Guild)
        {
            ConsoleService.Log(LogSeverity.Warning, Guild.Name, "Config Deleted!");
            if (GuildModel.GuildConfigs.ContainsKey(Guild.Id))
            {
                GuildModel.GuildConfigs.Remove(Guild.Id);
            }
            var path = GuildModel.configPath;
            await GuildModel.SaveAsync(path, GuildModel.GuildConfigs);
        }
    }
}