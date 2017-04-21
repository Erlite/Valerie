using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using AngleSharp;
using AngleSharp.Dom.Html;

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

        public static List<string> EvalImports { get; } = new List<string> {
            "Discord",
            "Discord.Net",
            "Discord.Rest",
            "Discord.Commands",
            "Discord.WebSocket",
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Diagnostics",
            "System.IO",
            "System.Linq",
            "System.Math",
            "System.Reflection",
            "System.Runtime",
            "System.Threading.Tasks"
        };

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

    }
}