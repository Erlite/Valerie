using Newtonsoft.Json;
using System.Collections.Generic;
using Rick.Interfaces;
using System.Threading.Tasks;
using System.IO;
using Rick.Models;

namespace Rick.Handlers
{
    public class GuildHandler
    {
        public static Dictionary<ulong, GuildModel> GuildConfigs { get; set; } = new Dictionary<ulong, GuildModel>();

        public const string configPath = "Data/GuildConfig.json";

        public static async Task SaveAsync<T>(Dictionary<ulong, T> configs) where T : IServer
            => File.WriteAllText(configPath, await Task.Run(() => JsonConvert.SerializeObject(configs, Formatting.Indented)));

        public static async Task<Dictionary<ulong, T>> LoadServerConfigsAsync<T> (string path = configPath) where T : IServer, new()
        {
            if (File.Exists(path))
            {
                return await Task.Run(() => JsonConvert.DeserializeObject<Dictionary<ulong, T>>(File.ReadAllText(path)));
            }
            var newConfig = new Dictionary<ulong, T>();
            await SaveAsync(newConfig);
            return newConfig;
        }
    }
}
