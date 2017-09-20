using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Valerie.Services;
using Valerie.Handlers.Config.Models;

namespace Valerie.Handlers.Config
{
    public class BotConfig
    {
        static IDocumentSession BotSession = MainHandler.Store.OpenSession();
        public static ConfigModel Config => BotSession.Load<ConfigModel>("Config");

        public static async Task LoadConfigAsync()
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                if (!await Session.ExistsAsync("Config").ConfigureAwait(false))
                {
                    Logger.Write(Logger.Status.ERR, Logger.Source.Config, "No config found! Creating one ...");
                    Logger.Write(Logger.Status.WRN, Logger.Source.Config, "Input Token: ");
                    string Token = Console.ReadLine();
                    Logger.Write(Logger.Status.WRN, Logger.Source.Config, "Input Prefix: ");
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
                    Logger.Write(Logger.Status.KAY, Logger.Source.Config, "Config has been locked and loaded!");
            }
        }

        public static Task SaveAsync()
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                Session.StoreAsync(Config).ConfigureAwait(false);
                Session.SaveChangesAsync().ConfigureAwait(false);
                Session.Dispose();
            }
            return Task.CompletedTask;
        }
    }
}