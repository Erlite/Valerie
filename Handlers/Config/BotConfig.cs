using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Valerie.Handlers.Config.Models;
using Valerie.Services.Logger;
using Valerie.Services.Logger.Enums;

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
                    Log.Write(Status.ERR, Source.Config, "No config found! Creating one ...");
                    Log.Write(Status.WRN, Source.Config, "Input Token: ");
                    string Token = Console.ReadLine();
                    Log.Write(Status.WRN, Source.Config, "Input Prefix: ");
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
                    Log.Write(Status.KAY, Source.Config, "Config has been locked and loaded!");
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