using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Commands;
using Rick.Handlers;
using Rick.Services;
using Rick.Services.Logger.Enums;
using Rick.Services.Logger;
using Rick.Handlers.ConfigHandler;

namespace Rick
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();

        DiscordSocketClient Client;
        CommandHandler CommandHandler;

        async Task StartAsync()
        {
            await BotDB.LoadConfigAsync();
            MainHandler.DirectoryCheck();
            MainHandler.ServicesLogin();

            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = Discord.LogSeverity.Info
            });

            var ServiceProvider = InjectServices();
            CommandHandler = new CommandHandler(ServiceProvider);
            await CommandHandler.ConfigureCommandsAsync();

            Client.Log += (log) => Task.Run(() => Log.Write(Status.KAY, Source.Client, log.Message));
            Client.GuildAvailable += EventsHandler.GuildAvailableAsync;
            Client.JoinedGuild += EventsHandler.JoinedGuildAsync;
            Client.LeftGuild += EventsHandler.LeftGuildAsync;
            Client.MessageReceived += EventsHandler.MessageReceivedAsync;
            Client.UserJoined += EventsHandler.UserJoinedAsync;
            Client.UserLeft += EventsHandler.UserLeftAsync;
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