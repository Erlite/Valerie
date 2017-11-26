using Discord;
using System;
using AngleSharp;
using System.Linq;
using AngleSharp.Dom;
using Valerie.Handlers;
using Newtonsoft.Json;
using Discord.Commands;
using Valerie.JsonModels;
using Google.Apis.Services;
using AngleSharp.Dom.Html;
using Google.Apis.YouTube.v3;
using System.Threading.Tasks;
using System.Collections.Generic;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules
{
    [Name("Web Search Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class SearchModule : ValerieBase
    {
        [Command("Urban"), Summary("Searches urban dictionary for your word.")]
        public async Task UrbanAsync([Remainder] string SearchTerm)
        {
            var Client = await Context.HttpClient.GetAsync($"http://api.urbandictionary.com/v0/define?term={SearchTerm.Replace(' ', '+')}");
            if (!Client.IsSuccessStatusCode) return;
            var Data = JsonConvert.DeserializeObject<UrbanModel>(await Client.Content.ReadAsStringAsync());
            if (!Data.List.Any())
            {
                await ReplyAsync($"Couldn't find anything related to *{SearchTerm}*.");
                return;
            }
            var TermInfo = Data.List[Context.Random.Next(0, Data.List.Count)];
            var embed = ValerieEmbed.Embed(EmbedColor.Cyan, FooterText: $"Related Terms: {string.Join(", ", Data.Tags)}" ?? "No related terms.");
            embed.AddField($"Definition of {TermInfo.Word}", TermInfo.Definition, false);
            embed.AddField("Example", TermInfo.Example, false);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Wiki"), Summary("Searches wikipedia for your terms.")]
        public async Task WikiAsync([Remainder]string Search)
        {
            var GetResult = await Context.HttpClient.GetAsync($"https://en.wikipedia.org/w/api.php?action=opensearch&search={Search}");
            dynamic Content = JsonConvert.DeserializeObject(await GetResult.Content.ReadAsStringAsync());
            string Result = string.IsNullOrWhiteSpace($"{Content[2][0]}") ? $"No results found for {Search}."
                : $"**{Content[1][0]}**\n{Content[2][0]}";
            await ReplyAsync(Result);
        }

        [Command("DuckDuckGo"), Alias("DDG"), Summary("Uses DuckDuckGo search engine to get your results.")]
        public async Task DuckDuckGoAsync([Remainder] string Search)
        {
            var Response = await Context.HttpClient.GetAsync($"http://api.duckduckgo.com/?q={Search.Replace(' ', '+')}&format=json&pretty=1");
            if (!Response.IsSuccessStatusCode) return;
            var Convert = JsonConvert.DeserializeObject<DDGModel>(await Response.Content.ReadAsStringAsync());
            var Topics = Convert.RelatedTopics.Take(3);
            var Embed = ValerieEmbed.Embed(EmbedColor.Red, AuthorName: Convert.Heading, AuthorUrl: Convert.AbstractURL,
                ImageUrl: Convert.Image ?? "https://i.imgur.com/OHF71A3.png", Description: $"{Convert.Abstract}");
            Embed.AddField("Related Topics", $"{string.Join("\n", Topics.Select(x => $"🔸 [{x.Text}]({x.FirstURL})"))}");
            await ReplyAsync(string.Empty, embed: Embed.Build());
        }

        [Command("Docs"), Summary("Searches Microsoft docs for programming related terms.")]
        public async Task DocsAsync([Remainder] string Search)
        {
            var Response = await Context.HttpClient.GetAsync($"https://docsapibrowser.azurewebsites.net/api/apibrowser/dotnet/search?search={Search}&$skip=25&$top=25");
            if (!Response.IsSuccessStatusCode) return;
            var ConvertedJson = JsonConvert.DeserializeObject<DocsModel>(await Response.Content.ReadAsStringAsync());
            if (!ConvertedJson.Results.Any())
            {
                await ReplyAsync($"Couldn't find anything for {Search}.");
                return;
            }
            string Results = null;
            foreach (var Result in ConvertedJson.Results.Take(5).OrderBy(x => x.Name))
                Results += $"[{Result.Name}]({Result.URL})\n" +
                    $"Kind: {Result.Kind} | Type: {Result.Type}\n{Result.Snippet}\n\n";
            await ReplyAsync("", embed: ValerieEmbed.Embed(EmbedColor.Random, Description: Results).Build());
        }

        [Command("Crypto"), Summary("Shows information about a crypto currency.")]
        public async Task CryptoAsync(string Currency = "Bitcoin")
        {
            var Get = await Context.HttpClient.GetAsync($"https://api.coinmarketcap.com/v1/ticker/{Currency}").ConfigureAwait(false);
            var Content = JsonConvert.DeserializeObject<CryptoModel[]>(await Get.Content.ReadAsStringAsync())[0];
            if (string.IsNullOrWhiteSpace(Content.Id)) { return; }
            await ReplyAsync(
                $"**Name:** {Content.Name}\n**Rank:** {Content.Rank}\n**Last Updated:** {UnixDT(Convert.ToDouble(Content.LastUpdated))}\n" +
                $"**USD Price:** {Content.PriceUsd}\n**Bitcoin Price:** {Content.PriceBtc}\n" +
                $"**USD Market Cap:** {Content.MarketCapUsd}\n**USD 24H Volume:** {Content.The24hVolumeUsd}\n" +
                $"**1H Percent Change:** {Content.PercentChange1h}\n**24H Percent Change:** {Content.PercentChange24h}\n**7D Percent Change:** {Content.PercentChange7d}\n" +
                $"**Available Supply:** {Content.AvailableSupply}\n**Total Supply:** {Content.TotalSupply}\n**Max Supply:** {Content.MaxSupply}");
        }

        [Command("Google"), Alias("G"), Summary("Searches google for your search terms.")]
        public async Task GoogleAsync([Remainder] string Search)
        {
            var Document = await DocumentAsync($"https://www.google.com/search?q={Search}");
            var Elements = Document.QuerySelectorAll("div.g");
            if (!Elements.Any()) return;
            var Query = Elements.Take(10).Select(x =>
            {
                var Tag = x.Children.FirstOrDefault()?.Children.FirstOrDefault() as IHtmlAnchorElement;
                if (Tag?.Href == null || Tag?.TextContent == null) return (null, null, null);
                var Desc = x.QuerySelectorAll(".st").FirstOrDefault()?.TextContent;
                return SearchResult(Tag.TextContent, Tag.Href, Desc);
            });

            if (!Query.Any()) await ReplyAsync($"No results found for **{Search}**.");
            string Description = null;
            foreach (var Result in Query.Where(x => x.Item1 != null).Take(3))
                Description += $"-> [{Result.Item1}]({Result.Item2})\n{Result.Item3}\n\n";
            var Embed = ValerieEmbed.Embed(EmbedColor.Random, Title: $"Search Results For: {Search}", Description: Description);
            await ReplyAsync(string.Empty, embed: Embed.Build());
        }

        [Command("Youtube"), Alias("Yt"), Summary("Searches a youtube for your video.")]
        public Task YoutubeAsync([Remainder] string Search)
        {
            var Service = new YouTubeService(new BaseClientService.Initializer { ApiKey = Context.Config.ApplicationKeys.GoogleKey });
            var SearchRequest = Service.Search.List("snippet");
            SearchRequest.Q = Search;
            SearchRequest.MaxResults = 1;
            SearchRequest.Type = "video";
            return ReplyAsync("https://www.youtube.com/watch?v=" + SearchRequest.Execute().Items.Select(x => x.Id.VideoId).FirstOrDefault());
        }

        [Command("Giphy"), Alias("Gif"), Summary("Searches Giphy for your Gifs??")]
        public async Task Giphy([Remainder] string SearchTerms = null)
        {
            string Response = null;
            if (!string.IsNullOrWhiteSpace(SearchTerms))
            {
                var GetGif = await MainHandler.Cookie.Giphy.SearchAsync(SearchTerms);
                Response = GetGif.Datum[Context.Random.Next(0, GetGif.Pagination.Count)].EmbedURL;
            }
            else
            {
                var gif = await MainHandler.Cookie.Giphy.TrendingAsync();
                var Random = Context.Random.Next(gif.Pagination.Count);
                Response = gif.Datum[Random].EmbedURL;
            }
            await ReplyAsync(Response);
        }

        [Command("SteamUser"), Summary("Shows info about a steam user.")]
        public async Task UserAsync(string UserId)
        {
            var UserInfo = await MainHandler.Cookie.Steam.GetUsersInfoAsync(new List<string> { UserId });
            var UserGames = await MainHandler.Cookie.Steam.OwnedGamesAsync(UserId);
            var UserRecent = await MainHandler.Cookie.Steam.RecentGamesAsync(UserId);
            var Info = UserInfo.PlayersInfo.Players.FirstOrDefault();
            string State;
            if (Info.ProfileState == 0) State = "Offline";
            else if (Info.ProfileState == 1) State = "Online";
            else if (Info.ProfileState == 2) State = "Busy";
            else if (Info.ProfileState == 3) State = "Away";
            else if (Info.ProfileState == 4) State = "Snooze";
            else if (Info.ProfileState == 5) State = "Looking to trade";
            else State = "Looking to play";

            var embed = ValerieEmbed.Embed(EmbedColor.Random, Info.AvatarFullUrl, Info.RealName, Info.ProfileLink,
                FooterText: string.Join(", ", UserRecent.RecentGames.GamesList.Select(x => x.Name)));
            embed.AddField("Display Name", $"{Info.Name}", true);
            embed.AddField("Location", $"{Info.State ?? "No State"}, {Info.Country ?? "No Country"}", true);
            embed.AddField("Person State", State, true);
            embed.AddField("Profile Created", UnixDT(Info.TimeCreated), true);
            embed.AddField("Last Online", UnixDT(Info.LastLogOff), true);
            embed.AddField("Primary Clan ID", Info.PrimaryClanId, true);
            embed.AddField("Owned Games", UserGames.OwnedGames.GamesCount, true);
            embed.AddField("Recently Played Games", UserRecent.RecentGames.TotalCount, true);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Potd"), Summary("Retrives picture of the day from NASA.")]
        public async Task PotdAsync()
        {
            var Get = await Context.HttpClient.GetAsync($"https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("There was an error getting picture of the day from NASA.");
                return;
            }
            var Content = JsonConvert.DeserializeObject<POTDModel>(await Get.Content.ReadAsStringAsync());
            await ReplyAsync("", embed: ValerieEmbed.Embed(EmbedColor.Yellow, AuthorName: $"{Content.Title} | {Content.Date}", AuthorUrl: Content.Url,
                Description: $"**Information: **{Content.Explanation}", ImageUrl: Content.Hdurl).Build());
        }

        [Command("Potd"), Summary("Retrives picture of the day from NASA with a specific date.")]
        public async Task PotdAsync(int Year, int Month, int Day)
        {
            var Get = await Context.HttpClient.GetAsync($"https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY&date={Year}-{Month}-{Day}").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("There was an error getting picture of the day from NASA.");
                return;
            }
            var Content = JsonConvert.DeserializeObject<POTDModel>(await Get.Content.ReadAsStringAsync());
            await ReplyAsync("", embed: ValerieEmbed.Embed(EmbedColor.Yellow, AuthorName: $"{Content.Title} | {Content.Date}", AuthorUrl: Content.Url,
                Description: $"**Information: **{Content.Explanation}", ImageUrl: Content.Hdurl).Build());

        }

        DateTime UnixDT(double Unix)
            => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Unix).ToLocalTime();

        Task<IDocument> DocumentAsync(string Url) => BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(Url);

        (string, string, string) SearchResult(string Name, string Url, string Desc) => (Name, Url, Desc);
    }
}