using System;
using System.IO;
using System.Linq;
using Valerie.Handlers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace Valerie.Services
{
    public class UpdateService
    {
        HttpClient HttpClient { get; }
        ConfigHandler ConfigHandler { get; }

        [JsonProperty("build")]
        public UpdateService Build { get; set; }
        [JsonProperty("jobs")]
        public UpdateService[] Jobs { get; set; }
        [JsonProperty("jobId")]
        public string JobId { get; set; }
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        public UpdateService(HttpClient httpClient, ConfigHandler configHandler)
        {
            HttpClient = httpClient;
            ConfigHandler = configHandler;
        }

        public async Task InitializeAsync()
        {
            LogService.Write("Update", "Checking for updates ...", ConsoleColor.Yellow);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("appllication/json"));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigHandler.Config.APIKeys["AppVeyor"]);
            var GetProject = await HttpClient.GetAsync("https://ci.appveyor.com/api/projects/Yucked/Valerie").ConfigureAwait(false);
            if (!GetProject.IsSuccessStatusCode)
            {
                LogService.Write("Update", "Something went wrong while trying to get project ...", ConsoleColor.Red);
                return;
            }
            var ProjectContent = JsonConvert.DeserializeObject<UpdateService>(await GetProject.Content.ReadAsStringAsync().ConfigureAwait(false));
            if (ConfigHandler.Config.UpdateId == ProjectContent.Build.Jobs[0].JobId)
            {
                LogService.Write("Update", "Already updated.", ConsoleColor.Green);
                return;
            }
            if (ProjectContent.Build.Jobs[0].JobId == ConfigHandler.Config.UpdateId)
            {
                LogService.Write("Update", "Already using the latest version!", ConsoleColor.Green);
                return;
            }
            var GetArtifacts = await HttpClient.GetAsync($"https://ci.appveyor.com/api/buildjobs/{ProjectContent.Build.Jobs[0].JobId}/artifacts").ConfigureAwait(false);
            if (!GetArtifacts.IsSuccessStatusCode)
            {
                LogService.Write("Update", "Something went wrong while trying to get artifacts ...", ConsoleColor.Red);
                return;
            }
            var ArtifactsContent = JsonConvert.DeserializeObject<UpdateService[]>(await GetArtifacts.Content.ReadAsStringAsync().ConfigureAwait(false));
            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Authorization = null;
            var GetFile = await HttpClient.GetAsync($"https://ci.appveyor.com/api/buildjobs/{ProjectContent.Build.Jobs[0].JobId}/artifacts/{ArtifactsContent[0].FileName}").ConfigureAwait(false);
            if (!GetFile.IsSuccessStatusCode)
            {
                LogService.Write("Update", "Something went wrong while trying to get file ...", ConsoleColor.Red);
                return;
            }

            await (await GetFile.Content.ReadAsStreamAsync().ConfigureAwait(false))
                .CopyToAsync(new FileStream(ArtifactsContent[0].FileName.Split('/').Last(), FileMode.Create, FileAccess.Write)).ConfigureAwait(false);
            LogService.Write("Update", "Finished downloading update.", ConsoleColor.Green);
            ConfigHandler.Config.UpdateId = ProjectContent.Build.Jobs[0].JobId;
            ConfigHandler.Save(ConfigHandler.Config);
        }
    }
}