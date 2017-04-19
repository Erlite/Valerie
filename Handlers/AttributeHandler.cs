using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rick.Handlers
{
    public class AttributeHandler
    {
        public ulong[] RequiredRole { get; set; }
        public ulong[] RequiredChannel { get; set; }
        public string[] ReqChannel { get; set; }

        public async Task SaveAsync()
        {
            using (var configStream = File.OpenWrite(Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json")))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    var save = JsonConvert.SerializeObject(this);
                    await configWriter.WriteAsync(save);
                }
            }
        }

        public static async Task<AttributeHandler> UseCurrentAsync()
        {
            AttributeHandler result;
            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json")))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var deserializedConfig = await configReader.ReadToEndAsync();
                    result = JsonConvert.DeserializeObject<AttributeHandler>(deserializedConfig);
                    return result;
                }
            }
        }

        public static  async Task<AttributeHandler> CreateNewAsync()
        {
            AttributeHandler result;
            result = new AttributeHandler();

            string directory = Directory.GetCurrentDirectory();

            using (var configStream = File.Create(Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json")))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    var save = JsonConvert.SerializeObject(result, Formatting.Indented);
                    await configWriter.WriteAsync(save);
                }
            }
            return result;
        }
    }
}