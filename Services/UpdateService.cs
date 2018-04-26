using System;
using System.IO;
using Valerie.Enums;
using System.Drawing;
using Valerie.Handlers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace Valerie.Services
{
    public class UpdateService
    {
        HttpClient HttpClient { get; }
        Timer UpdateTimer { get; set; }
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

        public void InitializeTimer() => UpdateTimer = new Timer(async _ =>
             {
                 LogService.Write(LogSource.UPT, "Checking for updates...", Color.MediumPurple);
                 await DownloadUpdate();
             }, null, TimeSpan.FromSeconds(30), TimeSpan.FromHours(1));

        async Task DownloadUpdate()
        {
            try
            {
                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("appllication/json"));
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", string.Empty);
                var GetProject = await HttpClient.GetAsync("https://ci.appveyor.com/api/projects/Yucked/Valerie").ConfigureAwait(false);
                var ProjectContent = JsonConvert.DeserializeObject<UpdateService>(await GetProject.Content.ReadAsStringAsync().ConfigureAwait(false));
                if (VersionCheck(ProjectContent.Build.Jobs[0].JobId))
                {
                    LogService.Write(LogSource.UPT, "Already using the latest update.", Color.LightCoral);
                    return;
                }
                var GetArtifacts = await HttpClient.GetAsync($"https://ci.appveyor.com/api/buildjobs/{ProjectContent.Build.Jobs[0].JobId}/artifacts").ConfigureAwait(false);
                var ArtifactsContent = JsonConvert.DeserializeObject<UpdateService[]>(await GetArtifacts.Content.ReadAsStringAsync().ConfigureAwait(false));
                HttpClient.DefaultRequestHeaders.Accept.Clear();
                HttpClient.DefaultRequestHeaders.Authorization = null;
                var GetFile = await HttpClient.GetAsync($"https://ci.appveyor.com/api/buildjobs/{ProjectContent.Build.Jobs[0].JobId}/artifacts/{ArtifactsContent[0].FileName}").ConfigureAwait(false);
                await (await GetFile.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    .CopyToAsync(new FileStream($"{ProjectContent.Build.Jobs[0].JobId}.zip", FileMode.Create, FileAccess.Write)).ConfigureAwait(false);
                LogService.Write(LogSource.UPT, "Finished downloading update.", Color.ForestGreen);
                await File.WriteAllTextAsync("version.txt", ProjectContent.Build.Jobs[0].JobId);
            }
            catch
            {
                LogService.Write(LogSource.EXC, "Something went wrong when trying to update!", Color.Crimson);
            }
        }

        bool VersionCheck(string Version)
        {
            if (!File.Exists("version.txt"))
                return false;
            var CurrentVersion = File.ReadAllText("version.txt");
            if (Version != CurrentVersion)
                return false;
            return true;

        }
    }
}