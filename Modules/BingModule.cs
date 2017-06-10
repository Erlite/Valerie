using System.Threading.Tasks;
using Discord.Commands;
using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Rick.Handlers;
using Rick.Services;
using System.Text;
using Rick.Attributes;
using Rick.Enums;
using Rick.JsonResponse;
using Rick.Extensions;

namespace Rick.Modules
{
    [CheckBlacklist]
    public class BingModule : ModuleBase
    {
        [Command("BImage"), Summary("Performs a bing image search for your query and replies back with a random image."), Remarks("BImage Tiny Rick")]
        public async Task ImageAsync([Remainder] string Query)
        {
            if (string.IsNullOrWhiteSpace(Query))
            {
                await ReplyAsync("A search term should be provided for me to search!");
                return;
            }
            using (var httpClient = new HttpClient())
            {
                var link = $"https://api.cognitive.microsoft.com/bing/v5.0/images/search?q={Query}&count=50&offset=0&mkt=en-us&safeSearch=Off";
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", BotHandler.BotConfig.APIKeys.BingKey);
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
                var Random = new Random();
                var RandomNum = Random.Next(1, 50);
                JObject image = (JObject)arr[RandomNum];
                var embed = EmbedExtension.Embed(EmbedColors.Cyan, $"Search Term:   {Query.ToUpper()}", Context.Client.CurrentUser.GetAvatarUrl(), ImageUrl: (string)image["contentUrl"]);
                await ReplyAsync("", embed: embed);
            }

        }

        [Command("Bing"), Summary("Performs a bing search for your query and replies back with 5 search results."), Remarks("Bing Tiny Rick")]
        public async Task SearchAsync([Remainder]string Query)
        {
            if (string.IsNullOrWhiteSpace(Query))
            {
                await ReplyAsync("Search terms can't be empty!");
                return;
            }
            using (var Http = new HttpClient())
            {
                Http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", BotHandler.BotConfig.APIKeys.BingKey);
                var GetRequest = await Http.GetAsync($"https://api.cognitive.microsoft.com/bing/v5.0/search?q={Query}&count=5&offset=0&mkt=en-us&safeSearch=moderate");
                if (!GetRequest.IsSuccessStatusCode)
                {
                    await ReplyAsync(GetRequest.ReasonPhrase);
                    return;
                }
                var getString = await GetRequest.Content.ReadAsStringAsync();
                var Convert = JToken.Parse(getString).ToObject<SearchRoot>();
                var str = new StringBuilder();
                foreach (var result in Convert.webPages.value)
                {
                    str.AppendLine($"**{result.name}**\n{result.snippet}\n{MethodsService.ShortenUrl(result.displayUrl)}\n");
                }
                var embed = EmbedExtension.Embed(EmbedColors.Cyan, $"Searched For: {Query}", Context.Client.CurrentUser.GetAvatarUrl(), Description: str.ToString(), FooterText: $"Total Results: {Convert.webPages.totalEstimatedMatches.ToString()}");
                await ReplyAsync("", embed: embed);
            }
        }
    }
}
