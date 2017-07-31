﻿using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json;
using Discord;
using Discord.Commands;
using Valerie.Extensions;
using Valerie.Models;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom.Html;
using Valerie.Handlers.ConfigHandler;
using Valerie.Attributes;

namespace Valerie.Modules
{
    [RequireAPIKeys, RequireBotPermission(GuildPermission.SendMessages)]
    public class SearchModule : ModuleBase
    {
        [Command("Urban"), Summary("Searches urban dictionary for your word")]
        public async Task UrbanAsync([Remainder] string SearchTerm)
        {
            var embed = new EmbedBuilder();
            var vc = new HttpClient();
            embed.WithAuthor(x =>
            {
                x.Name = "Urban Dictionary";
                x.WithIconUrl("https://lh3.googleusercontent.com/4hpSJ4pAfwRUg-RElZ2QXNh_pV01Z96iJGT2BFuk_RRsNc-AVY7cZhbN2g1zWII9PBQ=w170");
            });
            string req = await vc.GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + SearchTerm);
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
                outpt = SearchTerm + "\r\n\r\n" + col[i].Value;
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
            vc.Dispose();
        }

        [Command("Lmgtfy"), Summary("Googles something for that special person who is crippled")]
        public async Task LmgtfyAsync([Remainder] string search = "How to use Lmgtfy")
        {
            await ReplyAsync($"**Your special URL: **<http://lmgtfy.com/?q={ Uri.EscapeUriString(search) }>");
        }

        [Command("Imgur"), Summary("Imgur XD"), Remarks("Searches imgure for your image")]
        public async Task ImgurAsync([Remainder] string search)
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
            await ReplyAsync(source);
        }

        [Command("Robohash"), Summary("Robohash Yucked"), Remarks("Generates a bot image for your username/name")]
        public async Task RobohashAsync(string name)
        {
            string[] Sets = { "?set=set1", "?set=set2", "?set=set3" };
            var GetRandom = Sets[new Random().Next(0, Sets.Length)];
            string URL = $"https://robohash.org/{name}{GetRandom}";
            await ReplyAsync(URL);
        }

        [Command("Wiki"), Summary("Wiki KendValerie Lamar"), Remarks("Searches wikipedia for your terms")]
        public async Task WikiAsync([Remainder]string search)
        {
            HttpClient HttpClient = new HttpClient();
            var GetResult = HttpClient.GetAsync($"https://en.wikipedia.org/w/api.php?action=opensearch&search={search}").Result;
            var GetContent = GetResult.Content.ReadAsStringAsync().Result;
            dynamic responseObject = JsonConvert.DeserializeObject(GetContent);
            string title = responseObject[1][0];
            string firstParagraph = responseObject[2][0];
            if (string.IsNullOrWhiteSpace(firstParagraph))
                await ReplyAsync("No results found.");
            else
                await ReplyAsync($"{Format.Bold(title)}\n{firstParagraph}");
        }

        [Command("AdorableAvatar"), Summary("Generates an avatar from provided name/string"), Alias("AA")]
        public async Task AdorableAvatarAsync(string Name) => await ReplyAsync($"https://api.adorable.io/avatars/500/{Name}.png");

        [Command("DuckDuckGo"), Alias("DDG"), Summary("Uses Duck Duck Go search engine to get your results.")]
        public async Task DuckDuckGoAsync([Remainder] string Search)
        {
            var SB = new StringBuilder();
            string APIUrl = $"http://api.duckduckgo.com/?q={Search.Replace(' ', '+')}&format=json&pretty=1";
            var Response = await new HttpClient().GetAsync(APIUrl);
            if (!Response.IsSuccessStatusCode)
            {
                await ReplyAsync("An error occured while trying to fetch results from API."); return;
            }
            var Convert = JsonConvert.DeserializeObject<DuckDuckGo>(await Response.Content.ReadAsStringAsync());
            string Image = null;
            if (Convert.Image == null || string.IsNullOrWhiteSpace(Convert.Image))
                Image = "https://preview.ibb.co/e72xna/DDG.jpg";
            else
                Image = Convert.Image;

            foreach (var Res in Convert.RelatedTopics.Take(3))
            {
                SB.AppendLine($":point_right:  {Res.Text}: <{StringExtension.ShortenUrl(Res.FirstURL)}>");
            }
            string Description = $"**{Convert.Heading}**\n\n" +
                $"{Convert.Abstract}\n" +
                $"<{StringExtension.ShortenUrl(Convert.AbstractURL)}>\n\n" +
                $"**Related Topics:**\n" +
                $"{SB.ToString()}";
            await ReplyAsync(Description);
        }

        [Command("Docs"), Summary("Searches Microsoft docs for terms")]
        public async Task DocsAsync([Remainder] string Search)
        {
            var Builder = new StringBuilder();
            var Response = await new HttpClient().GetAsync($"https://docs.microsoft.com/api/apibrowser/dotnet/search?search={Search}");
            if (!Response.IsSuccessStatusCode)
            {
                await ReplyAsync(Response.ReasonPhrase);
                return;
            }
            var ConvertedJson = JsonConvert.DeserializeObject<DocsRoot>(await Response.Content.ReadAsStringAsync());
            foreach (var result in ConvertedJson.Results.Take(3).OrderBy(x => x.Name))
            {
                Builder.AppendLine($"**{result.Name}**\n" +
                    $"**Kind: **{result.Kind} || **Type: **{result.Type}\n" +
                    $"**Summary: **{result.Snippet}\n" +
                    $"**URL: ** {StringExtension.ShortenUrl(result.URL)}\n");
            }
            var embed = Vmbed.Embed(VmbedColors.Snow, Description: Builder.ToString(), FooterText: $"Total Results: {ConvertedJson.Count.ToString()}");
            await ReplyAsync("", embed: embed);
        }

        [Command("BImage"), Summary("Performs a bing image search for your query and replies back with a random image.")]
        public async Task ImageAsync([Remainder] string Query)
        {
            using (var httpClient = new HttpClient())
            {
                var link = $"https://api.cognitive.microsoft.com/bing/v5.0/images/search?q={Query}&count=50&offset=0&mkt=en-us&safeSearch=Off";
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", BotDB.Config.APIKeys.BingKey);
                var res = await httpClient.GetAsync(link);
                if (!res.IsSuccessStatusCode)
                {
                    await ReplyAsync($"An error occurred while trying to fetch results.");
                    return;
                }
                JObject result = JObject.Parse(await res.Content.ReadAsStringAsync());
                JArray arr = (JArray)result["value"];
                if (arr.Count == 0)
                {
                    await ReplyAsync("No results found.");
                    return;
                }
                var Random = new Random();
                var RandomNum = Random.Next(1, 50);
                JObject image = (JObject)arr[RandomNum];
                var embed = Vmbed.Embed(VmbedColors.Black, ImageUrl: (string)image["contentUrl"]);
                await ReplyAsync("", embed: embed);
            }
        }

        [Command("Bing"), Summary("Performs a bing search for your query and replies back with 5 search results.")]
        public async Task SearchAsync([Remainder]string Query)
        {
            using (var Http = new HttpClient())
            {
                Http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", BotDB.Config.APIKeys.BingKey);
                var GetRequest = await Http.GetAsync($"https://api.cognitive.microsoft.com/bing/v5.0/search?q={Query}&count=5&offset=0&mkt=en-us&safeSearch=moderate");
                if (!GetRequest.IsSuccessStatusCode)
                {
                    await ReplyAsync(GetRequest.ReasonPhrase);
                    return;
                }
                var getString = await GetRequest.Content.ReadAsStringAsync();
                var Convert = JToken.Parse(getString).ToObject<SearchRoot>();
                var str = new StringBuilder();
                foreach (var result in Convert.Pages.Value.Take(3))
                {
                    str.AppendLine($"**{result.Name}**\n{result.Snippet}\n<{StringExtension.ShortenUrl(result.URL)}>\n");
                }
                await ReplyAsync(str.ToString());
            }
        }

        [Command("SNews"), Summary("Shows news results for the game")]
        public async Task NewsAsync(int ID)
        {
            var Httpclient = new HttpClient();
            var RequestUrl = await Httpclient.GetAsync($"http://api.steampowered.com/ISteamNews/GetNewsForApp/v0002/?appid={ID}&count=5&maxlength=300&format=json");
            if (!RequestUrl.IsSuccessStatusCode)
            {
                await ReplyAsync(RequestUrl.ReasonPhrase);
                return;
            }
            var Content = await RequestUrl.Content.ReadAsStringAsync();
            var Convert = JToken.Parse(Content).ToObject<SteamAppNews>();

            var Builder = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"APP ID: {ID}",
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Total Results: {Convert.appnews.count.ToString()}"
                },
                Color = new Color(124, 12, 57)
            };

            foreach (var Result in Convert.appnews.newsitems)
            {
                Builder.AddField(x =>
                {
                    x.Name = $"{Result.title} || {Result.feedlabel}";
                    x.Value = $"{Result.contents}\n{Result.url}";
                });
            }
            await ReplyAsync("", embed: Builder);
        }

        [Command("SUser"), Summary("Shows info about a steam user")]
        public async Task UserAsync(ulong ID)
        {
            var Httpclient = new HttpClient();
            string IPlayerService = "http://api.steampowered.com/IPlayerService/";
            var SummarURL = await Httpclient.GetAsync($"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={BotDB.Config.APIKeys.SteamKey}&steamids={ID}");
            var GamesOwned = await Httpclient.GetAsync(IPlayerService + $"GetOwnedGames/v0001/?key={BotDB.Config.APIKeys.SteamKey}&steamid={ID}&format=json");
            var RecentlyPlayed = await Httpclient.GetAsync(IPlayerService + $"GetRecentlyPlayedGames/v0001/?key={BotDB.Config.APIKeys.SteamKey}&steamid={ID}&format=json");

            if (!SummarURL.IsSuccessStatusCode || !GamesOwned.IsSuccessStatusCode || !RecentlyPlayed.IsSuccessStatusCode)
            {
                await ReplyAsync(SummarURL.ReasonPhrase);
                return;
            }

            var SummaryContent = await SummarURL.Content.ReadAsStringAsync();
            var SummaryConvert = JToken.Parse(SummaryContent).ToObject<PlayerSummary>();

            var OGames = await GamesOwned.Content.ReadAsStringAsync();
            var OGamesConvert = JToken.Parse(OGames).ToObject<OwnedGames>();

            var RGames = await RecentlyPlayed.Content.ReadAsStringAsync();
            var RGamesConvert = JToken.Parse(RGames).ToObject<GetRecent>();

            var Info = SummaryConvert.response.players.FirstOrDefault();

            string State;
            if (Info.personastate == 0)
                State = "Offline";
            else if (Info.personastate == 1)
                State = "Online";
            else if (Info.personastate == 2)
                State = "Busy";
            else if (Info.personastate == 3)
                State = "Away";
            else if (Info.personastate == 4)
                State = "Snooze";
            else if (Info.personastate == 5)
                State = "Looking to trade";
            else
                State = "Looking to play";

            var Sb = new StringBuilder();

            var Builder = new EmbedBuilder()
            {
                Color = new Color(124, 12, 57),
                Author = new EmbedAuthorBuilder()
                {
                    Name = Info.realname,
                    IconUrl = Info.avatarfull,
                    Url = Info.profileurl
                },
                ThumbnailUrl = Info.avatarfull
            };
            Builder.AddInlineField("Display Name", $"{Info.personaname}");
            Builder.AddInlineField("Location", $"{Info.locstatecode ?? "No State"}, {Info.loccountrycode ?? "No Country"}");
            Builder.AddInlineField("Person State", State);
            Builder.AddInlineField("Profile Created", DateTimeExtension.UnixTimeStampToDateTime(Info.timecreated));
            Builder.AddInlineField("Last Online", DateTimeExtension.UnixTimeStampToDateTime(Info.lastlogoff));
            Builder.AddInlineField("Primary Clan ID", Info.primaryclanid);
            Builder.AddInlineField("Owned Games", OGamesConvert.response.game_count);
            Builder.AddInlineField("Recently Played Games", RGamesConvert.response.total_count);
            Sb.AppendLine(string.Join(", ", RGamesConvert.response.games.Select(game => game.name)));
            Builder.Footer = new EmbedFooterBuilder()
            {
                Text = Sb.ToString()
            };

            await ReplyAsync("", embed: Builder);
        }
    }
}