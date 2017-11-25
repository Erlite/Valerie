using System;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Valerie.Services
{
    public class NsfwService
    {
        public NsfwService(HttpClient GetClient, Random GetRandom)
        {
            Client = GetClient;
            Random = GetRandom;
        }
        Random Random { get; }
        HttpClient Client { get; }
        ConcurrentDictionary<ulong, Timer> RandomNsfw = new ConcurrentDictionary<ulong, Timer>();

        public async Task<string> RuNsfwAsync(string Url, int Max)
        {
            try
            {
                var Parse = JArray.Parse(await Client.GetStringAsync($"{Url}{Random.Next(Max)}").ConfigureAwait(false))[0];
                return ($"{Url}{Parse["preview"]}");
            }
            catch { return null; }
        }
    }
}