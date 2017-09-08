using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Valerie.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Valerie.Extensions;

namespace Valerie.Services
{
    public class Misc
    {
        public static async Task<Watson> Translate(string dest, string text)
        {
            string json = null;
            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Post;
            message.Content = new StringContent(text, Encoding.UTF8, "text/plain");
            message.RequestUri = new Uri("https://watson-api-explorer.mybluemix.net/language-translator/api/v2/identify");
            message.Headers.Add("Accept", "text/plain");
            message.Headers.Add("accept", "text/plain");
            var result = await new HttpClient().SendAsync(message).ConfigureAwait(false);
            string lang = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            message = new HttpRequestMessage();
            message.Method = HttpMethod.Post;
            message.Content = new StringContent("{\"source\":\"" + lang + "\",\"target\": \"" + dest + "\", \"text\":[\"" + text + "\"]}", Encoding.UTF8, "application/json");
            message.RequestUri = new Uri("https://watson-api-explorer.mybluemix.net/language-translator/api/v2/translate");
            message.Headers.Add("Accept", "application/json");
            message.Headers.Add("accept", "application/json");
            result = await new HttpClient().SendAsync(message).ConfigureAwait(false);
            json = await result.Content.ReadAsStringAsync();
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
