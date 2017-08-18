using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Addons.Interactive;
using Valerie.Handlers;
using Valerie.Services;
using Valerie.Services.Logger.Enums;
using Valerie.Services.Logger;
using Valerie.Handlers.ConfigHandler;

namespace Valerie
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();

        DiscordSocketClient Client;
        CommandHandler CommandHandler;

        async Task StartAsync()
        {
            await MainHandler.GetReadyAsync();

            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = Discord.LogSeverity.Info,
                MessageCacheSize = 10000
            });

            var ServiceProvider = IServiceProvider();
            CommandHandler = new CommandHandler(ServiceProvider);
            await CommandHandler.ConfigureCommandsAsync();

            Client.Log += (log) => Task.Run(() => Log.Write(Status.KAY, Source.Client, log.Message));
            Client.GuildAvailable += EventsHandler.GuildAvailableAsync;
            Task.Run(() => Client.JoinedGuild += EventsHandler.JoinedGuildAsync);
            Client.LeftGuild += EventsHandler.LeftGuildAsync;
            Task.Run(() => Client.MessageReceived += EventsHandler.MessageReceivedAsync);
            Client.UserJoined += EventsHandler.UserJoinedAsync;
            Client.UserLeft += EventsHandler.UserLeftAsync;
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

            await Client.LoginAsync(Discord.TokenType.Bot, BotDB.Config.Token);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        IServiceProvider IServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(new AudioService())
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