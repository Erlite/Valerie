using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Handlers;

namespace Valerie
{
    public class Start
    {
        static void Main(string[] args) => new Start().InitializeAsync().GetAwaiter().GetResult();

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
                    ThrowOnError = true,
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<MainHandler>()
                .AddSingleton<EventsHandler>()
                .AddSingleton(new HttpClient())
                .AddSingleton<ServerHandler>()
                .AddSingleton<ConfigHandler>();

            var Provider = Services.BuildServiceProvider();
            await Provider.GetRequiredService<MainHandler>().StartAsync();
            await Provider.GetRequiredService<EventsHandler>().InitializeAsync(Provider);

            await Task.Delay(-1);
        }
    }
}