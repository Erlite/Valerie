using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Commands;
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

            var ServiceProvider = InjectServices();
            CommandHandler = new CommandHandler(ServiceProvider);
            await CommandHandler.ConfigureCommandsAsync();

            Client.Log += (log) => Task.Run(() => Log.Write(Status.KAY, Source.Client, log.Message));
            Client.GuildAvailable += EventsHandler.GuildAvailableAsync;
            Client.JoinedGuild += EventsHandler.JoinedGuildAsync;
            Client.LeftGuild += EventsHandler.LeftGuildAsync;
            Task.Run(() => Client.MessageReceived += EventsHandler.MessageReceivedAsync);
            Client.UserJoined += EventsHandler.UserJoinedAsync;
            Client.UserLeft += EventsHandler.UserLeftAsync;
            Client.ReactionAdded += EventsHandler.ReactionAddedAsync;
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

        IServiceProvider InjectServices()
        {
            var Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(new Audio())
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    ThrowOnError = false
                }));

            var Provider = new DefaultServiceProviderFactory().CreateServiceProvider(Services);
            return Provider;
        }
    }
}