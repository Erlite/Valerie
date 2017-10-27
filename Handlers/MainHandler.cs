using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Discord.WebSocket;
using Tweetinvi;
using Valerie.Services;
using Valerie.Handlers.Config;
using Discord.Commands;
using System.Reflection;
using Discord;

namespace Valerie.Handlers
{
    public class MainHandler
    {
        IServiceProvider Provider;
        readonly BotConfig Config;
        readonly DiscordSocketClient Client;
        readonly EventsHandler EventsHandler;
        readonly CommandService CommandService;

        public MainHandler(DiscordSocketClient SocketClient, BotConfig BotConfig, EventsHandler Events, CommandService CmdService)
        {
            Client = SocketClient;
            Config = BotConfig;
            EventsHandler = Events;
            CommandService = CmdService;
        }

        static Lazy<IDocumentStore> DocumentStore = new Lazy<IDocumentStore>(new DocumentStore()
        {
            Database = "Test",
            Urls = new string[] { "http://localhost:8080" }
        }.Initialize());

        public static IDocumentStore Store => DocumentStore.Value;

        public async Task StartAsync(IServiceProvider IServiceProvider)
        {
            Provider = IServiceProvider;
            //ValerieBase<ValerieContext>.Provider = IServiceProvider;
            Config.LoadConfig();
            Auth.SetUserCredentials(BotConfig.Config.APIKeys.TwitterKeys.ConsumerKey, BotConfig.Config.APIKeys.TwitterKeys.ConsumerSecret,
                BotConfig.Config.APIKeys.TwitterKeys.AccessToken, BotConfig.Config.APIKeys.TwitterKeys.AccessTokenSecret);
            Logger.Write(Status.KAY, Source.Database, $"Twitter Info: {User.UserFactory.GetAuthenticatedUser()?.ScreenName ?? "Not Logged In."}");

            Client.Log += EventsHandler.Log;
            Client.JoinedGuild += EventsHandler.JoinedGuild;
            Client.LeftGuild += EventsHandler.LeftGuild;
            Client.GuildAvailable += EventsHandler.GuildAvailable;
            Client.UserJoined += EventsHandler.UserJoined;
            Client.UserLeft += EventsHandler.UserLeft;
            Client.UserBanned += EventsHandler.UserBanned;
            Client.MessageReceived += EventsHandler.MessageReceivedAsync;
            Client.MessageReceived += HandleCommandsAsync;
            Client.ReactionAdded += EventsHandler.ReactionAddedAsync;
            Client.ReactionRemoved += EventsHandler.ReactionRemovedAsync;
            Client.LatencyUpdated += (Older, Newer) => EventsHandler.LatencyUpdated(Client, Older, Newer);
            Client.Ready += () => EventsHandler.ReadyAsync(Client);

            await Client.LoginAsync(Discord.TokenType.Bot, BotConfig.Config.Token);
            await Client.StartAsync();
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        internal async Task HandleCommandsAsync(SocketMessage Message)
        {
            if (!(Message is SocketUserMessage Msg)) return;
            int argPos = 0;
            var Context = new ValerieContext(Client, Msg as IUserMessage, Provider);
            if (!(Msg.HasStringPrefix(Context.ValerieConfig.Prefix, ref argPos) || Msg.HasStringPrefix(Context.Config.Prefix, ref argPos)) ||
                Msg.Source != MessageSource.User || Msg.Author.IsBot || Context.ValerieConfig.UsersBlacklist.ContainsKey(Msg.Author.Id)) return;
            var Result = await CommandService.ExecuteAsync(Context, argPos, Provider, MultiMatchHandling.Best);
            Context.ValerieConfig.CommandUsed++;
            Config.Save(Context.ValerieConfig);

            switch (Result.Error)
            {
                case CommandError.Exception: Logger.Write(Status.ERR, Source.Exception, Result.ErrorReason); break;
                case CommandError.UnmetPrecondition: await Context.Channel.SendMessageAsync(Result.ErrorReason); break;
                case CommandError.Unsuccessful: Logger.Write(Status.ERR, Source.UnSuccesful, Result.ErrorReason); break;
            }
        }
    }
}