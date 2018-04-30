using Discord;
using Discord.WebSocket;
using Raven.Client.Documents;
using System.Threading.Tasks;

namespace Valerie.Handlers
{
    public class MainHandler
    {
        IDocumentStore Store { get; }
        ConfigHandler Config { get; }
        EventsHandler Events { get; }
        DiscordSocketClient Client { get; }

        public MainHandler(ConfigHandler config, EventsHandler events,
            DiscordSocketClient client, IDocumentStore store)
        {
            Store = store;
            Client = client;
            Config = config;
            Events = events;
        }

        public async Task InitializeAsync(DatabaseHandler Database)
        {
            await Database.DatabaseCheck(Database, Store, Config).ConfigureAwait(false);

            Client.Log += Events.Log;
            Client.Ready += Events.Ready;
            Client.LeftGuild += Events.LeftGuild;
            Client.Connected += Events.Connected;
            Client.UserLeft += Events.UserLeftAsync;
            Client.Disconnected += Events.Disconnected;
            Client.UserJoined += Events.UserJoinedAsync;
            Client.JoinedGuild += Events.JoinedGuildAsync;
            Client.GuildAvailable += Events.GuildAvailable;
            Client.LatencyUpdated += Events.LatencyUpdated;
            Client.ReactionAdded += Events.ReactionAddedAsync;
            Client.MessageReceived += Events.HandleMessageAsync;
            Client.MessageDeleted += Events.MessageDeletedAsync;
            Client.MessageReceived += Events.CommandHandlerAsync;
            Client.ReactionRemoved += Events.ReactionRemovedAsync;

            await Client.LoginAsync(TokenType.Bot, Config.Config.Token).ConfigureAwait(false);
            await Client.StartAsync().ConfigureAwait(false);
        }
    }
}