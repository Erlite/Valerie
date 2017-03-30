using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;


namespace GPB.Services.TagServices
{
    public class TagDb
    {
        //private TagDb() { }
        static string dict = Path.Combine(Directory.GetCurrentDirectory(), "Guilds", "tags.json");
        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; } = new List<Tag>();

        public static TagDb Load()
        {
            if (File.Exists(dict))
            {
                var json = File.ReadAllText(dict);
                return JsonConvert.DeserializeObject<TagDb>(json);
            }
            var db = new TagDb();
            db.Save();
            return db;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(dict, json);
        }
    }

    public sealed class Tag
    {
        public string Name { get; set; }
        public List<string> Aliases { get; set; }
        public ulong OwnerId { get; set; }
        public string Content { get; set; }
    }
}