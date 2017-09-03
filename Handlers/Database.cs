using System;
using Raven.Client.Documents;
using System.Threading.Tasks;
using Valerie.Services.Logger;
using Tweetinvi;
using Valerie.Services.Logger.Enums;
using Valerie.Handlers.Config;

namespace Valerie.Handlers
{
    public class Database
    {
        static Lazy<IDocumentStore> DocumentStore = new Lazy<IDocumentStore>(new DocumentStore()
        {
            Urls = new string[]
            {
                "http://localhost:8080"
            },
            Database = "ValerieAlpha"
        }.Initialize());

        public static IDocumentStore Store => DocumentStore.Value;

        public static async Task GetReadyAsync()
        {
            Log.PrintInfo();
            await BotConfig.LoadConfigAsync().ConfigureAwait(false);

            var TwitterConfig = BotConfig.Config.APIKeys.TwitterKeys;
            Auth.SetUserCredentials(TwitterConfig.ConsumerKey, TwitterConfig.ConsumerSecret, TwitterConfig.AccessToken, TwitterConfig.AccessTokenSecret);
            var AuthUser = User.GetAuthenticatedUser();
            if (AuthUser == null)
            {
                Log.Write(Status.ERR, Source.Config, ExceptionHandler.GetLastException().TwitterDescription);
            }
            else
                Log.Write(Status.KAY, Source.Config, "Logged into Twitter.");
        }
    }
}
