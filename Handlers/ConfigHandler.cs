using System;
using Valerie.Models;
using Valerie.Services;
using Raven.Client.Documents;

namespace Valerie.Handlers
{
    public class ConfigHandler
    {
        IDocumentStore Store { get; }
        public ConfigHandler(IDocumentStore store) => Store = store;

        public ConfigModel Config { get { using (var Session = Store.OpenSession()) return Session.Load<ConfigModel>("Config"); } }

        public void ConfigCheck()
        {
            using (var Session = Store.OpenSession())
            {
                if (Session.Advanced.Exists("Config")) return;
                LogService.Write("Config", "Enter Bot's Token: ", ConsoleColor.DarkCyan);
                string Token = Console.ReadLine();
                LogService.Write("Config", "Enter Bot's Prefix: ", ConsoleColor.DarkCyan);
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