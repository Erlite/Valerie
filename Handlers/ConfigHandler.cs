using System;
using Valerie.Services;
using Valerie.JsonModels;
using Raven.Client.Documents.Session;
using Raven.Client.Documents;

namespace Valerie.Handlers
{
    public class ConfigHandler
    {
        IDocumentStore Store { get; }
        IDocumentSession Session { get; }

        public ConfigHandler(IDocumentStore GetStore, IDocumentSession GetSession)
        {
            Store = GetStore;
            Session = GetSession;
        }

        public ConfigModel Config
        {
            get => Session.Load<ConfigModel>("Config");
        }

        public void LoadConfig()
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

        public void Save(ConfigModel GetConfig = null)
        {
            GetConfig = GetConfig ?? Config;
            Session.Store(GetConfig, "Config");
            Session.SaveChanges();
        }
    }
}