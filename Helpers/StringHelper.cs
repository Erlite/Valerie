using System;
using Discord;
using System.Linq;
using System.Text;
using Valerie.Enums;
using System.Net.Http;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Valerie.Helpers
{
    public class StringHelper
    {
        public static string CheckUser(IDiscordClient client, ulong UserId)
        {
            var Client = client as DiscordSocketClient;
            var User = Client.GetUser(UserId);
            return User == null ? "Unknown User." : User.Username;
        }

        public static string CheckRole(SocketGuild Guild, ulong Id)
        {
            var Role = Guild.GetRole(Id);
            return Role == null ? "Unknown Role." : Role.Name;
        }

        public static string CheckChannel(SocketGuild Guild, ulong Id)
        {
            var Channel = Guild.GetTextChannel(Id);
            return Channel == null ? "Unknown Channel." : Channel.Name;
        }

        public static string Star(int Stars)
        {
            if (Stars <= 5 && Stars > 0) return "⭐";
            else if (Stars > 5) return "🌟";
            else if (Stars > 15) return "💫";
            else return "✨";
        }

        public static string Replace(string Message, string Guild = null, string User = null, int Level = 0, int Crystals = 0)
        {
            StringBuilder Builder = new StringBuilder(Message);
            Builder.Replace("{guild}", Guild);
            Builder.Replace("{user}", User);
            Builder.Replace("{level}", $"{Level}");
            Builder.Replace("{crystals}", $"{Crystals}");
            return Builder.ToString();
        }

        public static async Task<string> NsfwAsync(HttpClient HttpClient, Random Random, string Url, int Max)
        {
            try
            {
                var Parse = JArray.Parse(await HttpClient.GetStringAsync($"{Url}{Random.Next(Max)}").ConfigureAwait(false))[0];
                return ($"{Parse["preview"]}");
            }
            catch { return null; }
        }

        public static async Task<string> HentaiAsync(HttpClient HttpClient, Random Random, NsfwType NsfwType, List<string> Tags)
        {
            string Url = null;
            string Result = null;
            MatchCollection Matches = null;
            Tags = !Tags.Any() ? new[] { "boobs", "tits", "ass", "sexy", "neko" }.ToList() : Tags;
            switch (NsfwType)
            {
                case NsfwType.Danbooru: Url = $"http://danbooru.donmai.us/posts?page={Random.Next(0, 15)}{string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
                case NsfwType.Gelbooru: Url = $"http://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=100&tags={string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
                case NsfwType.Rule34: Url = $"http://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=100&tags={string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
                case NsfwType.Cureninja: Url = $"https://cure.ninja/booru/api/json?f=a&o=r&s=1&q={string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
                case NsfwType.Konachan: Url = $"http://konachan.com/post?page={Random.Next(0, 5)}&tags={string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
                case NsfwType.Yandere: Url = $"https://yande.re/post.xml?limit=25&page={Random.Next(0, 15)}&tags={string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
            }
            var Get = await HttpClient.GetStringAsync(Url).ConfigureAwait(false);
            switch (NsfwType)
            {
                case NsfwType.Danbooru: Matches = Regex.Matches(Get, "data-large-file-url=\"(.*)\""); break;
                case NsfwType.Yandere:
                case NsfwType.Gelbooru:
                case NsfwType.Rule34: Matches = Regex.Matches(Get, "file_url=\"(.*?)\" "); break;
                case NsfwType.Cureninja: Matches = Regex.Matches(Get, "\"url\":\"(.*?)\""); break;
                case NsfwType.Konachan: Matches = Regex.Matches(Get, "<a class=\"directlink smallimg\" href=\"(.*?)\""); break;
            }
            if (!Matches.Any()) return "No results found.";
            switch (NsfwType)
            {
                case NsfwType.Danbooru: Result = $"http://danbooru.donmai.us/{Matches[Random.Next(Matches.Count)].Groups[1].Value}"; break;
                case NsfwType.Konachan:
                case NsfwType.Gelbooru: Result = $"http:{Matches[Random.Next(Matches.Count)].Groups[1].Value}"; break;
                case NsfwType.Yandere:
                case NsfwType.Rule34: Result = $"http:{Matches[Random.Next(Matches.Count)].Groups[1].Value}"; break;
                case NsfwType.Cureninja: Result = Matches[Random.Next(Matches.Count)].Groups[1].Value.Replace("\\/", "/"); break;
            }
            return Result;
        }
    }
}