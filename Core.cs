using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Net.Providers.WS4Net;
using Rick.Services;
using Rick.Handlers;
using Rick.Enums;
using Rick.Models;

namespace Rick
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();
        private DiscordSocketClient client;
        private CommandHandler handler;

        public async Task StartAsync()
        {
            BotHandler.DirectoryCheck();

            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = WS4NetProvider.Instance,
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 10000,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                HandlerTimeout = 1000
            });
            client.Log += (log) => Task.Run(() => Logger.Log(LogType.Info, LogSource.Client, log.Exception?.ToString() ?? log.Message));

            var ServiceProdivder = ConfigureServices();
            handler = new CommandHandler(ServiceProdivder);
            await handler.ConfigureAsync();

            client.GuildAvailable += EventService.CreateGuildConfigAsync;
            client.JoinedGuild += EventService.JoinedGuildAsync;
            client.LeftGuild += EventService.RemoveGuildConfigAsync;
            client.UserLeft += EventService.HandleGuildsTasks;
            client.MessageReceived += EventService.MessageServicesAsync;
            client.Ready += EventService.OnReadyAsync;

            GuildHandler.GuildConfigs = await GuildHandler.LoadServerConfigsAsync<GuildModel>();
            BotHandler.BotConfig = await BotHandler.LoadConfigAsync();

            Logger.TitleCard($"{BotHandler.BotConfig.BotName} v{BotHandler.BotVersion}");
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
                .AddSingleton(new EventService(client))
                .AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false, LogLevel = LogSeverity.Verbose }));

            var Provider = new DefaultServiceProviderFactory().CreateServiceProvider(Services);
            Provider.GetService<GuildHandler>();
            Provider.GetService<BotHandler>();
            Provider.GetService<EventService>();
            Provider.GetService<MsgsService>();
            return Provider;
        }
    }
}