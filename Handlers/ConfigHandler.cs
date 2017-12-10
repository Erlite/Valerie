using Cookie;
using System;
using Valerie.Services;
using Valerie.JsonModels;
using Raven.Client.Documents;

namespace Valerie.Handlers
{
    public class ConfigHandler
    {
        IDocumentStore Store { get; }

        public ConfigHandler(IDocumentStore GetStore)
        {
            Store = GetStore;
        }

        public ConfigModel Config
        {
            get
            {
                using (var Session = Store.OpenSession())
                    return Session.Load<ConfigModel>("Config");
            }
        }

        public void LoadConfig()
        {
            using (var Session = Store.OpenSession())
            {
                if (Session.Advanced.Exists("Config")) return;
                LogClient.Write(Source.CONFIG, "Enter Token: ");
                string Token = Console.ReadLine();
                LogClient.Write(Source.CONFIG, "Enter Prefix: ");
                string Prefix = Console.ReadLine();
                Session.Store(new ConfigModel
                {
                    Id = "Config",
                    Token = Token,
                    Prefix = Prefix
                });
                Session.SaveChanges();
            }
        }

        public void Save(ConfigModel GetConfig = null)
        {
            GetConfig = GetConfig ?? Config;
            if (GetConfig == null) return;
            using (var Session = Store.OpenSession())
            {
                Session.Store(GetConfig, "Config");
                Session.SaveChanges();
            }
        }

        public CookieClient Cookie
        {
            get => new CookieClient(new CookieConfig
            {
                GiphyKey = Config.ApplicationKeys.GiphyKey,
                SteamKey = Config.ApplicationKeys.SteamKey,
                CleverbotKey = Config.ApplicationKeys.CleverBotKey
            });
        }
    }
}