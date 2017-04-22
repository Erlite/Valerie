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
                LogLevel = LogSeverity.Verbose,
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

            client.GuildAvailable += CreateGuildConfigAsync;
            client.LeftGuild += RemoveGuildConfigAsync;

            GuildModel.GuildConfigs = await GuildModel.LoadServerConfigsAsync<GuildModel>();
            BotModel.BotConfig = await BotModel.LoadConfigAsync();

            await client.LoginAsync(TokenType.Bot, BotModel.BotConfig.BotToken);
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