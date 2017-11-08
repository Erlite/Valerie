using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Models;

namespace Valerie.Services.RestService
{
    public class RestConfig
    {
        readonly HttpClient HttpClient;
        readonly string RestBaseUrl;
        public RestConfig(string Url, HttpClient Client)
        {
            HttpClient = Client;
            RestBaseUrl = Url;
        }

        public Task<HttpResponseMessage> ConfigAsync(string Id) => HttpClient.GetAsync(RestBaseUrl + Id);

        public Task<HttpResponseMessage> AddConfigAsync(ConfigModel NewConfig)
            => HttpClient.PostAsync(RestBaseUrl, new StringContent(JsonConvert.SerializeObject(NewConfig, Formatting.Indented), Encoding.UTF8, "application/json"));

        public Task<HttpResponseMessage> UpdateConfigAsync(string Id, ConfigModel Update)
            => HttpClient.PutAsync(RestBaseUrl + Id, new StringContent(JsonConvert.SerializeObject(Update, Formatting.Indented), Encoding.UTF8, "application/json"));
    }
}