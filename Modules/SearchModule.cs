using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom.Html;
using System.Linq;
using Rick.Handlers;

namespace Rick.Modules
{
    public class SearchModule : ModuleBase
    {
        [Command("Gif"), Summary("Gif Cute kittens"), Remarks("Searches gif for your Gifs??")]
        public async Task GifsAsync([Remainder] string keywords = null)
        {
            if (string.IsNullOrWhiteSpace(keywords))
                throw new NullReferenceException("Please enter what you are trying to search for!");

            var getUrl = new Uri("http://api.giphy.com/");
            using (var client = new HttpClient())
            {
                client.BaseAddress = getUrl;
                var response = await client.GetAsync(Uri.EscapeDataString($"v1/gifs/random?api_key=dc6zaTOxFJmzC&tag={Uri.UnescapeDataString(keywords)}"));
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(jsonResponse);

                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = $"{Context.User.Username} searched for {keywords}",
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    ImageUrl = obj["data"]["image_original_url"].ToString(),
                    Color = new Color(153, 30, 87)
                };
                await ReplyAsync("", false, embed);
            }
        }

        [Command("Urban"), Summary("Urban IE"), Remarks("Searches urban dictionary for your word")]
        public async Task UrbanAsync([Remainder] string urban = null)
        {
            if (string.IsNullOrWhiteSpace(urban))
                throw new NullReferenceException("A search term should be provided for me to search!");
            var embed = new EmbedBuilder();
            var vc = new HttpClient();
            embed.WithAuthor(x =>
            {
                x.Name = "Urban Dictionary";
                x.WithIconUrl("https://lh3.googleusercontent.com/4hpSJ4pAfwRUg-RElZ2QXNh_pV01Z96iJGT2BFuk_RRsNc-AVY7cZhbN2g1zWII9PBQ=w170");
            });
            string req = await vc.GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + urban);
            embed.WithColor(new Color(153, 30, 87));

            MatchCollection col = Regex.Matches(req, @"(?<=definition"":"")[ -z~-🧀]+(?="",""permalink)");
            MatchCollection col2 = Regex.Matches(req, @"(?<=example"":"")[ -z~-🧀]+(?="",""thumbs_down)");
            if (col.Count == 0)
            {
                await ReplyAsync("Couldn't find anything dammit");
                return;
            }
            Random r = new Random();
            string outpt = "Failed fetching embed from Urban Dictionary, please try later!";
            string outpt2 = "No Example";
            int max = r.Next(0, col.Count);
            for (int i = 0; i <= max; i++)
            {
                outpt = urban + "\r\n\r\n" + col[i].Value;
            }

            for (int i = 0; i <= max; i++)
            {
                outpt2 = "\r\n\r\n" + col2[i].Value;
            }

            outpt = outpt.Replace("\\r", "\r");
            outpt = outpt.Replace("\\n", "\n");
            outpt2 = outpt2.Replace("\\r", "\r");
            outpt2 = outpt2.Replace("\\n", "\n");

            embed.AddField(x =>
            {
                x.Name = $"Definition";
                x.Value = outpt;
            });

            embed.AddField(x =>
            {
                x.Name = "Example";
                x.Value = outpt2;
            });

            await ReplyAsync("", embed: embed);
        }

        [Command("Image"), Summary("Image rick and morty"), Remarks("Searches Bing for your image.")]
        public async Task ImageAsync([Remainder] string search = null)
        {
            if (string.IsNullOrWhiteSpace(search))
                throw new NullReferenceException("A search term should be provided for me to search!");
            using (var httpClient = new HttpClient())
            {
                var link = $"https://api.cognitive.microsoft.com/bing/v5.0/images/search?q={search}&count=10&offset=0&mkt=en-us&safeSearch=Off";
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", BotHandler.BotConfig.BingAPIKey);
                var res = await httpClient.GetAsync(link);
                if (!res.IsSuccessStatusCode)
                {
                    await ReplyAsync($"An error occurred: {res.ReasonPhrase}");
                    return;
                }
                JObject result = JObject.Parse(await res.Content.ReadAsStringAsync());
                JArray arr = (JArray)result["value"];
                if (arr.Count == 0)
                {
                    await ReplyAsync("No results found.");
                    return;
                }
                JObject image = (JObject)arr[0];
                var embed = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x.Name = $"Search Term:   {search.ToUpper()}";
                        x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                        x.Url = (string)image["contentUrl"];
                    })
                    .WithColor(new Color(66, 244, 191))
                    .WithImageUrl((string)image["contentUrl"]);
                await ReplyAsync("", embed: embed);
            }

        }

        [Command("Lmgtfy"), Summary("Lmgtfy How To Google"), Remarks("Googles something for that special person who is crippled")]
        public async Task LmgtfyAsync([Remainder] string search = "How to use Lmgtfy")
        {
            await ReplyAsync($"**Your special URL: **<http://lmgtfy.com/?q={ Uri.EscapeUriString(search) }>");
        }

        [Command("Imgur"), Summary("Imgur XD"), Remarks("Searches imgure for your image")]
        public async Task ImgurAsync([Remainder]string search)
        {
            var BaseUrl = $"http://imgur.com/search?q={search}";
            var config = Configuration.Default.WithDefaultLoader();
            var document = await BrowsingContext.New(config).OpenAsync(BaseUrl);
            var elems = document.QuerySelectorAll("a.image-list-link").ToList();
            if (!elems.Any())
                return;
            var img = (elems.ElementAtOrDefault(new Random().Next(0, elems.Count))?.Children?.FirstOrDefault() as IHtmlImageElement);
            if (img?.Source == null)
                return;
            var source = img.Source.Replace("b.", ".");

            var embed = new EmbedBuilder()
                .WithColor(new Color(66, 244, 191))
                .WithAuthor(x =>
                {
                    x.IconUrl = "https://s25.postimg.org/mi3j4sppb/imgur_1.png";
                    x.Name = $"Searched for: {search}";
                    x.Url = BaseUrl;
                })
                .WithDescription(source)
                .WithImageUrl(source);
            await ReplyAsync("", embed: embed);
        }

        [Command("Catfacts"), Summary("Catfacts"), Remarks("Catfacts for cat lovers")]
        public async Task CatfactsAsync()
        {
            using (var http = new HttpClient())
            {
                var response = await http.GetStringAsync("http://catfacts-api.appspot.com/api/facts");
                if (response == null)
                    return;
                var fact = JObject.Parse(response)["facts"][0].ToString();
                await ReplyAsync($":feet: {fact}");
            }
        }

        [Command("Robohash"), Summary("Bot ExceptionDev"), Remarks("Generates a bot image for your username/name")]
        public async Task RobohashAsync(string name)
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Clear();
                http.DefaultRequestHeaders.Add("X-Mashape-Key", BotHandler.BotConfig.MashapeKey);
                http.DefaultRequestHeaders.Add("Accept", "application/json");
                var res = JObject.Parse(await http.GetStringAsync($"https://robohash.p.mashape.com/index.php?text={Uri.EscapeUriString(name)}"));
                var link = res["imageUrl"].ToString();
                var embed = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x.Name = Context.Client.CurrentUser.Username;
                        x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                    })
                    .WithColor(new Color(102, 255, 255))
                    .WithImageUrl(link);
                await ReplyAsync("", embed: embed);
            }
        }

        [Command("Leet"), Summary("Leet text"), Remarks("Generates text in leet language")]
        public async Task LeetAsync([Remainder] string text)
        {
            try
            {
                using (var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Clear();
                    http.DefaultRequestHeaders.Add("X-Mashape-Key", BotHandler.BotConfig.MashapeKey);
                    http.DefaultRequestHeaders.Add("Accept", "text/plain");
                    var get = await http.GetStringAsync($"https://montanaflynn-l33t-sp34k.p.mashape.com/encode?text={Uri.EscapeUriString(text)}");
                    var embed = new EmbedBuilder()
                        .WithAuthor(x =>
                        {
                            x.Name = Context.Client.CurrentUser.Username;
                            x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                        })
                        .WithColor(new Color(102, 255, 255))
                        .WithDescription(get);
                    await ReplyAsync("", embed: embed);
                }
            }
            catch (Exception e)
            { await ReplyAsync(e.Message); }
        }

        [Command("Cookie"), Summary("Normal Command"), Remarks("Gets a random Fortune cookie for you")]
        public async Task FortuneCookieAsync()
        {
            try
            {
                using (var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Clear();
                    http.DefaultRequestHeaders.Add("X-Mashape-Key", BotHandler.BotConfig.MashapeKey);
                    http.DefaultRequestHeaders.Add("Accept", "text/plain");
                    var get = await http.GetStringAsync($"https://thibaultcha-fortunecow-v1.p.mashape.com/random");
                    var embed = new EmbedBuilder()
                        .WithAuthor(x =>
                        {
                            x.Name = Context.Client.CurrentUser.Username;
                            x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                        })
                        .WithColor(new Color(102, 255, 255))
                        .WithDescription($"```{get}```");
                    await ReplyAsync("", embed: embed);
                }
            }
            catch (Exception e)
            { await ReplyAsync(e.Message); }
        }

    }
}