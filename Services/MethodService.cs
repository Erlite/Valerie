using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.Net;
using Rick.Models;
using System.Diagnostics;

namespace Rick.Services
{
    public static class MethodService
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

        public static string LimitLength(this string str, int maxLengh)
        {
            if (str.Length <= maxLengh) return str;
            return str.Substring(0, maxLengh);
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
            var botConfig = BotModel.BotConfig;
            if (botConfig.AutoUpdate)
            {
                ConsoleService.Log("Autoupdate", "Checking for updates ...");
                WebClient web = new WebClient();
                Stream stream = web.OpenRead("https://exceptiondev.github.io/Docs/Downloads/version.txt");
                using (StreamReader reader = new StreamReader(stream))
                {
                    double version = Convert.ToDouble(reader.ReadToEnd());
                    if (BotModel.BotVersion < version)
                    {
                        ConsoleService.Log("Autoupdate", $"New version is available! Version: {version}.\nWould you like to update now? ");
                        var response = Console.ReadLine().ToLower();
                        if (response == "yes")
                        {
                            ConsoleService.Log("Autoupdate", "Downloading update ...");
                            web.DownloadFile("https://exceptiondev.github.io/Docs/Downloads/Installer.bat", "Installer.bat");
                            Process.Start("Installer.bat");
                            await Task.Delay(5000);
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                    else
                        ConsoleService.Log("Autoupdate", "Already using the latest version!\n");
                }
            }
            else
                ConsoleService.Log("Autoupdate", "Autoupdate is disabled! Continuing ...\n");
        }
    }
}