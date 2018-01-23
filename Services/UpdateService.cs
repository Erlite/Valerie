using System.IO;
using System.Linq;
using System.Text;
using Valerie.Handlers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Valerie.Services
{
    public class UpdateService
    {
        LogClient LogClient { get; }
        HttpClient HttpClient { get; }
        ConfigHandler ConfigHandler { get; }

        public UpdateService(HttpClient GetHttp, ConfigHandler GetConfig)
        {
            HttpClient = GetHttp;
            ConfigHandler = GetConfig;
        }

        public async Task InitializeAsync()
        {
            LogClient.Write(Source.CONFIG, $"Checking for updates ....");
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var Get = await HttpClient.GetAsync($"https://api.github.com/repos/yucked/valerie/releases").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode) { LogClient.Write(Source.CONFIG, $"Failed to update Valerie. Error Code: {Get.StatusCode}"); return; }
            var Parse = JsonConvert.DeserializeObject<IReadOnlyList<GitRelease>>(await Get.Content.ReadAsStringAsync().ConfigureAwait(false))[0];
            if (Parse.Id == ConfigHandler.Config.Version) { LogClient.Write(Source.CONFIG, $"Valerie is already running the latest version!"); return; }
            LogClient.Write(Source.CONFIG, $"Downloading update {Parse.Id} ...");

            var Zip = await HttpClient.GetAsync(Parse.ZipballUrl).ConfigureAwait(false);
            string SaveDir = Path.Combine(Directory.GetCurrentDirectory(), "Update");
            if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir);
            using (var Stream = await Zip.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                using (var WriteTo = File.Open($"{SaveDir}/Version{Parse.Id}.zip", FileMode.CreateNew)) await Stream.CopyToAsync(WriteTo).ConfigureAwait(false);
                HttpClient.DefaultRequestHeaders.Clear();
                LogClient.Write(Source.CONFIG, $"Finished downloading update {Parse.Id}!");
            }

            LogClient.Write(Source.CONFIG, $"Extracting update {Parse.Id} ...");
            ZipFile.ExtractToDirectory($"{SaveDir}/Version{Parse.Id}.zip", SaveDir, Encoding.UTF8, false);
            LogClient.Write(Source.CONFIG, $"Finished extracting update {Parse.Id}!");
            File.Delete($"{SaveDir}/Version{Parse.Id}.zip");
            var GetDir = Directory.GetDirectories(SaveDir).FirstOrDefault();
            LogClient.Write(Source.CONFIG, "Changing working directories ...");
            Directory.SetCurrentDirectory(GetDir);
            LogClient.Write(Source.CONFIG, "Starting up new command process ...");
            Process CmdProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "dotnet new console",
                    WorkingDirectory = GetDir
                }
            };
            CmdProcess.Start();
            LogClient.Write(Source.CONFIG, "Process started!");
        }
    }

    public partial class GitRelease
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("zipball_url")]
        public string ZipballUrl { get; set; }
    }
}