using System;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Raven.Client.Documents;
using Valerie.Handlers.Config;
using Valerie.Services;

namespace Valerie.Handlers
{
    class MainHandler
    {
        static Lazy<IDocumentStore> DocumentStore = new Lazy<IDocumentStore>(new DocumentStore()
        {
            Urls = new string[]
            {
                "http://localhost:8080"
            },
            Database = "Valerie"
        }.Initialize());

        public static IDocumentStore Store => DocumentStore.Value;

        public static async Task GetReadyAsync()
        {
            Logger.PrintInfo();
            await BotConfig.LoadConfigAsync();
            var TwitterConfig = BotConfig.Config.APIKeys.TwitterKeys;
            try
            {
                Auth.SetUserCredentials(TwitterConfig.ConsumerKey, TwitterConfig.ConsumerSecret, TwitterConfig.AccessToken, TwitterConfig.AccessTokenSecret);
            }
            catch (TwitterInvalidCredentialsException E)
            {
                Logger.Write(Logger.Status.ERR, Logger.Source.Config, E.Message);
            }
        }
    }
}
