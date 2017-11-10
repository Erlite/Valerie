using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Models;
using Valerie.Services;
using System;

namespace Valerie.Handlers
{
    public class ServerHandler
    {
        public async Task AddServerAsync(ServerModel Server)
        {
            var Load = await ServerListAsync().ConfigureAwait(false);
            if (Load.Any(x => x.Id == Server.Id)) return;
            var Get = await MainHandler.RestServer.AddServerAsync(Server).ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode) LogClient.Write(Source.REST, $"There was an error adding server: {Server.Id}");
        }

        public async Task DeleteServerAsync(ulong Id)
        {
            var Get = await MainHandler.RestServer.DeleteServerAsync(Id).ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode) LogClient.Write(Source.REST, $"There was an error deleting server: {Id}");
        }

        public async Task<bool> UpdateServerAsync(ulong Id, ServerModel Server)
        {
            var TryUpdate = await MainHandler.RestServer.UpdateServerAsync(Id, Server).ConfigureAwait(false);
            if (!TryUpdate.IsSuccessStatusCode) LogClient.Write(Source.REST, $"Failed to update server: {Id}");
            return true;
        }

        public async Task<IReadOnlyCollection<ServerModel>> ServerListAsync()
        {
            try
            {
                var Get = await MainHandler.RestServer.ServerListAsync().ConfigureAwait(false);
                if (!Get.IsSuccessStatusCode) { LogClient.Write(Source.REST, Get.ReasonPhrase); };
                var Content = await Get.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<IReadOnlyCollection<ServerModel>>(Content);
            }
            catch(Exception Ex)
            {
                LogClient.Write(Source.SERVER, $"-> {Ex.Message}\n{Ex.StackTrace}");
                return await Task.FromException<IReadOnlyCollection<ServerModel>>(Ex);
            }
        }

        public async Task<ServerModel> GetServerAsync(ulong Id)
        {
            try
            {
                var Get = await MainHandler.RestServer.GetServerAsync(Id).ConfigureAwait(false);
                if (!Get.IsSuccessStatusCode) LogClient.Write(Source.REST, $"Failed to get server: {Id}");
                var Content = await Get.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<ServerModel>(Content);
            }
            catch (Exception Ex)
            {
                LogClient.Write(Source.SERVER, $"-> {Ex.Message}\n{Ex.StackTrace}");
                return await Task.FromException<ServerModel>(Ex);
            }
        }
    }
}