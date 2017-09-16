using System;
using Raven.Client.Documents;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Valerie.Services;
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
            try
            {
                Auth.SetUserCredentials(TwitterConfig.ConsumerKey, TwitterConfig.ConsumerSecret, TwitterConfig.AccessToken, TwitterConfig.AccessTokenSecret);
            }
            catch (TwitterInvalidCredentialsException E)
            {
                Log.Write(Log.Status.ERR, Log.Source.Config, E.Message);
            }
        }
    }
}
