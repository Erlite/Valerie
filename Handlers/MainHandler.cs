﻿using System;
using Discord;
using Valerie.Services;
using System.Net.Http;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Valerie.Handlers
{
    public class MainHandler
    {
        ConfigHandler Config { get; }
        HttpClient HttpClient { get; }
        EventsHandler Events { get; }
        DiscordSocketClient Client { get; }
        UpdateService UpdateService { get; }

        public MainHandler(ConfigHandler config, HttpClient httpClient, EventsHandler events, DiscordSocketClient client, UpdateService updateService)
        {
            Client = client;
            Config = config;
            Events = events;
            HttpClient = httpClient;
            UpdateService = updateService;
        }

        public async Task InitializeAsync()
        {
            LogService.PrintApplicationInformation();
            await DatabaseCheck();
            await UpdateService.InitializeAsync();

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
            Client.ReactionRemoved += Events.ReactionRemovedAsync;
            Client.MessageReceived += Events.CommandHandlerAsync;

            await Client.LoginAsync(TokenType.Bot, Config.Config.Token).ConfigureAwait(false);
            await Client.StartAsync().ConfigureAwait(false);
        }

        async Task DatabaseCheck()
        {
            try
            {
                var RavenDb = await HttpClient.GetAsync("http://127.0.0.1:8080/studio/index.html").ConfigureAwait(false);
                var Database = await HttpClient.GetAsync("http://127.0.0.1:8080/studio/index.html#databases/documents?&database=Valerie").ConfigureAwait(false);
                if (RavenDb.IsSuccessStatusCode || Database.IsSuccessStatusCode) Config.ConfigCheck();
            }
            catch
            {
                LogService.Write(nameof(DatabaseCheck), "Either RavenDB isn't running or Database 'Valerie' has not been created.", ConsoleColor.Red);
                await Task.Delay(5000);
                Environment.Exit(Environment.ExitCode);
            }
        }
    }
}