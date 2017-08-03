using Raven.Client;
using System;
using System.Threading.Tasks;
using Valerie.Handlers.ConfigHandler.Enum;
using Valerie.Handlers.ConfigHandler.Models;
using Valerie.Services.Logger;
using Valerie.Services.Logger.Enums;

namespace Valerie.Handlers.ConfigHandler
{
    public class BotDB
    {
        static string BotConfig = "BotConfig";

        public static ConfigModel Config
        {
            get
            {
                using (IDocumentSession Session = MainHandler.Store.OpenSession())
                {
                    return Session.Load<ConfigModel>(BotConfig);
                }
            }
        }

        public static async Task LoadConfigAsync()
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var Config = await Session.LoadAsync<ConfigModel>(BotConfig);
                if (Config == null)
                {
                    Log.Write(Status.ERR, Source.BotDatabase, "No Bot's table found! Creating one..");
                    Log.Write(Status.WRN, Source.BotDatabase, "Enter Bot Token: ");
                    string Token = Console.ReadLine();
                    Log.Write(Status.WRN, Source.BotDatabase, "Enter Bot Prefix: ");
                    string Prefix = Console.ReadLine();
                    await Session.StoreAsync(new ConfigModel
                    {
                        Id = BotConfig,
                        Token = Token,
                        Prefix = Prefix
                    });
                    Log.Write(Status.KAY, Source.BotDatabase, "Bot's table created!");
                    await Session.SaveChangesAsync();
                    Session.Dispose();
                }
                else
                    Log.Write(Status.KAY, Source.BotDatabase, "Bot's table loaded.");
            }
        }

        public static async Task UpdateConfigAsync(ConfigValue Model, string Value = null, ulong ID = 0)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var Config = await Session.LoadAsync<ConfigModel>(BotConfig);
                switch (Model)
                {
                    case ConfigValue.CommandUsed: Config.CommandsUsed += 1; break;
                    case ConfigValue.MessageReceived: Config.MessagesReceived += 1; break;
                    case ConfigValue.Prefix: Config.Prefix = Value; break;
                    case ConfigValue.Token: Config.Token = Value; break;
                    case ConfigValue.EvalAdd: Config.EvalImports.Add(Value); break;
                    case ConfigValue.EvalRemove: Config.EvalImports.Remove(Value); break;
                    case ConfigValue.GamesAdd: Config.Games.Add(Value); break;
                    case ConfigValue.GamesRemove: Config.Games.Remove(Value); break;
                    case ConfigValue.BlacklistAdd: Config.Blacklist.Add(ID, Value); break;
                    case ConfigValue.BlacklistRemove: Config.Blacklist.Remove(ID); break;
                    case ConfigValue.GuildWelcome: Config.GuildJoinMessage = Value; break;
                }
                await Session.StoreAsync(Config);
                await Session.SaveChangesAsync();
                Session.Dispose();
            }
        }
    }
}
