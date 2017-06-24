using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Services;
using Rick.Enums;
using Rick.JsonResponse;
using Rick.Handlers;
using Tweetinvi;

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
                        System.IO.Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
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

        public static string ShortenUrl(string URL)
        {
            var Service = new UrlshortenerService(new BaseClientService.Initializer()
            {
                ApiKey = BotHandler.BotConfig.APIKeys.GoogleKey
            });
            var Refer = new Google.Apis.Urlshortener.v1.Data.Url();
            Refer.LongUrl = URL;
            return Service.Url.Insert(Refer).Execute().Id;
        }

        public static string Censor(string Text)
        {
            Regex Swear = new Regex(BotHandler.BotConfig.CensoredWords, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return Swear.Replace(Text, "***");
        }

        public static void ServicesLogin()
        {
            var Config = BotHandler.BotConfig;
            var TwitterConfig = Config.Twitter;


            Auth.SetUserCredentials(TwitterConfig.ConsumerKey, TwitterConfig.ConsumerSecret, TwitterConfig.AccessToken, TwitterConfig.AccessTokenSecret);
            var AuthUser = User.GetAuthenticatedUser();
            if (AuthUser == null)
            {
                Logger.Log(LogType.Error, LogSource.Configuration, ExceptionHandler.GetLastException().TwitterDescription);
            }
            else
                Logger.Log(LogType.Info, LogSource.Configuration, "Logged into Twitter!");
            try
            {
                CleverbotLib.Core.SetAPIKey(Config.APIKeys.CleverBotKey);
            }
            catch (WebException Ex)
            {
                Logger.Log(LogType.Error, LogSource.Configuration, Ex.Message);
            }
        }

        public static async Task<string> MashapeHeaders(string Headers, string Link)
        {
            try
            {
                var HTTP = new HttpClient();
                HTTP.DefaultRequestHeaders.Clear();
                HTTP.DefaultRequestHeaders.Add("X-Mashape-Key", BotHandler.BotConfig.APIKeys.MashapeKey);
                HTTP.DefaultRequestHeaders.Add("Accept", Headers);
                return await HTTP.GetStringAsync(Link);
            }
            catch (Exception EX)
            {
                return EX.Message;
            }
        }
    }
}