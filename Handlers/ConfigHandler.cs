using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using DiscordBot.Interfaces;

namespace DiscordBot.Handlers
{
    public class ConfigHandler : IHandler
    {
        private JObject config;
        public ConfigHandler()
        {
            config = null;
        }

        public async Task InitializeAsync()
        {
            await LoadAsync();
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                config = JObject.Parse(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}config.json"));
            });
        }

        public string GetBotToken()
        {
            if (config == null)
                return "";
            return (string)config["BotToken"];
        }

        public string GetDefaultCommandPrefix()
        {
            if (config == null)
                return "";
            return (string)config["DefaultCommandPrefix"];
        }

        public string GetBingAPI()
        {
            if (config == null)
                return "";
            return (string)config["BingAPI"];
        }

        public string GetWelcomeMessage()
        {
            if (config == null)
                return "";
            return (string)config["WelcomeMessage"];
        }

        public bool DebugMode()
        {
            if (config == null)
                return true;
            return (bool)config["EnableDebugMode"];
        }
    }
}
