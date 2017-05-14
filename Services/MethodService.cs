using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.Net;
using Rick.Handlers;
using System.Diagnostics;
using Discord.WebSocket;
using CleverbotLib.Models;

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
            var BotConfig = BotHandler.BotConfig;
            if (BotConfig.AutoUpdate)
            {
                ConsoleService.Log("Autoupdate", "Checking for updates ...");
                var Http = new HttpClient();
                var GetUrl = await Http.GetStringAsync("https://exceptiondev.github.io/Docs/Downloads/version.txt");
                double version = Convert.ToDouble(GetUrl);
                if (BotHandler.BotVersion < version)
                {
                    ConsoleService.Log("Autoupdate", $"New version is available! Version: {version}.\nWould you like to update now? ");
                    ConsoleService.Log("Autoupdate", "Please type Yes to update! ");
                    var Response = Console.ReadLine().ToLower();
                    if (Response == "yes")
                    {
                        ConsoleService.Log("Autoupdate", "Downloading update ...");
                        Uri url = new Uri("https://exceptiondev.github.io/Docs/Downloads/Installer.bat");
                        await Http.DownloadAsync(url, "Installer.bat");
                        Process.Start("Installer.bat");
                        await Task.Delay(5000);
                        Process.GetCurrentProcess().Kill();
                    }
                    else
                        ConsoleService.Log("Autoupdate", "Wrong response! Continuing...");
                }
                else
                    ConsoleService.Log("Autoupdate", "Already using the latest version!\n");
            }
            else
                ConsoleService.Log("Autoupdate", "Autoupdate is disabled! Continuing ...\n");
        }

        public static double GiveKarma(double karma)
        {
            return (Math.Pow(karma, 2) + 10 * karma) / 5;
        }

        public static int GetLevelFromXP(double karma)
        {
            return Convert.ToInt32(Math.Sqrt(karma) / 5);
        }

        public static async Task AfkAsync(SocketUserMessage message, SocketGuild gld)
        {
            var AfkList = GuildHandler.GuildConfigs[gld.Id].AfkList;
            string afkReason = null;
            SocketUser gldUser = message.MentionedUsers.FirstOrDefault(u => AfkList.TryGetValue(u.Id, out afkReason));
            if (gldUser != null)
                await message.Channel.SendMessageAsync(afkReason);
        }

        public static async Task ChatKarma(SocketUserMessage message, SocketGuild gld)
        {
            var Guilds = GuildHandler.GuildConfigs[gld.Id];
            if (message.Author.IsBot || !Guilds.ChatKarma) return;
            Random rand = new Random();
            double RandomKarma = rand.Next(1, 5);
            RandomKarma = GiveKarma(RandomKarma);
            if (Guilds.ChatKarma)
            {
                var karmalist = Guilds.Karma;
                if (!karmalist.ContainsKey(message.Author.Id))
                    karmalist.Add(message.Author.Id, 1);
                else
                {
                    int getKarma = karmalist[message.Author.Id];
                    getKarma += Convert.ToInt32(RandomKarma);
                    karmalist[message.Author.Id] = getKarma;
                }
                GuildHandler.GuildConfigs[gld.Id] = Guilds;
                await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            }
        }

        public static async Task CleverBot(SocketUserMessage message, SocketGuild gld)
        {
            var IsEnabled = GuildHandler.GuildConfigs[gld.Id].ChatterBot;
            if (message.Author.IsBot || !message.Content.StartsWith(BotHandler.BotConfig.BotName) || !IsEnabled) return;
            CleverbotResponse Response = null;
            Response = CleverbotLib.Core.Talk(message.Content, Response);
            await message.Channel.SendMessageAsync(Response.Output);
        }
    }
}