using System;
using System.Threading.Tasks;
using Discord.Commands;
using Rick.Attributes;
using System.Net.Http;
using Rick.Handlers;
using Newtonsoft.Json;
using Rick.JsonResponse;

namespace Rick.Modules
{
    [CheckBlacklist, Group("Giphy"), ]
    public class GiphyModule : ModuleBase
    {
        string GifsEndpoint = "https://api.giphy.com/v1/gifs/";
        string StickersEndpoint = "http://api.giphy.com/v1/stickers/";
        string Key = $"api_key={BotHandler.BotConfig.APIKeys.GiphyKey}";

        [Command, Summary("Gif Cute kittens"), Remarks("Searches Giphy for your Gifs??"), Alias("Gif"), Priority(0)]
        public async Task GiphyAsync([Remainder] string Terms)
        {
            var GetUrl = GifsEndpoint + $"search?q={Terms.Replace(' ', '+')}&" + Key;
            var Response = await new HttpClient().GetAsync(GetUrl);
            if (!Response.IsSuccessStatusCode)
            {
                await ReplyAsync(Response.ReasonPhrase);
                return;
            }
            var ConvertedJson = JsonConvert.DeserializeObject<GiphyRoot>(await Response.Content.ReadAsStringAsync());
            var items = new Random().Next(0, ConvertedJson.data.Count);
            await ReplyAsync(ConvertedJson.data[items].embed_url);
        }

        [Command("Tag"), Summary("Giphy Tag Kittens"), Remarks("Searches Giphy for your tag"), Priority(1)]
        public async Task TagsAsync(string Tag)
        {
            var GetUrl = GifsEndpoint + "random?" + Key + $"&tag={Tag}";
            var Response = await new HttpClient().GetAsync(GetUrl);
            if (!Response.IsSuccessStatusCode)
            {
                await ReplyAsync(Response.ReasonPhrase);
                return;
            }
            var ConvertedJson = JsonConvert.DeserializeObject<GiphyRoot>(await Response.Content.ReadAsStringAsync());
            var items = new Random().Next(0, ConvertedJson.data.Count);
            await ReplyAsync(ConvertedJson.data[items].embed_url);

        }

        [Command("Stickers"), Summary("Giphy Stickers Dank Memes"), Remarks("Animated stickers rather than gifs")]
        public async Task StickersAsync([Remainder] string Search)
        {
            var GetUrl = StickersEndpoint + $"search?q={Search.Replace(' ', '+')}&" + Key;
            var Response = await new HttpClient().GetAsync(GetUrl);
            if (!Response.IsSuccessStatusCode)
            {
                await ReplyAsync(Response.ReasonPhrase);
                return;
            }
            var ConvertedJson = JsonConvert.DeserializeObject<GiphyRoot>(await Response.Content.ReadAsStringAsync());
            var items = new Random().Next(0, ConvertedJson.data.Count);
            await ReplyAsync(ConvertedJson.data[items].embed_url);
        }
    }
}
