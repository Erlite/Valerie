using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;
using System.Reflection;
using Rick.Handlers;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Services;
using Rick.Enums;
using Rick.JsonResponse;

namespace Rick.Services
{
    public static class MethodsService
    {
        public static async Task DownloadAsync(this HttpClient client, Uri requestUri, string filename)
        {
            using (client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    using (
                        Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                               stream = new FileStream
                                   (filename, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
                    {
                        await contentStream.CopyToAsync(stream).ConfigureAwait(false);
                    }
                }
            }
        }

        public static Task<string> GetE621ImageLink(string tag) => Task.Run(async () =>
        {
            try
            {
                using (var http = new HttpClient())
                {
                    http.Headers();
                    var data = await http.GetStreamAsync("http://e621.net/post/index.xml?tags=" + tag).ConfigureAwait(false);
                    var doc = new XmlDocument();
                    doc.Load(data);
                    var nodes = doc.GetElementsByTagName("file_url");

                    var node = nodes[new Random().Next(0, nodes.Count)];
                    return node.InnerText;
                }
            }
            catch
            {
                return null;
            }
        });

        public static void Headers(this HttpClient http)
        {
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1");
            http.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        }

        public static IEnumerable<Assembly> GetAssemblies()
        {
            var Assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
            foreach (var a in Assemblies)
            {
                var asm = Assembly.Load(a);
                yield return asm;
            }
            yield return Assembly.GetEntryAssembly();
            yield return typeof(ILookup<string, string>).GetTypeInfo().Assembly;
        }

        public static async Task ProgramUpdater()
        {
            var BotConfig = BotHandler.BotConfig;
            if (BotConfig.AutoUpdate)
            {
                ConsoleService.Log(LogType.Info, LogSource.Configuration, "Checking for updates ..");
                var Http = new HttpClient();
                var GetUrl = await Http.GetStringAsync("https://exceptiondev.github.io/Downloads/version.txt");
                double version = Convert.ToDouble(GetUrl);
                if (BotHandler.BotVersion < version)
                {
                    ConsoleService.Log(LogType.Info, LogSource.Configuration, $"New version is available! Version: {version}.\nWould you like to update now? ");
                    ConsoleService.Log(LogType.Info, LogSource.Configuration, "Type Yes to update!");
                    var Response = Console.ReadLine().ToLower();
                    if (Response == "yes")
                    {
                        ConsoleService.Log(LogType.Info, LogSource.Configuration, $"Downloading Update .... ");
                        Uri url = new Uri("https://exceptiondev.github.io/Downloads/Installer.bat");
                        await Http.DownloadAsync(url, "Installer.bat");
                        Process.Start("Installer.bat");
                        await Task.Delay(5000);
                        Process.GetCurrentProcess().Kill();
                    }
                    else
                        ConsoleService.Log(LogType.Critical, LogSource.Configuration, $"Ignoring Update ...");
                }
                else
                    ConsoleService.Log(LogType.Info, LogSource.Configuration, $"Already using the latest version: ");
            }
            else
                ConsoleService.Log(LogType.Warning, LogSource.Configuration, $"Update is disabled! Continuing..");
        }

        public static async Task<WatsonModel> Translate(string dest, string text)
        {
            string json = null;

            using (var client = new HttpClient())
            {
                var message = new HttpRequestMessage();
                message.Method = HttpMethod.Post;
                message.Content = new StringContent(text, Encoding.UTF8, "text/plain");
                message.RequestUri = new Uri("https://watson-api-explorer.mybluemix.net/language-translator/api/v2/identify");
                message.Headers.Add("Accept", "text/plain");
                message.Headers.Add("accept", "text/plain");
                var result = await client.SendAsync(message);
                string lang = await result.Content.ReadAsStringAsync();
                message = new HttpRequestMessage();
                message.Method = HttpMethod.Post;
                message.Content = new StringContent("{\"source\":\"" + lang + "\",\"target\": \"" + dest + "\", \"text\":[\"" + text + "\"]}", Encoding.UTF8, "application/json");
                message.RequestUri = new Uri("https://watson-api-explorer.mybluemix.net/language-translator/api/v2/translate");
                message.Headers.Add("Accept", "application/json");
                message.Headers.Add("accept", "application/json");
                result = await client.SendAsync(message);
                json = await result.Content.ReadAsStringAsync();
            }

            return JsonConvert.DeserializeObject<WatsonModel>(json);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string ShortenUrl(string URL)
        {
            var Service = new UrlshortenerService(new BaseClientService.Initializer()
            {
                ApiKey = BotHandler.BotConfig.GoogleAPIKey,
            });
            var Refer = new Google.Apis.Urlshortener.v1.Data.Url();
            Refer.LongUrl = URL;
            return Service.Url.Insert(Refer).Execute().Id;
        }

    }
}