using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Models;

namespace Valerie.Services.RestService
{
    public class RestServer
    {
        readonly HttpClient HttpClient;
        readonly string RestBaseUrl;
        public RestServer(string Url, HttpClient Client)
        {
            HttpClient = Client;
            RestBaseUrl = Url;
        }

        public Task<HttpResponseMessage> ServerListAsync() => HttpClient.GetAsync(RestBaseUrl);

        public Task<HttpResponseMessage> GetServerAsync(ulong Id) => HttpClient.GetAsync($"{RestBaseUrl}{Id}");

        public Task<HttpResponseMessage> AddServerAsync(ServerModel NewServer)
            => HttpClient.PostAsync(RestBaseUrl, new StringContent(JsonConvert.SerializeObject(NewServer, Formatting.Indented), Encoding.UTF8, "application/json"));

        public Task<HttpResponseMessage> UpdateServerAsync(ulong Id, ServerModel ServerUpdate)
            => HttpClient.PutAsync($"{RestBaseUrl}{Id}", new StringContent(JsonConvert.SerializeObject(ServerUpdate, Formatting.Indented), Encoding.UTF8, "application/json"));

        public Task<HttpResponseMessage> DeleteServerAsync(ulong Id) => HttpClient.DeleteAsync($"{RestBaseUrl}{Id}");
    }
}