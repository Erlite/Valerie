using System;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Discord;
using Discord.Commands;
using AngleSharp;
using AngleSharp.Dom.Html;
using Cookie.Giphy;
using Cookie.Steam;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.Customsearch.v1;
using Valerie.Models;
using Valerie.Handlers;
using Valerie.Attributes;
using Valerie.Extensions;

namespace Valerie.Modules
{
    [RequireBotPermission(ChannelPermission.SendMessages), RequireAPIKeys]
    public class SearchModule : ValerieBase<ValerieContext>
    {
        readonly HttpClient HttpClient = new HttpClient();

        [Command("Urban"), Summary("Searches urban dictionary for your word")]
        public async Task UrbanAsync([Remainder] string SearchTerm)
        {
            var Client = await HttpClient.GetAsync($"http://api.urbandictionary.com/v0/define?term={SearchTerm.Replace(' ', '+')}");
            if (!Client.IsSuccessStatusCode)
            {
                await ReplyAsync("Couldn't communicate with Urban's API.");
                return;
            }
            var Data = JToken.Parse(await Client.Content.ReadAsStringAsync()).ToObject<Urban>();
            if (!Data.List.Any())
            {
                await ReplyAsync($"Couldn't find anything related to *{SearchTerm}*.");
                return;
            }
            var TermInfo = Data.List[Context.Random.Next(0, Data.List.Count)];
            var embed = ValerieEmbed.Embed(EmbedColor.Gold, FooterText: $"Related Terms: {string.Join(", ", Data.Tags)}" ?? "No related terms.");
            embed.AddField($"Definition of {TermInfo.Word}", TermInfo.Definition, false);
            embed.AddField("Example", TermInfo.Example, false);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Lmgtfy"), Summary("Googles something for that special person who is crippled")]
        public Task LmgtfyAsync([Remainder] string search = "How to use Lmgtfy")
            => ReplyAsync($"**Your special URL: **<http://lmgtfy.com/?q={ Uri.EscapeUriString(search) }>");

        [Command("Imgur"), Summary("Imgur XD"), Remarks("Searches imgure for your image")]
        public async Task ImgurAsync([Remainder] string search)
        {
            var BaseUrl = $"http://imgur.com/search?q={search}";
            var config = Configuration.Default.WithDefaultLoader();
            var document = await BrowsingContext.New(config).OpenAsync(BaseUrl);
            var elems = document.QuerySelectorAll("a.image-list-link").ToList();
            if (!elems.Any())
                return;
            var img = (elems.ElementAtOrDefault(Context.Random.Next(0, elems.Count))?.Children?.FirstOrDefault() as IHtmlImageElement);
            if (img?.Source == null)
                return;
            var source = img.Source.Replace("b.", ".");
            await ReplyAsync(source);
        }

        [Command("Robohash"), Summary("Robohash Yucked"), Remarks("Generates a bot image for your username/name")]
        public async Task RobohashAsync(string name)
        {
            string[] Sets = { "?set=set1", "?set=set2", "?set=set3" };
            var GetRandom = Sets[Context.Random.Next(0, Sets.Length)];
            string URL = $"https://robohash.org/{name}{GetRandom}";
            await ReplyAsync(URL);
        }

        [Command("Wiki"), Summary("Wiki KendValerie Lamar"), Remarks("Searches wikipedia for your terms")]
        public async Task WikiAsync([Remainder]string search)
        {
            var GetResult = await HttpClient.GetAsync($"https://en.wikipedia.org/w/api.php?action=opensearch&search={search}");
            var GetContent = await GetResult.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(GetContent);
            string title = responseObject[1][0];
            string firstParagraph = responseObject[2][0];
            if (string.IsNullOrWhiteSpace(firstParagraph))
                await ReplyAsync("No results found.");
            else
                await ReplyAsync($"{Format.Bold(title)}\n{firstParagraph}");
        }

        [Command("AdorableAvatar"), Summary("Generates an avatar from provided name/string"), Alias("AA")]
        public async Task AdorableAvatarAsync(IGuildUser User = null) => await ReplyAsync($"https://api.adorable.io/avatars/500/{User.Username ?? Context.User.Username}.png");

        [Command("DuckDuckGo"), Alias("DDG"), Summary("Uses Duck Duck Go search engine to get your results.")]
        public async Task DuckDuckGoAsync([Remainder] string Search)
        {
            var SB = new StringBuilder();
            string APIUrl = $"http://api.duckduckgo.com/?q={Search.Replace(' ', '+')}&format=json&pretty=1";
            var Response = await HttpClient.GetAsync(APIUrl);
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
            string Description = $"**{Convert.Heading}**\n" +
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
            var Response = await HttpClient.GetAsync($"https://docs.microsoft.com/api/apibrowser/dotnet/search?search={Search}");
            if (!Response.IsSuccessStatusCode)
            {
                await ReplyAsync(Response.ReasonPhrase);
                return;
            }
            var ConvertedJson = JsonConvert.DeserializeObject<DocsRoot>(await Response.Content.ReadAsStringAsync());
            foreach (var result in ConvertedJson.Results.Take(3).OrderBy(x => x.Name))
            {
                Builder.AppendLine(
                    $"**{result.Name}**\n" +
                    $"**Kind: **{result.Kind} || **Type: **{result.Type}\n" +
                    $"**Summary: **{result.Snippet}\n" +
                    $"**URL: ** {StringExtension.ShortenUrl(result.URL)}\n");
            }
            var embed = ValerieEmbed.Embed(EmbedColor.Snow, Description: Builder.ToString(), FooterText: $"Total Results: {ConvertedJson.Count.ToString()}");
            if (string.IsNullOrWhiteSpace(Builder.ToString()))
                await ReplyAsync("No results found.");
            else
                await ReplyAsync("", embed: embed.Build());
        }

        [Command("BImage"), Summary("Performs a bing image search for your query and replies back with a random image.")]
        public async Task ImageAsync([Remainder] string Query)
        {
            var Client = new HttpClient();
            var link = $"https://api.cognitive.microsoft.com/bing/v5.0/images/search?q={Query}&count=50&offset=0&mkt=en-us&safeSearch=Off";
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Context.ValerieConfig.APIKeys.BingKey);
            var res = await Client.GetAsync(link);
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
            var RandomNum = Context.Random.Next(1, 50);
            JObject image = (JObject)arr[RandomNum];
            var embed = ValerieEmbed.Embed(EmbedColor.Black, ImageUrl: (string)image["contentUrl"]);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Bing"), Summary("Performs a bing search for your query and replies back with 5 search results.")]
        public async Task SearchAsync([Remainder]string Query)
        {
            var Client = new HttpClient();
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Context.ValerieConfig.APIKeys.BingKey);
            var GetRequest = await Client.GetAsync($"https://api.cognitive.microsoft.com/bing/v5.0/search?q={Query}&count=5&offset=0&mkt=en-us&safeSearch=moderate");
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

        [Command("SteamUser"), Summary("Shows info about a steam user")]
        public async Task UserAsync(string UserId)
        {
            var SteamClient = new SteamClient(Context.ValerieConfig.APIKeys.SteamKey);
            var UserInfo = await SteamClient.GetUsersInfoAsync(new List<string> { UserId });
            var UserGames = await SteamClient.OwnedGamesAsync(UserId);
            var UserRecent = await SteamClient.RecentGamesAsync(UserId);

            var Info = UserInfo.PlayersInfo.Players.FirstOrDefault();

            string State;
            if (Info.ProfileState == 0)
                State = "Offline";
            else if (Info.ProfileState == 1)
                State = "Online";
            else if (Info.ProfileState == 2)
                State = "Busy";
            else if (Info.ProfileState == 3)
                State = "Away";
            else if (Info.ProfileState == 4)
                State = "Snooze";
            else if (Info.ProfileState == 5)
                State = "Looking to trade";
            else
                State = "Looking to play";

            var embed = ValerieEmbed.Embed(EmbedColor.Pastel, Info.AvatarFullUrl, Info.RealName, Info.ProfileLink,
                FooterText: string.Join(", ", UserRecent.RecentGames.GamesList.Select(x => x.Name)));
            embed.AddField("Display Name", $"{Info.Name}", true);
            embed.AddField("Location", $"{Info.State ?? "No State"}, {Info.Country ?? "No Country"}", true);
            embed.AddField("Person State", State, true);
            embed.AddField("Profile Created", DateTimeExtension.UnixTimeStampToDateTime(Info.TimeCreated), true);
            embed.AddField("Last Online", DateTimeExtension.UnixTimeStampToDateTime(Info.LastLogOff), true);
            embed.AddField("Primary Clan ID", Info.PrimaryClanId, true);
            embed.AddField("Owned Games", UserGames.OwnedGames.GamesCount, true);
            embed.AddField("Recently Played Games", UserRecent.RecentGames.TotalCount, true);

            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Giphy"), Summary("Searches Giphy for your Gifs??"), Alias("Gif")]
        public async Task Giphy([Remainder] string SearchTerms = null)
        {
            GiphyClient Client = new GiphyClient(Context.ValerieConfig.APIKeys.GiphyKey);
            string Response = null;
            if (!string.IsNullOrWhiteSpace(SearchTerms))
            {
                var GetGif = await Client.SearchAsync(SearchTerms);
                Response = GetGif.Datum[Context.Random.Next(0, GetGif.Pagination.Count)].EmbedURL;
            }
            else
            {
                var gif = await Client.TrendingAsync();
                var Random = Context.Random.Next(0, gif.Pagination.Count);
                Response = gif.Datum[Random].EmbedURL;
            }
            await ReplyAsync(Response);
        }

        [Command("Neko"), Summary("Eh, Get yourself some Neko?")]
        public async Task LewdAsync()
        {
            JToken Token = JToken.Parse(await HttpClient.GetStringAsync("http://nekos.life/api/neko").ConfigureAwait(false));
            await ReplyAsync(Token["neko"].ToString());
        }

        [Command("News"), Summary("Gets you the latest news.")]
        public async Task NewsAsync()
        {
            var Get = await HttpClient.GetAsync($"https://newsapi.org/v1/articles?source=bbc-news&sortBy=top&apiKey={Context.ValerieConfig.APIKeys.NewsKey}").ConfigureAwait(false);
            var Embed = ValerieEmbed.Embed(EmbedColor.Pastel, Title: "Today's Headlines From BB");
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("Failed to get today's headlines.");
                return;
            }
            var Content = JsonConvert.DeserializeObject<BBC>(await Get.Content.ReadAsStringAsync());
            foreach (var Article in Content.Articles.Take(10))
                Embed.AddField($"{Article.Title}", $"{Article.Description}\n**Link:**{Article.Url}");
            await ReplyAsync("", embed: Embed.Build());
        }

        [Command("Google"), Alias("G"), Summary("Searches google for your search terms.")]
        public async Task GoogleAsync([Remainder] string search)
        {
            var Str = new StringBuilder();
            string URL = "http://diylogodesigns.com/blog/wp-content/uploads/2016/04/google-logo-icon-PNG-Transparent-Background.png";

            var Service = new CustomsearchService(new BaseClientService.Initializer
            {
                ApiKey = Context.ValerieConfig.APIKeys.GoogleKey
            });
            var RequestList = Service.Cse.List(search);
            RequestList.Cx = Context.ValerieConfig.APIKeys.SearchEngineID;

            var items = RequestList.Execute().Items.Take(5);
            foreach (var result in items)
            {
                Str.AppendLine($"• **{result.Title}**\n{result.Snippet}\n{StringExtension.ShortenUrl(result.Link)}\n");
            }

            var embed = ValerieEmbed.Embed(EmbedColor.Pastel, Description: Str.ToString(), ThumbUrl: URL);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("GImage"), Summary("Searches google for your image and returns a random image from 50 results.")]
        public async Task GImageAsync([Remainder] string search)
        {
            var rng = Context.Random;
            var reqString = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString(search)}&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start=" +
                $"{ rng.Next(1, 10) }&fields=items%2Flink&key={Context.ValerieConfig.APIKeys.GoogleKey}";
            var obj = JObject.Parse(await new HttpClient().GetStringAsync(reqString));
            var items = obj["items"] as JArray;
            string image = items[0]["link"].ToString();

            if (!string.IsNullOrWhiteSpace(image))
                await ReplyAsync(image);
            else
                await ReplyAsync("No results found!");
        }

        [Command("Youtube"), Alias("Yt"), Summary("Searches the first search result from youtube.")]
        public Task YoutubeAsync([Remainder] string Search)
        {
            var Service = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = Context.ValerieConfig.APIKeys.GoogleKey
            });
            var SearchRequest = Service.Search.List("snippet");
            SearchRequest.Q = Search;
            SearchRequest.MaxResults = 1;
            SearchRequest.Type = "video";
            var Result = SearchRequest.Execute().Items.Select(x => x.Id.VideoId).FirstOrDefault();
            return ReplyAsync("https://www.youtube.com/watch?v=" + Result);
        }

        [Command("Shorten"), Summary("Shortens a URL using Google URL Shortner.")]
        public Task ShortenAsync([Remainder] string URL) => ReplyAsync($"This is your shortened URL: {StringExtension.ShortenUrl(URL)}");

        [Command("Revav"), Summary("Performs a reverse image search for a user avatar.")]
        public Task RevavAsync(IGuildUser User)
            => ReplyAsync($"Reverse Image Result: " +
                $"{StringExtension.ShortenUrl($"https://images.google.com/searchbyimage?image_url={User.GetAvatarUrl()}")}");
    }
}