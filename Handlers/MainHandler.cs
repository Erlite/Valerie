﻿using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Discord.WebSocket;
using Tweetinvi;
using Valerie.Services;
using Valerie.Handlers.Config;

namespace Valerie.Handlers
{
    public class MainHandler
    {
        readonly BotConfig Config;
        readonly DiscordSocketClient Client;
        readonly EventsHandler EventsHandler;

        public MainHandler(DiscordSocketClient SocketClient, BotConfig BotConfig, EventsHandler Events)
        {
            Client = SocketClient;
            Config = BotConfig;
            EventsHandler = Events;
        }

        static Lazy<IDocumentStore> DocumentStore = new Lazy<IDocumentStore>(new DocumentStore()
        {
            Database = "Valerie",
            Urls = new string[]
            {
                "http://localhost:8080"
            }
        }.Initialize());

        public static IDocumentStore Store => DocumentStore.Value;

        public async Task StartAsync(IServiceProvider IServiceProvider)
        {
            ValerieBase<ValerieContext>.Provider = IServiceProvider;
            Logger.PrintInfo();
            await Config.LoadConfigAsync();
            try
            {
                var TwitterConfig = BotConfig.Config.APIKeys.TwitterKeys;
                Auth.SetUserCredentials(TwitterConfig.ConsumerKey, TwitterConfig.ConsumerSecret, TwitterConfig.AccessToken, TwitterConfig.AccessTokenSecret);
                Logger.Write(Status.KAY, Source.Database, $"Twitter Info: {User.UserFactory.GetAuthenticatedUser()?.ScreenName ?? "Not Logged In."}");
            }
            catch (Exception E)
            {
                Logger.Write(Status.ERR, Source.Config, E.StackTrace);
            }

            Client.Log += EventsHandler.Log;
            Client.JoinedGuild += EventsHandler.JoinedGuild;
            Client.LeftGuild += EventsHandler.LeftGuild;
            Client.GuildAvailable += EventsHandler.GuildAvailable;
            Client.UserJoined += EventsHandler.UserJoined;
            Client.UserLeft += EventsHandler.UserLeft;
            Client.UserBanned += EventsHandler.UserBanned;
            Client.MessageReceived += EventsHandler.MessageReceivedAsync;
            Client.ReactionAdded += EventsHandler.ReactionAddedAsync;
            Client.ReactionRemoved += EventsHandler.ReactionRemovedAsync;
            Client.LatencyUpdated += (Older, Newer) => EventsHandler.LatencyUpdated(Client, Older, Newer);
            Client.Ready +=  () => EventsHandler.ReadyAsync(Client);

            await Client.LoginAsync(Discord.TokenType.Bot, BotConfig.Config.Token);
            await Client.StartAsync();
        }
    }
}