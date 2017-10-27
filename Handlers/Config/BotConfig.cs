using System;
using Valerie.Services;
using Valerie.Handlers.Config.Models;

namespace Valerie.Handlers.Config
{
    public class BotConfig
    {
        public static ConfigModel Config => GetConfig();

        static ConfigModel GetConfig()
        {
            ConfigModel model;
            using (var session = MainHandler.Store.OpenSession())
            {
                model = session.Load<ConfigModel>("Config");
                session.Dispose();
                return model;
            }
        }

        public void LoadConfig()
        {
            Logger.PrintInfo();
            using (var Session = MainHandler.Store.OpenSession())
            {
                if (Session.Load<ConfigModel>("Config") == null)
                {
                    Logger.Write(Status.ERR, Source.Config, "No config found! Creating one ...");
                    Logger.Write(Status.WRN, Source.Config, "Input Token: ");
                    string Token = Console.ReadLine();
                    Logger.Write(Status.WRN, Source.Config, "Input Prefix: ");
                    string Prefix = Console.ReadLine();
                    Session.Store(new ConfigModel
                    {
                        Id = "Config",
                        Token = Token,
                        Prefix = Prefix
                    });
                    Session.SaveChanges();
                    Session.Dispose();
                }
                else Logger.Write(Status.KAY, Source.Config, "Config has been locked and loaded!");
            }
        }

        public void Save(ConfigModel GetConfig)
        {
            using (var Session = MainHandler.Store.OpenSession())
            {
                Session.Store(GetConfig, "Config");
                Session.SaveChanges();
                Session.Dispose();
            }
        }
    }
}