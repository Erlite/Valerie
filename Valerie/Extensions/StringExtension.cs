using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Google.Apis.YouTube.v3;
using Valerie.Handlers.ConfigHandler;
using System.Xml;

namespace Valerie.Extensions
{
    public class StringExtension
    {
        public static string ReplaceWith(string Msg, string Username, string GuildName)
        {
            StringBuilder sb = new StringBuilder(Msg);
            sb.Replace("{user}", Username);
            sb.Replace("{guild}", GuildName);
            return sb.ToString();
        }

        public static string JoinReplace(string Msg, string GuildPrefix, string BotPrefix)
        {
            StringBuilder sb = new StringBuilder(Msg);
            sb.Replace("{gprefix}", GuildPrefix);
            sb.Replace("{bprefix}", BotPrefix);
            return sb.ToString();
        }

        public static string Censor(string Text)
        {
            Regex Swear = new Regex(BotDB.Config.CensoredWords, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return Swear.Replace(Text, "BEEP");
        }

        public static string ShortenUrl(string URL)
        {
            var Service = new UrlshortenerService(new BaseClientService.Initializer()
            {
                ApiKey = BotDB.Config.APIKeys.GoogleKey
            });
            var Refer = new Google.Apis.Urlshortener.v1.Data.Url();
            Refer.LongUrl = URL;
            return Service.Url.Insert(Refer).Execute().Id;
        }

        public static async Task<string> MashapeHeaders(string Headers, string Link)
        {
            try
            {
                var HTTP = new HttpClient();
                HTTP.DefaultRequestHeaders.Clear();
                HTTP.DefaultRequestHeaders.Add("X-Mashape-Key", BotDB.Config.APIKeys.MashapeKey);
                HTTP.DefaultRequestHeaders.Add("Accept", Headers);
                return await HTTP.GetStringAsync(Link);
            }
            catch (Exception EX)
            {
                return EX.Message;
            }
        }

        public static string Youtube(string Search)
        {
            var Service = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = BotDB.Config.APIKeys.GoogleKey
            });
            var SearchRequest = Service.Search.List("snippet");
            SearchRequest.Q = Search;
            SearchRequest.MaxResults = 1;
            SearchRequest.Type = "video";
            return SearchRequest.Execute().Items.Select(x => x.Id.VideoId).FirstOrDefault();
        }

        public static async Task<string> GetE621ImageLinkAsync(string tag)
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
                return "";
            }
        }

        public static string StarType(int Stars)
        {
            if (Stars <= 5 && Stars > 0)
                return "⭐";
            else if (Stars > 5)
                return "🌟";
            else if (Stars > 15)
                return "💫";
            else
                return "✨";
        }

        public static string Suggestion(string Action, string Channel)
        {
            if (string.IsNullOrWhiteSpace(Channel))
                return $"\n\n**Warn:** {Action} Channel is not set. To Set {Action} Channel: `--Channel {Action} #Some_Channel`";
            else
                return null;
        }
    }
}
