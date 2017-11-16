using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Valerie.Services;
using Valerie.Services.RestService;
using Cookie;

namespace Valerie.Handlers
{
    public class MainHandler
    {
        public static string ConfigId { get; set; } = "Config";
        public static RestConfig RestConfig { get; set; }
        public static RestServer RestServer { get; set; }
        public static CookieClient Cookie { get; set; }
        readonly HttpClient HttpClient;
        readonly EventsHandler EventsHandler;
        readonly DiscordSocketClient Client;
        readonly ConfigHandler ConfigHandler;

        public MainHandler(HttpClient HttpParam, DiscordSocketClient DiscordParam, ConfigHandler ConfigParam, EventsHandler EventParam)
        {
            Client = DiscordParam;
            HttpClient = HttpParam;
            EventsHandler = EventParam;
            ConfigHandler = ConfigParam;
        }

        public async Task StartAsync()
        {
            LogClient.AppInfo();

            RestConfig = new RestConfig("http://localhost:51117/api/config/", HttpClient);
            RestServer = new RestServer("http://localhost:51117/api/server/", HttpClient);

            await ConfigHandler.ConfigCheckAsync();
            var Config = await ConfigHandler.GetConfigAsync();

            Cookie = new CookieClient(new CookieConfig
            {
                GiphyKey = Config.ApplicationKeys.GiphyKey,
                SteamKey = Config.ApplicationKeys.SteamKey,
                CleverbotKey = Config.ApplicationKeys.CleverBotKey
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

            await Client.LoginAsync(TokenType.Bot, Config.Token);
            await Client.StartAsync();
        }
    }
}