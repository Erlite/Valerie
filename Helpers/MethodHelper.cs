using System;
using System.Linq;
using Valerie.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Valerie.Helpers
{
    public class MethodHelper
    {
        HttpClient HttpClient { get; }
        public MethodHelper(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public List<Type> GetNamespaces(string Namespace)
            => Assembly.GetExecutingAssembly().GetTypes().Where(x => String.Equals(x.Namespace, Namespace, StringComparison.Ordinal)).ToList();

        public DateTime UnixDateTime(double Unix)
            => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Unix).ToLocalTime();

        public DateTime EasternTime
            => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Eastern Standard Time");

        public IEnumerable<Assembly> GetAssemblies
        {
            get
            {
                var Assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
                foreach (var Ass in Assemblies) yield return Assembly.Load(Ass);
                yield return Assembly.GetEntryAssembly();
                yield return typeof(ILookup<string, string>).GetTypeInfo().Assembly;
            }
        }

        public async Task<IReadOnlyCollection<GithubModel>> GetCommitsAsync()
        {
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var Request = await HttpClient.GetAsync("https://api.github.com/repos/Yucked/Valerie/commits");
            if (!Request.IsSuccessStatusCode) return null;
            var Content = JsonConvert.DeserializeObject<IReadOnlyCollection<GithubModel>>(await Request.Content.ReadAsStringAsync());
            HttpClient.DefaultRequestHeaders.Clear();
            return Content;
        }
    }
}