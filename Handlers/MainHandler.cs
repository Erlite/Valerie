using System;
using Discord;
using Valerie.Enums;
using Valerie.Services;
using System.Net.Http;
using Discord.WebSocket;
using Raven.Client.Documents;
using System.Threading.Tasks;
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
            try
            {
                var Get = await HttpClient.GetAsync($"{Store.Urls[0]}/studio/index.html#databases/documents?&database=Valerie");
                if (!Get.IsSuccessStatusCode)
                {
                    LogService.Write(LogSource.DTB, "Either RavenDB isn't running or Database 'Valerie' has not been created.", CC.IndianRed);
                    await Store.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord("Valerie")));
                    LogService.Write(LogSource.DTB, "Created Database Valerie.", CC.ForestGreen);
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