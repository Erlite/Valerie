using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rick.Attributes;
using Rick.Classes;
using Discord;
using Discord.Commands;
using Rick.Services;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Rick.Handlers;

namespace Rick.Modules
{
    [CheckBlacklist]
    public class SteamModule : ModuleBase
    {
        [Command("SNews"), Summary("SNews 440"), Remarks("Shows news results for the game")]
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
                Color = new Color(124,12,57)
            };

            foreach(var Result in Convert.appnews.newsitems)
            {
                Builder.AddField(x =>
                {
                    x.Name = $"{Result.title} || {Result.feedlabel}";
                    x.Value = $"{Result.contents}\n{Result.url}";
                });
            }
            await ReplyAsync("", embed: Builder);
        }

        [Command("SUser"), Summary("Steam User 001100110011"), Remarks("Shows info about a steam user")]
        public async Task UserAsync(ulong ID)
        {
            var Httpclient = new HttpClient();
            string IPlayerService = "http://api.steampowered.com/IPlayerService/";
            var SummarURL = await Httpclient.GetAsync($"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={BotHandler.BotConfig.SteamAPIKey}&steamids={ID}");
            var GamesOwned = await Httpclient.GetAsync(IPlayerService + $"GetOwnedGames/v0001/?key={BotHandler.BotConfig.SteamAPIKey}&steamid={ID}&format=json");
            var RecentlyPlayed = await Httpclient.GetAsync(IPlayerService + $"GetRecentlyPlayedGames/v0001/?key={BotHandler.BotConfig.SteamAPIKey}&steamid={ID}&format=json");

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
            Builder.AddInlineField("Location", $"{Info.locstatecode}, {Info.loccountrycode}");
            Builder.AddInlineField("Person State", State);
            Builder.AddInlineField("Profile Created", StaticMethodService.UnixTimeStampToDateTime(Info.timecreated));
            Builder.AddInlineField("Last Online", StaticMethodService.UnixTimeStampToDateTime(Info.lastlogoff));
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
