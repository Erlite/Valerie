using Cookie;
using System;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Services;
using System.Drawing;
using Raven.Client.Documents;

namespace Valerie.Handlers
{
    public class ConfigHandler
    {
        IDocumentStore Store { get; }
        public ConfigHandler(IDocumentStore store) => Store = store;

        public ConfigModel Config { get { using (var Session = Store.OpenSession()) return Session.Load<ConfigModel>("Config"); } }
        public CookieClient Cookie
        {
            get => new CookieClient(new CookieConfig
            {
                GiphyKey = Config.APIKeys["Giphy"],
                SteamKey = Config.APIKeys["Steam"],
                CleverbotKey = Config.APIKeys["Cleverbot"]
            });
        }

        public void ConfigCheck()
        {
            using (var Session = Store.OpenSession())
            {
                if (Session.Advanced.Exists("Config")) return;
                LogService.Write(LogSource.CNF, "Enter Bot's Token: ", Color.LightCoral);
                string Token = Console.ReadLine();
                LogService.Write(LogSource.CNF, "Enter Bot's Prefix: ", Color.LightCoral);
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
    }
}