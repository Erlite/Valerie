using System;
using System.Threading.Tasks;
using Valerie.Services;
using Valerie.Handlers.Config.Models;
using Raven.Client.Documents.Session;

namespace Valerie.Handlers.Config
{
    public class BotConfig
    {
        public ConfigModel Config
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

        public Task SaveAsync(ConfigModel GetConfig)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                Session.StoreAsync(GetConfig, id: "Config");
                Session.SaveChangesAsync();
                Session.Dispose();
            }
            return Task.CompletedTask;
        }
    }
}