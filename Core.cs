﻿using System;
using Discord;
using Valerie.Helpers;
using Valerie.Handlers;
using Valerie.Services;
using System.Net.Http;
using Discord.Commands;
using Discord.WebSocket;
using Raven.Client.Documents;
using System.Threading.Tasks;
using Discord.Net.Providers.WS4Net;
using Microsoft.Extensions.DependencyInjection;

namespace Valerie
{
    class Core
    {
        static void Main(string[] args) => new Core().InitializeAsync().GetAwaiter().GetResult();

        async Task InitializeAsync()
        {
            var Database = await DatabaseHandler.LoadDBConfigAsync();

            var Services = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 20,
                    LogLevel = LogSeverity.Error,
                    AlwaysDownloadUsers = true,
#if !OSCHECK
                    WebSocketProvider = WS4NetProvider.Instance
#endif
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    ThrowOnError = true,
                    IgnoreExtraArgs = false,
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<IDocumentStore>(new DocumentStore
                {
                    Certificate = Database.Certificate,
                    Database = Database.DatabaseName,
                    Urls = new[] { Database.DatabaseUrl }
                }.Initialize())
                .AddSingleton<LogService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<GuildHelper>()
                .AddSingleton<EventHelper>()
                .AddSingleton<MainHandler>()
                .AddSingleton<RedditService>()
                .AddSingleton<GuildHandler>()
                .AddSingleton<MethodHelper>()
                .AddSingleton<ConfigHandler>()
                .AddSingleton<EventsHandler>()
                .AddSingleton<WebhookService>()
                .AddSingleton<DatabaseHandler>()
                .AddSingleton(new Random(Guid.NewGuid().GetHashCode()));

            var Provider = Services.BuildServiceProvider();
            Provider.GetRequiredService<LogService>().Initialize();
            await Provider.GetRequiredService<MainHandler>().InitializeAsync();
            await Provider.GetRequiredService<EventsHandler>().InitializeAsync(Provider);
            Provider.GetRequiredService<RedditService>().Initialize();

            await Task.Delay(-1);
        }
    }
}