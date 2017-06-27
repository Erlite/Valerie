using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Rick.Models;
using Rick.Interfaces;

namespace Rick.Handlers
{
    public class ConfigHandler
    {
        public static IConfig IConfig { get; set; }

        static string DataFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        public static string CacheFolder = Path.Combine("Data", "Cache");

        public static string ConfigFile = "Data/Config.json";

        public static async Task<ConfigModel> LoadConfigAsync()
        {
            if (File.Exists(ConfigFile))
            {
                return JsonConvert.DeserializeObject<ConfigModel>(await File.ReadAllTextAsync(ConfigFile));
            }
            var NewConfig = await CreateNewAsync();
            return NewConfig;
        }

        static async Task<ConfigModel> CreateNewAsync()
        {
            var Model = new ConfigModel();

            using (var CS = File.Create(ConfigFile))
            {
                using (var CW = new StreamWriter(CS))
                {
                    var save = JsonConvert.SerializeObject(Model, Formatting.Indented);
                    await CW.WriteAsync(save).ConfigureAwait(false);
                }
            }
            return Model;
        }

        public static async Task SaveAsync()
                        => await File.WriteAllTextAsync(ConfigFile, JsonConvert.SerializeObject(IConfig, Formatting.Indented));

        //File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(botConfig, Formatting.Indented));
        public static void DirectoryCheck()
        {
            if (!(Directory.Exists(DataFolder) || Directory.Exists(CacheFolder)))
            {
                Directory.CreateDirectory(DataFolder);
                Directory.CreateDirectory(CacheFolder);
            }
        }
    }
}
