﻿using System;
using System.Xml;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Valerie.Handlers.Config;

namespace Valerie.Extensions
{
    public class StringExtension
    {
        static BotConfig BotConfig;
        public StringExtension(BotConfig Config) => BotConfig = Config;

        public static string ReplaceWith(string Msg, string ValueOne, string ValueTwo)
        {
            StringBuilder Builder = new StringBuilder(Msg);
            Builder.Replace("{user}", ValueOne);
            Builder.Replace("{guild}", ValueTwo);
            Builder.Replace("{gprefix}", ValueOne);
            Builder.Replace("{bprefix}", ValueTwo);
            Builder.Replace("{rank}", ValueTwo);
            return Builder.ToString();
        }

        public static string Censor(string Text)
        {
            Regex Swear = new Regex(BotConfig.Config.CensoredWords, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return Swear.Replace(Text, "BEEP");
        }

        public static string ShortenUrl(string URL)
        {
            var Service = new UrlshortenerService(new BaseClientService.Initializer()
            {
                ApiKey = BotConfig.Config.APIKeys.GoogleKey
            });
            var Refer = new Google.Apis.Urlshortener.v1.Data.Url();
            Refer.LongUrl = URL;
            return Service.Url.Insert(Refer).Execute().Id;
        }

        public static async Task<string> MashapeHeaders(string Headers, string Link)
        {
            var Client = new HttpClient();
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("X-Mashape-Key", BotConfig.Config.APIKeys.MashapeKey);
            Client.DefaultRequestHeaders.Add("Accept", Headers);
            return await Client.GetStringAsync(Link);
        }

        public static async Task<string> GetE621ImageLinkAsync(string tag)
        {
            var Client = new HttpClient();
            HTTPExtension.Headers(Client);
            var data = await Client.GetStreamAsync("http://e621.net/post/index.xml?tags=" + tag).ConfigureAwait(false);
            var doc = new XmlDocument();
            doc.Load(data);
            var nodes = doc.GetElementsByTagName("file_url");
            var node = nodes[new Random().Next(0, nodes.Count)];
            return node.InnerText;
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
    }
}
