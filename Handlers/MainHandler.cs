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
        IDocumentStore Store { get; }
        ConfigHandler Config { get; }
        EventsHandler Events { get; }
        DiscordSocketClient Client { get; }

        public MainHandler(ConfigHandler config, EventsHandler events,
            DiscordSocketClient client, IDocumentStore store)
        {
            Store = store;
            Client = client;
            Config = config;
            Events = events;
        }

        public async Task InitializeAsync(DatabaseHandler Database)
        {
            await DatabaseCheck(Database).ConfigureAwait(false);

            Client.Log += Events.Log;
            Client.Ready += Events.Ready;
            Client.LeftGuild += Events.LeftGuild;
            Client.Connected += Events.Connected;
            Client.UserLeft += Events.UserLeftAsync;
            Client.Disconnected += Events.Disconnected;
            Client.UserJoined += Events.UserJoinedAsync;
            Client.JoinedGuild += Events.JoinedGuildAsync;
            Client.GuildAvailable += Events.GuildAvailable;
            Client.LatencyUpdated += Events.LatencyUpdated;
            Client.ReactionAdded += Events.ReactionAddedAsync;
            Client.MessageReceived += Events.HandleMessageAsync;
            Client.MessageDeleted += Events.MessageDeletedAsync;
            Client.MessageReceived += Events.CommandHandlerAsync;
            Client.ReactionRemoved += Events.ReactionRemovedAsync;

            await Client.LoginAsync(TokenType.Bot, Config.Config.Token).ConfigureAwait(false);
            await Client.StartAsync().ConfigureAwait(false);
        }

        async Task DatabaseCheck(DatabaseHandler DB)
        {
            if (Process.GetProcesses().FirstOrDefault(x => x.ProcessName == "Raven.Server") == null)
            {
                LogService.Write(LogSource.DTB, "Raven Server isn't running. Please make sure RavenDB is running.\nExiting ...", CC.Crimson);
                await Task.Delay(5000);
                Environment.Exit(Environment.ExitCode);
            }
            try
            {
                await DB.DatabaseOptionsAsync(DB, Store).ConfigureAwait(false);
            }
            finally
            {
                Config.ConfigCheck();
            }
        }
    }
}