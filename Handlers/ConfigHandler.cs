using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Models;
using Valerie.Services;

namespace Valerie.Handlers
{
    public class ConfigHandler
    {
        public async Task ConfigCheckAsync()
        {
            var Get = await MainHandler.RestConfig.ConfigAsync(MainHandler.ConfigId).ConfigureAwait(false);
            if (Get.ReasonPhrase.Contains("No Content"))
            {
                LogClient.Write(Source.REST, "Config wasn't found. Initializing One ...");
                LogClient.Write(Source.CONFIG, "Enter Default Prefix: ");
                string Prefix = Console.ReadLine();
                LogClient.Write(Source.CONFIG, "Enter Token: ");
                string Token = Console.ReadLine();
                var NewConfig = await MainHandler.RestConfig.AddConfigAsync(new ConfigModel
                {
                    Id = MainHandler.ConfigId,
                    Prefix = Prefix,
                    Token = Token
                }).ConfigureAwait(false);
                if (!NewConfig.IsSuccessStatusCode)
                    LogClient.Write(Source.REST, "Failed to create new config.");
                else
                    LogClient.Write(Source.REST, "Config Created Succesfully.");
            }
        }

        public async Task<ConfigModel> GetConfigAsync()
        {
            var Get = await MainHandler.RestConfig.ConfigAsync(MainHandler.ConfigId).ConfigureAwait(false);
            var Content = await Get.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<ConfigModel>(Content);
        }

        public async Task<bool> UpdateConfigAsync(ConfigModel Update)
        {
            var TryUpdate = await MainHandler.RestConfig.UpdateConfigAsync(MainHandler.ConfigId, Update).ConfigureAwait(false);
            if (!TryUpdate.IsSuccessStatusCode)
            {
                LogClient.Write(Source.REST, "Failed to update config.");
                return false;
            }
            return true;
        }
    }
}