# pragma warning disable 4014
using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Valerie.Services;
using Valerie.Handlers.Config;
using Raven.Client.Documents;

namespace Valerie.Handlers
{
    class MainHandler
    {
        IServiceProvider Provider;
        readonly BotConfig Config;
        readonly DiscordSocketClient Client;
        readonly EventsHandler EventsHandler;
        readonly CommandService CommandService;

        public MainHandler(DiscordSocketClient SocketClient, BotConfig BotConfig, EventsHandler Events, CommandService Commands)
        {
            Client = SocketClient;
            Config = BotConfig;
            EventsHandler = Events;
            CommandService = Commands;
        }

        static Lazy<IDocumentStore> DocumentStore = new Lazy<IDocumentStore>(new DocumentStore()
        {
            Database = "Alpha",
            Urls = new string[]
            {
                "http://localhost:8080"
            }
        }.Initialize());

        public static IDocumentStore Store => DocumentStore.Value;

        public async Task StartAsync(IServiceProvider IServiceProvider)
        {
            Provider = IServiceProvider;

            Logger.PrintInfo();
            await Config.LoadConfigAsync();

            var TwitterConfig = Config.Config.APIKeys.TwitterKeys;
            try
            {
                Auth.SetUserCredentials(TwitterConfig.ConsumerKey, TwitterConfig.ConsumerSecret, TwitterConfig.AccessToken, TwitterConfig.AccessTokenSecret);
            }
            catch (TwitterInvalidCredentialsException E)
            {
                Logger.Write(Logger.Status.ERR, Logger.Source.Config, E.Message);
            }

            CommandService.Log += EventsHandler.Log;
            Client.Log += EventsHandler.Log;
            Client.JoinedGuild += EventsHandler.JoinedGuild;
            Client.LeftGuild += EventsHandler.LeftGuild;
            Client.GuildAvailable += EventsHandler.GuildAvailable;
            Client.UserJoined += EventsHandler.UserJoined;
            Client.UserLeft += EventsHandler.UserLeft;
            Client.MessageReceived += EventsHandler.MessageReceivedAsync;
            Client.LatencyUpdated += (Older, Newer) => EventsHandler.LatencyUpdated(Client, Older, Newer);
            Client.Ready += async () => EventsHandler.ReadyAsync(Client);

            await Client.LoginAsync(Discord.TokenType.Bot, Config.Config.Token);
            await Client.StartAsync();
        }
    }
}
