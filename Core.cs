using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Rick.Services;
using Rick.Handlers;
using Discord.Addons.InteractiveCommands;
using Discord.Net.Providers.WS4Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using Discord.Commands;

namespace Rick
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();
        private DiscordSocketClient client;
        private CommandHandler handler;

        public async Task StartAsync()
        {
            ProfileService.DirectoryCheck();

            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = WS4NetProvider.Instance,
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 10000,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                HandlerTimeout = 1000
            });
            
            client.Log += (log) => Task.Run(() => ConsoleService.Log(log.Severity, log.Source, log.Exception?.ToString() ?? log.Message));

            var ServiceProdivder = ConfigureServices();

            handler = new CommandHandler(ServiceProdivder);
            await handler.ConfigureAsync();

            client.GuildAvailable += EventService.CreateGuildConfigAsync;
            client.JoinedGuild += EventService.JoinedGuildAsync;
            client.LeftGuild += EventService.RemoveGuildConfigAsync;
            await Task.Run(() => client.MessageReceived += EventService.MessageServicesAsync);
            client.Ready += EventService.OnReadyAsync;

            GuildHandler.GuildConfigs = await GuildHandler.LoadServerConfigsAsync<GuildHandler>();
            BotHandler.BotConfig = await BotHandler.LoadConfigAsync();

            ConsoleService.TitleCard($"{BotHandler.BotConfig.BotName} v{BotHandler.BotVersion}");
            await MethodsService.ProgramUpdater();

            CleverbotLib.Core.SetAPIKey(BotHandler.BotConfig.CleverBotAPIKey);
            await client.LoginAsync(TokenType.Bot, BotHandler.BotConfig.BotToken);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var Services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(new GuildHandler())
                .AddSingleton(new BotHandler())
                .AddSingleton(new ProfilesHandler())
                .AddSingleton(new EventService(client))
                .AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false, LogLevel = LogSeverity.Verbose}))
                .AddSingleton(new InteractiveService(client));

            var Provider = new DefaultServiceProviderFactory().CreateServiceProvider(Services);
            Provider.GetService<GuildHandler>();
            Provider.GetService<BotHandler>();
            Provider.GetService<ProfilesHandler>();
            Provider.GetService<EventService>();
            Provider.GetService<MsgsService>();
            Provider.GetService<ProfileService>();
            return Provider;
        }
    }
}