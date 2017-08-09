using System;
using System.Net;
using Tweetinvi;
using Cleverbot;
using Valerie.Handlers.ConfigHandler;
using Valerie.Services.Logger;
using Valerie.Services.Logger.Enums;
using System.IO;
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

        public static void ServicesLogin()
        {
            var Config = BotDB.Config;
            var TwitterConfig = Config.APIKeys.TwitterKeys;


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
                Main.SetAPIKey(Config.APIKeys.CleverBotKey);
            }
            catch (WebException Ex)
            {
                Log.Write(Status.ERR, Source.BotDatabase, Ex.Message);
            }
        }

        public static void DirectoryCheck()
        {
            if (!Directory.Exists(CacheFolder))
            {
                Directory.CreateDirectory(CacheFolder);
            }
        }
    }
}
