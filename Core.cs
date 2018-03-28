using System;
using Discord;
using Valerie.Helpers;
using Valerie.Handlers;
using System.Net.Http;
using Discord.Commands;
using Discord.WebSocket;
using Raven.Client.Documents;
using System.Threading.Tasks;
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
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Error
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    ThrowOnError = true,
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<IDocumentStore>(new DocumentStore
                {
                    Database = "Valerie",
                    Urls = new[] { "http://127.0.0.1:8080" }
                }.Initialize())
                .AddSingleton<GuildHelper>()
                .AddSingleton<MainHandler>()
                .AddSingleton<GuildHandler>()
                .AddSingleton<ConfigHandler>()
                .AddSingleton<EventsHandler>()                
                .AddSingleton(new HttpClient())
                .AddSingleton(new Random(Guid.NewGuid().GetHashCode()));

            var Provider = Services.BuildServiceProvider();
            await Provider.GetRequiredService<MainHandler>().InitializeAsync();
            await Provider.GetRequiredService<EventsHandler>().InitializeAsync(Provider);

            await Task.Delay(-1);
        }
    }
}