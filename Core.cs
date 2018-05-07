using System;
using Discord;
using Valerie.Helpers;
using Valerie.Services;
using System.Net.Http;
using Valerie.Handlers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Raven.Client.Documents;
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
                    LogLevel = LogSeverity.Error,
                    AlwaysDownloadUsers = true
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<IDocumentStore>(new DocumentStore
                {
                    Certificate = DatabaseHandler.DBConfig.Certificate,
                    Database = DatabaseHandler.DBConfig.DatabaseName,
                    Urls = new[] { DatabaseHandler.DBConfig.DatabaseUrl }
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
            Provider.GetRequiredService<LogService>().PrintApplicationInformation();
            await Provider.GetRequiredService<DatabaseHandler>().DatabaseCheck();
            await Provider.GetRequiredService<MainHandler>().InitializeAsync();
            await Provider.GetRequiredService<EventsHandler>().InitializeAsync(Provider);
            Provider.GetRequiredService<RedditService>().Initialize();

            await Task.Delay(-1);
        }
    }
}