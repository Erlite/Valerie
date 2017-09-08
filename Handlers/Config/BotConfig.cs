using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Valerie.Services;
using Valerie.Handlers.Config.Models;

namespace Valerie.Handlers.Config
{
    public class BotConfig
    {
        static IDocumentSession BotSession = Database.Store.OpenSession();
        public static ConfigModel Config => BotSession.Load<ConfigModel>("Config");

        public static async Task LoadConfigAsync()
        {
            using (IAsyncDocumentSession Session = Database.Store.OpenAsyncSession())
            {
                if (!await Session.ExistsAsync("Config").ConfigureAwait(false))
                {
                    Log.Write(Log.Status.ERR, Log.Source.Config, "No config found! Creating one ...");
                    Log.Write(Log.Status.WRN, Log.Source.Config, "Input Token: ");
                    string Token = Console.ReadLine();
                    Log.Write(Log.Status.WRN, Log.Source.Config, "Input Prefix: ");
                    string Prefix = Console.ReadLine();
                    await Session.StoreAsync(new ConfigModel
                    {
                        Id = "Config",
                        Token = Token,
                        Prefix = Prefix
                    }).ConfigureAwait(false);
                    await Session.SaveChangesAsync().ConfigureAwait(false);
                    Session.Dispose();
                }
                else
                    Log.Write(Log.Status.KAY, Log.Source.Config, "Config has been locked and loaded!");
            }
        }

        public static async Task SaveAsync()
        {
            using (IAsyncDocumentSession Session = Database.Store.OpenAsyncSession())
            {
                await Session.StoreAsync(Config).ConfigureAwait(false);
                await Session.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}