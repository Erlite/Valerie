using System;
using System.IO;
using System.Threading.Tasks;
using Tweetinvi;
using Valerie.Handlers.ConfigHandler;
using Valerie.Services.Logger;
using Valerie.Services.Logger.Enums;
using Raven.Client.Documents;

namespace Valerie.Handlers
{
    public class MainHandler
    {
        public static string CacheFolder = Path.Combine(Directory.GetCurrentDirectory(), "Cache");
        static Lazy<IDocumentStore> DocumentStore = new Lazy<IDocumentStore>(CreateDocStore);

        static IDocumentStore CreateDocStore => new DocumentStore()
        {
            Database = "Valerie",
            Urls = new string[] { "http://localhost:8080" }
        }.Initialize();


        public static IDocumentStore Store => DocumentStore.Value;

        public static async Task GetReadyAsync()
        {
            Log.PrintInfo();
            await BotDB.LoadConfigAsync();

            var TwitterConfig = BotDB.Config.APIKeys.TwitterKeys;
            Auth.SetUserCredentials(TwitterConfig.ConsumerKey, TwitterConfig.ConsumerSecret, TwitterConfig.AccessToken, TwitterConfig.AccessTokenSecret);
            var AuthUser = User.GetAuthenticatedUser();
            if (AuthUser == null)
            {
                Log.Write(Status.ERR, Source.Config, ExceptionHandler.GetLastException().TwitterDescription);
            }
            else
                Log.Write(Status.KAY, Source.Config, "Logged into Twitter.");
            if (!Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);
        }
    }
}
