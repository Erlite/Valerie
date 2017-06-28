using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Database.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Database.Interfaces;
using Database.Wrappers;

namespace Database
{
    public class MainHandler : IServer
    {
        public char Prefix { get; set; } = '!';
        public List<string> WelcomeMessages { get; set; } = new List<string>();
        public ulong MuteRoleID { get; set; }
        public int AdminCases { get; set; }
        public bool IsKarmaEnabled { get; set; }
        public Wrapper JoinEvent { get; set; } = new Wrapper();
        public Wrapper LeaveEvent { get; set; } = new Wrapper();
        public Wrapper AdminLog { get; set; } = new Wrapper();
        public Wrapper Chatterbot { get; set; } = new Wrapper();
        public List<TagsModel> TagsList { get; set; } = new List<TagsModel>();
        public Dictionary<ulong, string> AFKList { get; set; } = new Dictionary<ulong, string>();
        public Dictionary<ulong, int> KarmaList { get; set; } = new Dictionary<ulong, int>();
        public List<string> AssignableRoles { get; set; } = new List<string>();

        [JsonIgnore]
        public const string ConfigFolder = "Data/Configs";

        public static void Checks(ulong ID)
        {
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            if (!Directory.Exists(Path.Combine(ConfigFolder, $"{ID}")))
                Directory.CreateDirectory(Path.Combine(ConfigFolder, $"{ID}"));

            if (!File.Exists(Path.Combine(ConfigFolder, $"{ID}/Config.json")))
                File.Create(Path.Combine(ConfigFolder, $"{ID}/Config.json"));
        }

        public static MainHandler LoadConfig(ulong ID)
        {
            var ConfigFile = Path.Combine(ConfigFolder, $"{ID}/Config.json");
            return JsonConvert.DeserializeObject<MainHandler>(File.ReadAllText(ConfigFile));
        }

        public void SaveAsync(ulong ID)
        {
            var ConfigFile = Path.Combine(ConfigFolder, $"{ID}/Config.json");
            File.WriteAllTextAsync(ConfigFile, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
