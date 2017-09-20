# pragma warning disable 1998, 4014

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Addons.Interactive;
using Valerie.Handlers;
using Valerie.Handlers.Server;
using Valerie.Handlers.Config;

namespace Valerie
{
    class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();
        DiscordSocketClient Client;
        CommandHandler CommandHandler;

        async Task StartAsync()
        {
            await MainHandler.GetReadyAsync().ConfigureAwait(false);

            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info
            });

            CommandHandler = new CommandHandler(IServiceProvider());
            await CommandHandler.ConfigureCommandsAsync();
            EventsHandler.Provider = IServiceProvider();

            Client.Log += EventsHandler.Log;
            Client.JoinedGuild += EventsHandler.JoinedGuild;
            Client.LeftGuild += EventsHandler.LeftGuild;
            Client.GuildAvailable += EventsHandler.GuildAvailable;
            Client.UserJoined += EventsHandler.UserJoined;
            Client.UserLeft += EventsHandler.UserLeft;
            Client.MessageReceived += EventsHandler.MessageReceivedAsync;
            Client.LatencyUpdated += (Older, Newer) => EventsHandler.LatencyUpdated(Client, Older, Newer);
            Client.Ready += async () => EventsHandler.ReadyAsync(Client);

            await Client.LoginAsync(TokenType.Bot, BotConfig.Config.Token);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        IServiceProvider IServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton<ServerConfig>()
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
