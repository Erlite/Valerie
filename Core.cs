# pragma warning disable 4014
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Addons.Interactive;
using Valerie.Handlers;
using Valerie.Handlers.Config;
using Valerie.Services.Logger;
using Valerie.Services.Logger.Enums;

namespace Valerie
{
    class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();
        DiscordSocketClient Client;
        CommandHandler CommandHandler;

        async Task StartAsync()
        {
            await Database.GetReadyAsync();

            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 50000
            });

            Client.Log += (log) => Task.Run(() => Log.Write(Status.KAY, Source.Client, log.Message ?? log.Exception.StackTrace));
            Client.GuildAvailable += EventsHandler.GuildAvailableAsync;
            Client.LeftGuild += EventsHandler.LeftGuildAsync;
            Task.Run(() => Client.JoinedGuild += EventsHandler.JoinedGuildAsync);
            Task.Run(() => Client.MessageReceived += EventsHandler.MessageReceivedAsync);
            Task.Run(() => Client.UserJoined += EventsHandler.UserJoinedAsync);
            Task.Run(() => Client.UserLeft += EventsHandler.UserLeftAsync);
            Task.Run(() => Client.ReactionAdded += EventsHandler.ReactionAddedAsync);
            Task.Run(() => Client.ReactionRemoved += EventsHandler.ReactionRemovedAsync);
            Client.Ready += async () =>
            {
                await EventsHandler.ReadyAsync(Client);
            };
            Client.LatencyUpdated += async (int Older, int Newer) =>
            {
                await EventsHandler.LatencyUpdatedAsync(Client, Older, Newer);
            };

            var ServiceProvider = IServiceProvider();
            CommandHandler = new CommandHandler(ServiceProvider);
            await CommandHandler.ConfigureCommandsAsync();

            await Client.LoginAsync(TokenType.Bot, BotConfig.Config.Token);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        IServiceProvider IServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton<InteractiveService>()
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    ThrowOnError = false,
                    DefaultRunMode = RunMode.Async
                }))
                .BuildServiceProvider();
        }
    }
}