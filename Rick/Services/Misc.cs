using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Rick.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Rick.Services
{
    public class Misc
    {
        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        public static async Task<Watson> Translate(string dest, string text)
        {
            string json = null;

            using (var client = new HttpClient())
            {
                var message = new HttpRequestMessage();
                message.Method = HttpMethod.Post;
                message.Content = new StringContent(text, Encoding.UTF8, "text/plain");
                message.RequestUri = new Uri("https://watson-api-explorer.mybluemix.net/language-translator/api/v2/identify");
                message.Headers.Add("Accept", "text/plain");
                message.Headers.Add("accept", "text/plain");
                var result = await client.SendAsync(message);
                string lang = await result.Content.ReadAsStringAsync();
                message = new HttpRequestMessage();
                message.Method = HttpMethod.Post;
                message.Content = new StringContent("{\"source\":\"" + lang + "\",\"target\": \"" + dest + "\", \"text\":[\"" + text + "\"]}", Encoding.UTF8, "application/json");
                message.RequestUri = new Uri("https://watson-api-explorer.mybluemix.net/language-translator/api/v2/translate");
                message.Headers.Add("Accept", "application/json");
                message.Headers.Add("accept", "application/json");
                result = await client.SendAsync(message);
                json = await result.Content.ReadAsStringAsync();
            }

            return JsonConvert.DeserializeObject<Watson>(json);
        }

        public static IEnumerable<Assembly> GetAssemblies()
        {
            var Assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
            foreach (var a in Assemblies)
            {
                var asm = Assembly.Load(a);
                yield return asm;
            }
            yield return Assembly.GetEntryAssembly();
            yield return typeof(ILookup<string, string>).GetTypeInfo().Assembly;
        }
    }
}
