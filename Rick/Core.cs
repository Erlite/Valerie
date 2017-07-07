using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rick.Handlers;
using Rick.Controllers;
using Rick.Models;
using Rick.Functions;

namespace Rick
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();

        DiscordSocketClient Client;
        CommandHandler CommandHandler;

        async Task StartAsync()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });

            Client.Log += (Log) => Task.Run(() 
                => Logger.Log(Enums.LogType.Info, Enums.LogSource.Client, Log.Message));

            ConfigHandler.DirectoryCheck();
            ConfigHandler.IConfig = await ConfigHandler.LoadConfigAsync();
            GuildHandler.GuildConfigs = await GuildHandler.LoadServerConfigsAsync<GuildModel>();

            #region Events
            Client.UserJoined += Events.UserJoinedAsync;
            Client.UserLeft += Events.UserLeftAsync;
            Client.LeftGuild += Events.DeleteGuildConfig;
            Client.GuildAvailable += Events.HandleGuildConfigAsync;
            Client.MessageReceived += Events.HandleGuildMessagesAsync;
            Client.JoinedGuild += async (Guild) =>
            {
                await Events.JoinedGuildAsync(Guild);
            };
            Client.Ready += async () =>
            {
                await Events.OnReadyAsync(Client);
            };
            Client.LatencyUpdated += async (int Older, int Newer) =>
            {
                await Events.LatencyHandlerAsync(Client, Older, Newer);
            };
            #endregion

            var ServiceProvider = InjectServices();

            CommandHandler = new CommandHandler(ServiceProvider);
            await CommandHandler.ConfigureCommandsAsync();

            Function.ServicesLogin();

            await Client.LoginAsync(TokenType.Bot, ConfigHandler.IConfig.Token);
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
