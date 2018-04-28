using System;
using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Services;
using System.Net.Http;
using System.Diagnostics;
using Discord.WebSocket;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using CC = System.Drawing.Color;
using Raven.Client.ServerWide.Operations;

namespace Valerie.Handlers
{
    public class MainHandler
    {
        ConfigHandler Config { get; }
        HttpClient HttpClient { get; }
        EventsHandler Events { get; }
        IDocumentStore Store { get; }
        DiscordSocketClient Client { get; }

        public MainHandler(ConfigHandler config, HttpClient httpClient, EventsHandler events, DiscordSocketClient client, IDocumentStore store)
        {
            Store = store;
            Client = client;
            Config = config;
            Events = events;
            HttpClient = httpClient;
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
                if (!Store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 5)).Any(x => x == Database.DatabaseName))
                {
                    LogService.Write(LogSource.DTB, $"No Database named {Database.DatabaseName} found! Creating Database {Database.DatabaseName}...", CC.IndianRed);
                    await Store.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(Database.DatabaseName)));
                    LogService.Write(LogSource.DTB, $"Created Database {Database.DatabaseName}.", CC.ForestGreen);
                }
            }
            catch { }
            finally
            {
                Config.ConfigCheck();
            }
        }
    }
}