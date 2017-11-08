using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Models;
using Valerie.Services;

namespace Valerie.Handlers
{
    public class ServerHandler
    {
        public async Task AddServerAsync(ServerModel Server)
        {
            var Load = await ServerListAsync().ConfigureAwait(false);
            if (Load.Any(x => x.Id == Server.Id)) return;
            var Get = await MainHandler.RestServer.AddServerAsync(Server).ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode) LogClient.Write(Source.SERVER, $"There was an error adding server: {Server.Id}");
        }

        public async Task DeleteServerAsync(ulong Id)
        {
            var Get = await MainHandler.RestServer.DeleteServerAsync(Id).ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode) LogClient.Write(Source.SERVER, $"There was an error deleting server: {Id}");
        }

        public async Task<bool> UpdateServerAsync(ulong Id, ServerModel Server)
        {
            var TryUpdate = await MainHandler.RestServer.UpdateServerAsync(Id, Server).ConfigureAwait(false);
            if (!TryUpdate.IsSuccessStatusCode) LogClient.Write(Source.SERVER, $"Failed to update server: {Id}");
            return true;
        }

        public async Task<IReadOnlyCollection<ServerModel>> ServerListAsync()
        {
            var Get = await MainHandler.RestServer.ServerListAsync().ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode) { }
            var Content = await Get.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<IReadOnlyCollection<ServerModel>>(Content);
        }

        public async Task<ServerModel> GetServerAsync(ulong Id)
        {
            var Get = await MainHandler.RestServer.GetServerAsync(Id).ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode) LogClient.Write(Source.SERVER, $"Failed to get server: {Id}");
            var Content = await Get.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<ServerModel>(Content);
        }
    }
}