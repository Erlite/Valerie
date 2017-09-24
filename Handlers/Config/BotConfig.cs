using System;
using System.Threading.Tasks;
using Valerie.Services;
using Valerie.Handlers.Config.Models;
using Raven.Client.Documents.Session;

namespace Valerie.Handlers.Config
{
    public class BotConfig
    {
        public static ConfigModel Config
        {
            get
            {
                using (IDocumentSession Session = MainHandler.Store.OpenSession())
                    return Session.Load<ConfigModel>("Config");
            }
        }

        public async Task LoadConfigAsync()
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                if (await Session.LoadAsync<ConfigModel>("Config") == null)
                {
                    Logger.Write(Status.ERR, Source.Config, "No config found! Creating one ...");
                    Logger.Write(Status.WRN, Source.Config, "Input Token: ");
                    string Token = Console.ReadLine();
                    Logger.Write(Status.WRN, Source.Config, "Input Prefix: ");
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
                    Logger.Write(Status.KAY, Source.Config, "Config has been locked and loaded!");
            }
        }

        public async Task SaveAsync(ConfigModel GetConfig)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                await Session.StoreAsync(GetConfig, id: "Config").ConfigureAwait(false);
                await Session.SaveChangesAsync().ConfigureAwait(false);
                Session.Dispose();
            }
        }
    }
}