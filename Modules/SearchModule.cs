using System;
using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Helpers;
using Newtonsoft.Json;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static Valerie.Addons.Embeds;

namespace Valerie.Modules
{
    public class SearchModule : Base
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
            await ReplyAsync(string.Empty,
                GetEmbed(Paint.Aqua)
                .WithFooter(x =>
                {
                    x.Text = !Data.Tags.Any() ? "No related terms." : string.Join(", ", Data.Tags);
                })
                .AddField($"Definition of {TermInfo.Word}", TermInfo.Definition)
                .AddField("Example", TermInfo.Example).Build());
        }

        [Command("Wiki"), Summary("Searches wikipedia for your terms.")]
        public async Task WikiAsync([Remainder]string Search)
        {
            var GetResult = await Context.HttpClient.GetAsync($"https://en.wikipedia.org/w/api.php?action=opensearch&search={Search}");
            dynamic Content = JsonConvert.DeserializeObject(await GetResult.Content.ReadAsStringAsync());
            await ReplyAsync(string.IsNullOrWhiteSpace($"{Content[2][0]}") ? $"No results found for {Search}."
                : $"**{Content[1][0]}**\n{Content[2][0]}");
        }

        [Command("DuckDuckGo"), Alias("DDG"), Summary("Uses DuckDuckGo search engine to get your results.")]
        public async Task DuckDuckGoAsync([Remainder] string Search)
        {
            var Response = await Context.HttpClient.GetAsync($"http://api.duckduckgo.com/?q={Search.Replace(' ', '+')}&format=json&pretty=1");
            if (!Response.IsSuccessStatusCode) return;
            var Convert = JsonConvert.DeserializeObject<DDGModel>(await Response.Content.ReadAsStringAsync());
            var Topics = Convert.RelatedTopics.Take(3);
            await ReplyAsync(string.Empty,
                GetEmbed(Paint.Aqua)
                .WithAuthor(x =>
                {
                    x.Name = Convert.Heading;
                    x.Url = Convert.AbstractURL;
                })
                .WithThumbnailUrl(Convert.Image ?? "https://png.icons8.com/color/256/000000/duckduckgo.png")
                .WithDescription(Convert.Abstract)
                .AddField("Related Topics", $"{string.Join("\n", Topics.Select(x => $"🔸 [{x.Text}]({x.FirstURL})"))}").Build());
        }

        [Command("Dotnet"), Summary("Searches Dotnet docs for namespaces.")]
        public async Task DotnetAsync([Remainder] string Search)
        {
            var Response = await Context.HttpClient.GetAsync($"https://docsapibrowser.azurewebsites.net/api/apibrowser/dotnet/search?search={Search}&$top=10");
            if (!Response.IsSuccessStatusCode) return;
            var ConvertedJson = JsonConvert.DeserializeObject<DotnetModel>(await Response.Content.ReadAsStringAsync());
            if (!ConvertedJson.Results.Any())
            {
                await ReplyAsync($"Couldn't find anything for {Search}.");
                return;
            }
            var Embed = GetEmbed(Paint.Aqua)
                .WithAuthor(x =>
               {
                   x.Name = $"Displaying Results For {Search}";
                   x.IconUrl = "https://avatars2.githubusercontent.com/u/9141961?s=200&v=4";
               });
            foreach (var Result in ConvertedJson.Results.Take(5).OrderBy(x => x.Name))
                Embed.AddField(Result.Name, $"Kind: {Result.Kind} | Type: {Result.Type}\n" +
                    $"[{Result.Snippet}]({Result.URL})");
            await ReplyAsync(string.Empty, Embed.Build());
        }

        [Command("Crypto"), Summary("Shows information about a crypto currency.")]
        public async Task CryptoAsync(string Currency = "Bitcoin")
        {
            var Get = await Context.HttpClient.GetAsync($"https://api.coinmarketcap.com/v1/ticker/{Currency}").ConfigureAwait(false);
            var Content = JsonConvert.DeserializeObject<CryptoModel[]>(await Get.Content.ReadAsStringAsync())[0];
            if (string.IsNullOrWhiteSpace(Content.Id)) { await ReplyAsync($"Couldn't find anything on {Currency}."); return; }
            await ReplyAsync(string.Empty, GetEmbed(Paint.Aqua)
                .WithAuthor(x =>
               {
                   x.Name = $"{Content.Rank} | {Content.Name}";
                   x.IconUrl = "https://png.icons8.com/color/512/000000/ethereum.png";
               })
               .WithFooter(x =>
               {
                   x.Text = $"Last Updated: {MethodHelper.UnixDateTime(Convert.ToDouble(Content.LastUpdated))}";
                   x.IconUrl = "https://png.icons8.com/color/512/000000/ethereum.png";
               })
               .AddField("Prices", $"**USD:** {Content.PriceUsd}\n**Bitcoin:** {Content.PriceBtc}", true)
               .AddField("Market", $"**Cap:** {Content.MarketCapUsd}\n**24 Hour Volume:** {Content.The24hVolumeUsd}", true)
               .AddField("Changes", $"**1 Hour:** {Content.PercentChange1h}\n**24 Hours:** {Content.PercentChange24h}\n**7 Days:** {Content.PercentChange7d}", true)
               .AddField("Supply", $"**Max:** {Content.MaxSupply}\n**Total:** {Content.TotalSupply}\n**Available:** {Content.AvailableSupply}", true)
               .Build());
        }

        [Command("POTD"), Summary("Retrives picture of the day from NASA.")]
        public async Task PotdAsync()
        {
            var Get = await Context.HttpClient.GetAsync($"https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("There was an error getting picture of the day from NASA.");
                return;
            }
            var Content = JsonConvert.DeserializeObject<NASAModel>(await Get.Content.ReadAsStringAsync());
            await ReplyAsync(string.Empty, GetEmbed(Paint.Aqua)
                .WithAuthor(x =>
                {
                    x.Name = $"{Content.Title} | {Content.Date}";
                    x.IconUrl = Content.Hdurl;
                    x.Url = Content.Url;
                })
                .WithDescription(Content.Explanation)
                .WithImageUrl(Content.Hdurl).Build());
        }

        [Command("POTD"), Summary("Retrives picture of the day from NASA with a specific date.")]
        public async Task PotdAsync(int Year, int Month, int Day)
        {
            var Get = await Context.HttpClient.GetAsync($"https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY&date={Year}-{Month}-{Day}").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("There was an error getting picture of the day from NASA.");
                return;
            }
            var Content = JsonConvert.DeserializeObject<NASAModel>(await Get.Content.ReadAsStringAsync());
            await ReplyAsync(string.Empty, GetEmbed(Paint.Aqua)
                .WithAuthor(x =>
                {
                    x.Name = $"{Content.Title} | {Content.Date}";
                    x.IconUrl = Content.Hdurl;
                    x.Url = Content.Url;
                })
                .WithDescription(Content.Explanation)
                .WithImageUrl(Content.Hdurl).Build());

        }

        [Command("SteamUser"), Summary("Shows info about a steam user.")]
        public async Task UserAsync(string UserId)
        {
            var UserInfo = await Context.ConfigHandler.Cookie.Steam.GetUsersInfoAsync(new[] { UserId }.ToList());
            var UserGames = await Context.ConfigHandler.Cookie.Steam.OwnedGamesAsync(UserId);
            var UserRecent = await Context.ConfigHandler.Cookie.Steam.RecentGamesAsync(UserId);
            var Info = UserInfo.PlayersInfo.Players.FirstOrDefault();
            string State;
            if (Info.ProfileState == 0) State = "Offline";
            else if (Info.ProfileState == 1) State = "Online";
            else if (Info.ProfileState == 2) State = "Busy";
            else if (Info.ProfileState == 3) State = "Away";
            else if (Info.ProfileState == 4) State = "Snooze";
            else if (Info.ProfileState == 5) State = "Looking to trade";
            else State = "Looking to play";

            var Embed = GetEmbed(Paint.Aqua)
                .WithAuthor(x =>
               {
                   x.Name = Info.RealName;
                   x.Url = Info.ProfileLink;
                   x.IconUrl = "https://png.icons8.com/material/256/e5e5e5/steam.png";
               })
              .WithThumbnailUrl(Info.AvatarFullUrl)
              .AddField("Display Name", $"{Info.Name}", true)
              .AddField("Location", $"{Info.State ?? "No State"}, {Info.Country ?? "No Country"}", true)
              .AddField("Person State", State, true)
              .AddField("Profile Created", MethodHelper.UnixDateTime(Info.TimeCreated), true)
              .AddField("Last Online", MethodHelper.UnixDateTime(Info.LastLogOff), true)
              .AddField("Primary Clan ID", Info.PrimaryClanId ?? "None.", true)
              .AddField("Owned Games", UserGames.OwnedGames.GamesCount, true)
              .AddField("Recently Played Games", UserRecent.RecentGames.TotalCount, true);
            await ReplyAsync(string.Empty, Embed.Build());
        }

        [Command("Giphy"), Alias("Gif"), Summary("Searches Giphy for your Gifs??")]
        public async Task Giphy([Remainder] string SearchTerms = null)
        {
            string Response = null;
            if (!string.IsNullOrWhiteSpace(SearchTerms))
            {
                var GetGif = await Context.ConfigHandler.Cookie.Giphy.SearchAsync(SearchTerms);
                Response = GetGif.Datum[Context.Random.Next(0, GetGif.Pagination.Count)].EmbedURL;
            }
            else
            {
                var Gif = await Context.ConfigHandler.Cookie.Giphy.TrendingAsync();
                var Random = Context.Random.Next(Gif.Pagination.Count);
                Response = Gif.Datum[Random].EmbedURL;
            }
            await ReplyAsync(string.Empty, GetEmbed(Paint.Aqua).WithImageUrl(Response).Build());
        }
    }
}