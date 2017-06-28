using Newtonsoft.Json;
using Rick.Interfaces;
using Rick.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Rick.Handlers
{
    public class GuildHandler
    {
        public static Dictionary<ulong, GuildModel> GuildConfigs { get; set; } = new Dictionary<ulong, GuildModel>();

        public const string configPath = "Data/GuildConfig.json";

        public static async Task SaveAsync<T>(Dictionary<ulong, T> configs) where T : IServer
            => File.WriteAllText(configPath, await Task.Run(() => JsonConvert.SerializeObject(configs, Formatting.Indented)).ConfigureAwait(false));

        public static async Task<Dictionary<ulong, T>> LoadServerConfigsAsync<T>() where T : IServer, new()
        {
            if (File.Exists(configPath))
            {
                return JsonConvert.DeserializeObject<Dictionary<ulong, T>>(File.ReadAllText(configPath));
            }
            var newConfig = new Dictionary<ulong, T>();
            await SaveAsync(newConfig);
            return newConfig;
        }
    }
}
