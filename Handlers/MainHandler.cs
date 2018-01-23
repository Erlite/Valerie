using Discord;
using Valerie.Services;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Valerie.Handlers
{
    public class MainHandler
    {
        readonly DiscordSocketClient Client;
        readonly ConfigHandler ConfigHandler;
        readonly EventsHandler EventsHandler;
        readonly UpdateService UpdateService;

        public MainHandler(DiscordSocketClient DiscordParam, ConfigHandler ConfigParam, EventsHandler EventParam, UpdateService UpdateParam)
        {
            Client = DiscordParam;
            EventsHandler = EventParam;
            ConfigHandler = ConfigParam;
            UpdateService = UpdateParam;
        }

        public async Task StartAsync()
        {
            ConfigHandler.LoadConfig();
            LogClient.AppInfo(ConfigHandler.Config.Version);
            await UpdateService.InitializeAsync();

            Client.Log += EventsHandler.LogAsync;
            Client.Ready += EventsHandler.ReadyAsync;
            Client.UserLeft += EventsHandler.UserLeftAsync;
            Client.LeftGuild += EventsHandler.LeftGuildAsync;
            Client.UserJoined += EventsHandler.UserJoinedAsync;
            Client.JoinedGuild += EventsHandler.JoinedGuildAsync;
            Client.UserBanned += EventsHandler.UserBannedAsync;
            Client.GuildAvailable += EventsHandler.GuildAvailableAsync;
            Client.ReactionAdded += EventsHandler.ReactionAddedAsync;
            Client.LatencyUpdated += EventsHandler.LatencyUpdatedAsync;
            Client.MessageReceived += EventsHandler.HandleMessageAsync;
            Client.MessageReceived += EventsHandler.HandleCommandAsync;
            Client.ReactionRemoved += EventsHandler.ReactionRemovedAsync;

            await Client.LoginAsync(TokenType.Bot, ConfigHandler.Config.Token);
            await Client.StartAsync();
        }
    }
}