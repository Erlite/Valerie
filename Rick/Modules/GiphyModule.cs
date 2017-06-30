using System;
using System.Threading.Tasks;
using Discord.Commands;
using Rick.Attributes;
using System.Net.Http;
using Rick.Handlers;
using Newtonsoft.Json;
using Rick.JsonModels;

namespace Rick.Modules
{
    [CheckBlacklist, Group("Giphy"), APICheck ]
    public class GiphyModule : ModuleBase
    {
        string GifsEndpoint = "https://api.giphy.com/v1/gifs/";
        string StickersEndpoint = "http://api.giphy.com/v1/stickers/";
        string Key = $"api_key={ConfigHandler.IConfig.APIKeys.GiphyKey}";

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
            var ConvertedJson = JsonConvert.DeserializeObject<Giphy>(await Response.Content.ReadAsStringAsync());
            var items = new Random().Next(0, ConvertedJson.Root.Count);
            await ReplyAsync(ConvertedJson.Root[items].EmbedUrl);
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
            var ConvertedJson = JsonConvert.DeserializeObject<Giphy>(await Response.Content.ReadAsStringAsync());
            var items = new Random().Next(0, ConvertedJson.Root.Count);
            await ReplyAsync(ConvertedJson.Root[items].EmbedUrl);

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
            var ConvertedJson = JsonConvert.DeserializeObject<Giphy>(await Response.Content.ReadAsStringAsync());
            var items = new Random().Next(0, ConvertedJson.Root.Count);
            await ReplyAsync(ConvertedJson.Root[items].EmbedUrl);
        }
    }
}
