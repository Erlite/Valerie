﻿using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using System.Net.Http;
using System;
using Newtonsoft.Json.Linq;
using Google.Apis.YouTube.v3;
using Rick.Enums;
using Rick.Extensions;
using Rick.Attributes;
using Rick.Handlers;
using Rick.Functions;
using Discord.WebSocket;
using Discord.Commands;

namespace Rick.Modules
{
    [CheckBlacklist, APICheck]
    public class GoogleModule : ModuleBase
    {
        [Command("Google"), Alias("G"), Summary("Searches google for your search terms."), Remarks("Google What is love?")]
        public async Task GoogleAsync([Remainder] string search)
        {
            var Str = new StringBuilder();
            Uri URL = new Uri("http://diylogodesigns.com/blog/wp-content/uploads/2016/04/google-logo-icon-PNG-Transparent-Background.png");
            
            var Service = new CustomsearchService(new BaseClientService.Initializer
            {
                ApiKey = ConfigHandler.IConfig.APIKeys.GoogleKey
            });
            var RequestList = Service.Cse.List(search);
            RequestList.Cx = ConfigHandler.IConfig.APIKeys.SearchEngineID;

            var items = RequestList.Execute().Items.Take(6);
            foreach (var result in items)
            {
                Str.AppendLine($"• **{result.Title}**\n{result.Snippet}\n{Function.ShortenUrl(result.Link)}\n");
            }

            var embed = EmbedExtension.Embed(EmbedColors.Pastle, $"Searched for: {search}", 
                new Uri(Context.Client.CurrentUser.GetAvatarUrl()), Description: Str.ToString(), ThumbUrl: URL);

            if (string.IsNullOrWhiteSpace(Str.ToString()) || Str.ToString() == null)
                await ReplyAsync("No results found!");
            else
                await ReplyAsync("", embed: embed);
        }

        [Command("GImage"), Summary("Searches google for your image and returns a random image from 50 results."), Remarks("GImage Dank Memes")]
        public async Task GImageAsync([Remainder] string search)
        {
            using (var http = new HttpClient())
            {
                var rng = new Random();
                var reqString = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString(search)}&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start={ rng.Next(1, 50) }&fields=items%2Flink&key={ConfigHandler.IConfig.APIKeys.GoogleKey}";
                var obj = JObject.Parse(await http.GetStringAsync(reqString));
                var items = obj["items"] as JArray;
                var image = items[0]["link"].ToString();
                var embed = EmbedExtension.Embed(EmbedColors.Yellow, $"Searched for: {search}", 
                    new Uri(Context.Client.CurrentUser.GetAvatarUrl()), ImageUrl: new Uri(image));
                await ReplyAsync("", embed: embed);
            }
        }

        [Command("Youtube"), Alias("Yt"), Summary("Searches the first search result from youtube."), Remarks("Youtube SomeVideo Name")]
        public async Task YoutubeAsync([Remainder] string search)
        {
            var Service = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = ConfigHandler.IConfig.APIKeys.GoogleKey
            });
            var SearchRequest = Service.Search.List("snippet");
            SearchRequest.Q = search;
            SearchRequest.MaxResults = 1;
            SearchRequest.Type = "video";
            var SearchResponse = (await SearchRequest.ExecuteAsync()).Items.Select(x => "http://www.youtube.com/watch?v=" + x.Id.VideoId).FirstOrDefault();
            await ReplyAsync(SearchResponse);

        }

        [Command("Shorten"), Summary("Shortens a URL using Google URL Shortner."), Remarks("Shorten https://github.com/ExceptionDev"),]
        public async Task ShortenAsync([Remainder] string URL)
        {
            await ReplyAsync($"This is your shortened URL: {Function.ShortenUrl(URL)}");
        }

        [Command("Revav"), Summary("Performs a reverse image search for a user avatar.")]
        public async Task RevavAsync(SocketGuildUser User)
        {
            await ReplyAsync(
                $"Reverse Image Result: " +
                $"{Function.ShortenUrl($"https://images.google.com/searchbyimage?image_url={User.GetAvatarUrl()}")}");
        }
    }
}