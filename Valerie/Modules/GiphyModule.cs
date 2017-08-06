using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Discord;
using Discord.Commands;
using Valerie.Attributes;
using Valerie.Handlers.ConfigHandler;
using Valerie.Models;
using Valerie.Extensions;

namespace Valerie.Modules
{
    [Group("Giphy"), RequireAPIKeys, RequireBotPermission(ChannelPermission.SendMessages)]
    public class GiphyModule : CommandBase
    {
        string GifsEndpoint = "https://api.giphy.com/v1/gifs/";
        string StickersEndpoint = "http://api.giphy.com/v1/stickers/";
        string Key = $"api_key={BotDB.Config.APIKeys.GiphyKey}";

        [Command, Summary("Searches Giphy for your Gifs??"), Alias("Gif"), Priority(0)]
        public async Task Giphy([Remainder] string Terms)
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

        [Command("Tag"), Summary("Searches Giphy for your tag"), Priority(1)]
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

        [Command("Stickers"), Summary("Animated stickers gifs?"), Priority(1)]
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
