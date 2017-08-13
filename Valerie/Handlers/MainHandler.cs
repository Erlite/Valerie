using System;
using System.IO;
using System.Threading.Tasks;
using Tweetinvi;
using Cleverbot;
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

        static IDocumentStore CreateDocStore()
        {
            return new DocumentStore()
            {
                Database = "Valerie",
                Urls = new string[] { "http://localhost:8080" }
            }.Initialize();
        }

        public static IDocumentStore Store
        {
            get
            {
                return DocumentStore.Value;
            }
        }

        public static async Task GetReadyAsync()
        {
            Log.PrintInfo();
            await BotDB.LoadConfigAsync();

            var TwitterConfig = BotDB.Config.APIKeys.TwitterKeys;
            Auth.SetUserCredentials(TwitterConfig.ConsumerKey, TwitterConfig.ConsumerSecret, TwitterConfig.AccessToken, TwitterConfig.AccessTokenSecret);
            var AuthUser = User.GetAuthenticatedUser();
            if (AuthUser == null)
            {
                Log.Write(Status.ERR, Source.BotDatabase, ExceptionHandler.GetLastException().TwitterDescription);
            }
            else
                Log.Write(Status.KAY, Source.BotDatabase, "Logged into Twitter.");
            try
            {
                Main.SetAPIKey(BotDB.Config.APIKeys.CleverBotKey);
            }
            catch (Cleverbot.Exceptions.CleverbotApiException Ex)
            {
                Log.Write(Status.ERR, Source.BotDatabase, Ex.Message);
            }
            if (!Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);
        }
    }
}
