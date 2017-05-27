using System.Collections.Generic;
using System.Threading.Tasks;
using Rick.Models;
using System.IO;
using Newtonsoft.Json;
using Rick.Services;

namespace Rick.Handlers
{
    public class ProfilesHandler
    {
        [JsonIgnore]
        public static string CacheFolder = Path.Combine(BotHandler.Data, "Cache");
        [JsonIgnore]
        public static string UserImages = Path.Combine(CacheFolder, "Downloads");
        [JsonIgnore]
        public static string EditImages = Path.Combine(CacheFolder, "Edits");
        [JsonIgnore]
        static string Resources = Path.Combine(CacheFolder, "Resources");
        [JsonIgnore]
        const string SavePath = "Data/Profiles.json";

        [JsonProperty("Profiles")]
        public List<ProfileModel> Profiles = new List<ProfileModel>();

        public static async Task LoadProfilesAsync()
        {
            if (!File.Exists(SavePath))
            {
                var pro = new ProfilesHandler();
                using (var configStream = File.Create(Path.Combine(Directory.GetCurrentDirectory(), SavePath)))
                {
                    using (var configWriter = new StreamWriter(configStream))
                    {
                        var save = JsonConvert.SerializeObject(pro, Formatting.Indented);
                        await configWriter.WriteAsync(save);
                    }
                }
            }
            else
            {
                var json = File.ReadAllText(SavePath);
                JsonConvert.DeserializeObject<BotHandler>(json);
            }
        }

        public static void DirectoryCheck()
        {
            if (!Directory.Exists(BotHandler.Data))
            {
                Directory.CreateDirectory(BotHandler.Data);
                ConsoleService.Log("Config", "Creating Data Folder ...");
            }
            if (!Directory.Exists(CacheFolder))
            {
                Directory.CreateDirectory(CacheFolder);
                ConsoleService.Log("Config", "Creating Cache Folder ...");
            }
            if (!Directory.Exists(UserImages))
            {
                Directory.CreateDirectory(UserImages);
                ConsoleService.Log("Config", "Creating User Images Folder ...");
            }
            if (!Directory.Exists(Resources))
            {
                Directory.CreateDirectory(Resources);
                ConsoleService.Log("Config", "Creating Resources Folder ...");
            }
            if (!Directory.Exists(EditImages))
            {
                Directory.CreateDirectory(EditImages);
                ConsoleService.Log("Config", "Creating Images Edit Folder ...");
            }
            var UserImage = Directory.GetFiles(UserImages);
            var EditImage = Directory.GetFiles(EditImages);
            foreach (var x in UserImage)
                File.Delete(x);
        }
    }
}
