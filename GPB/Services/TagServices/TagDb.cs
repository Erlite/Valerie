using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;


namespace GPB.Services.TagServices
{
    public class TagDb
    {
        private TagDb() { }

        [JsonProperty("Tags")]
        public List<Tag> Tags { get; set; } = new List<Tag>();

        public static TagDb Load()
        {
            if (File.Exists("Tags.json"))
            {
                var json = File.ReadAllText("Tags.json");
                return JsonConvert.DeserializeObject<TagDb>(json);
            }
            var db = new TagDb();
            db.Save();
            return db;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText("Tags.json", json);
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
