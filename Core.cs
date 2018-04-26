using System;
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
            var Services = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 20,
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Error,
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
                    Database = "Valerie",
                    Urls = new[] { "http://127.0.0.1:8080" }
                }.Initialize())
                .AddSingleton<HttpClient>()
                .AddSingleton<LogService>()
                .AddSingleton<GuildHelper>()
                .AddSingleton<EventHelper>()
                .AddSingleton<MainHandler>()
                .AddSingleton<GuildHandler>()
                .AddSingleton<ConfigHandler>()
                .AddSingleton<RedditService>()
                .AddSingleton<MethodHelper>()
                .AddSingleton<EventsHandler>()
                .AddSingleton<WebhookService>()
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