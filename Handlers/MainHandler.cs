using Cookie;
using Discord;
using Valerie.Services;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Valerie.Handlers
{
    public class MainHandler
    {
        public static CookieClient Cookie { get; set; }
        readonly DiscordSocketClient Client;
        readonly ConfigHandler ConfigHandler;
        readonly EventsHandler EventsHandler;

        public MainHandler(DiscordSocketClient DiscordParam, ConfigHandler ConfigParam, EventsHandler EventParam)
        {
            Client = DiscordParam;
            EventsHandler = EventParam;
            ConfigHandler = ConfigParam;
        }

        public async Task StartAsync()
        {
            LogClient.AppInfo();
            ConfigHandler.LoadConfig();

            Cookie = new CookieClient(new CookieConfig
            {
                GiphyKey = ConfigHandler.Config.ApplicationKeys.GiphyKey,
                SteamKey = ConfigHandler.Config.ApplicationKeys.SteamKey,
                CleverbotKey = ConfigHandler.Config.ApplicationKeys.CleverBotKey
            });

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