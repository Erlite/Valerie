using System;
using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Services;
using System.Diagnostics;
using Discord.WebSocket;
using System.Threading.Tasks;
using Raven.Client.Documents;
using CC = System.Drawing.Color;

namespace Valerie.Handlers
{
    public class MainHandler
    {
        DatabaseHandler DB { get; }
        IDocumentStore Store { get; }
        ConfigHandler Config { get; }
        EventsHandler Events { get; }
        DiscordSocketClient Client { get; }

        public MainHandler(ConfigHandler config, EventsHandler events,
            DiscordSocketClient client, IDocumentStore store, DatabaseHandler database)
        {
            Store = store;
            Client = client;
            Config = config;
            Events = events;
            DB = database;
        }

        public async Task InitializeAsync()
        {
            await DatabaseCheck();

            Client.Log += Events.Log;
            Client.Ready += Events.Ready;
            Client.LeftGuild += Events.LeftGuild;
            Client.Connected += Events.Connected;
            Client.UserLeft += Events.UserLeftAsync;
            Client.Disconnected += Events.Disconnected;
            Client.GuildAvailable += Events.GuildAvailable;
            Client.UserJoined += Events.UserJoinedAsync;
            Client.JoinedGuild += Events.JoinedGuildAsync;
            Client.LatencyUpdated += Events.LatencyUpdated;
            Client.ReactionAdded += Events.ReactionAddedAsync;
            Client.MessageReceived += Events.HandleMessageAsync;
            Client.MessageDeleted += Events.MessageDeletedAsync;
            Client.ReactionRemoved += Events.ReactionRemovedAsync;
            Client.MessageReceived += Events.CommandHandlerAsync;

            AppDomain.CurrentDomain.UnhandledException += Events.UnhandledException;

            await Client.LoginAsync(TokenType.Bot, Config.Config.Token).ConfigureAwait(false);
            await Client.StartAsync().ConfigureAwait(false);
        }

        async Task DatabaseCheck()
        {
            var Database = await DatabaseHandler.LoadDBConfigAsync();
            if (Process.GetProcesses().FirstOrDefault(x => x.ProcessName == "Raven.Server") == null)
            {
                LogService.Write(LogSource.DTB, "Raven Server isn't running. Please make sure RavenDB is running.\nExiting ...", CC.Crimson);
                await Task.Delay(5000);
                Environment.Exit(Environment.ExitCode);
            }
            try
            {
                await DB.LoadAndRestoreAsync(Database, Store).ConfigureAwait(false);
            }
            finally
            {
                Config.ConfigCheck();
            }
        }
    }
}