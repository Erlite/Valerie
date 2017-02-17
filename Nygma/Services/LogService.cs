//using System;
//using System.Threading.Tasks;
//using System.IO;
//using Newtonsoft.Json;
//using Discord;
//using Nygma.Utils;
//using System.Collections.Generic;

//namespace Nygma.Services
//{
//    public class LogService
//    {
//        public async Task SaveAsync()
//        {
//            using (var configStream = File.OpenWrite(Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Messages.json")))
//            {
//                using (var configWriter = new StreamWriter(configStream))
//                {
//                    var save = JsonConvert.SerializeObject(this);
//                    await configWriter.WriteAsync(save);
//                }
//            }
//        }

//        public static async Task<LogService> UseCurrentAsync()
//        {
//            LogService result;
//            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Messages.json")))
//            {
//                using (var configReader = new StreamReader(configStream))
//                {
//                    var deserializedConfig = await configReader.ReadToEndAsync();

//                    result = JsonConvert.DeserializeObject<LogService>(deserializedConfig);
//                    return result;
//                }
//            }
//        }

//        public static async Task<LogService> CreateNewAsync()
//        {
//            LogService result;
//            result = new LogService();

//            string directory = Directory.GetCurrentDirectory();
//            using (var configStream = File.Create(Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Messages.json")))
//            {
//                using (var configWriter = new StreamWriter(configStream))
//                {
//                    var save = JsonConvert.SerializeObject(result, Formatting.Indented);
//                    await configWriter.WriteAsync(save);
//                }
//            }

//            return result;
//        }
//    }
//}
