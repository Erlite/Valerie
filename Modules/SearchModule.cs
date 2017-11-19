using Discord;
using System;
using System.Linq;
using Newtonsoft.Json;
using Discord.Commands;
using Valerie.Extensions;
using Valerie.JsonModels;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Valerie.Modules.Addons;
using System.Threading.Tasks;
using System.Collections.Generic;
using Valerie.Handlers.ModuleHandler;
using AngleSharp;
using AngleSharp.Dom.Html;

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
            string Result = string.IsNullOrWhiteSpace(Content[2][0]) ? $"No results found for {Search}."
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
            var Response = await Context.HttpClient.GetAsync($"https://docs.microsoft.com/api/apibrowser/dotnet/search?search={Search}");
            if (!Response.IsSuccessStatusCode) return;
            var ConvertedJson = JsonConvert.DeserializeObject<DocsModel>(await Response.Content.ReadAsStringAsync());
            if (!ConvertedJson.Results.Any())
            {
                await ReplyAsync($"Couldn't find anything for {Search}.");
                return;
            }
            string Results = null;
            foreach (var Result in ConvertedJson.Results.Take(4).OrderBy(x => x.Name))
                Results += $"[{Result.Name}]({Result.URL})\n" +
                    $"Kind: {Result.Kind} | Type: {Result.Type}\n{Result.Snippet}\n";
            await ReplyAsync(Results);
        }

        [Command("Imgur"), Summary("Searches imgure for your image")]
        public async Task ImgurAsync([Remainder] string Search)
        {
            var Document = await BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync("http://imgur.com/search?q={Search.Replace(' ', '+')}");
            var Elements = Document.QuerySelectorAll("a.image-list-link").ToList();
            if (Elements.Any()) return;
            var Image = (Elements.ElementAtOrDefault(Context.Random.Next(Elements.Count))?.Children.FirstOrDefault() as IHtmlImageElement);
            if (Image?.Source == null) return;
            await ReplyAsync(Image.Source.Replace("b", "."));
        }
    }
}