# pragma warning disable 4014

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Valerie.Handlers;
using Valerie.Handlers.Server;
using Valerie.Handlers.Config;

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
                    LogLevel = LogSeverity.Info
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<BotConfig>()
                .AddSingleton<MainHandler>()
                .AddSingleton<ServerConfig>()
                .AddSingleton<EventsHandler>();

            var Provider = Services.BuildServiceProvider();
            Provider.GetRequiredService<MainHandler>().StartAsync(Provider);

            await Task.Delay(-1);
        }
    }
}