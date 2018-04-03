﻿using Valerie.Models;
using Newtonsoft.Json;
using System.Net.Http;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Valerie.Helpers
{
    public class MethodHelper
    {
        HttpClient HttpClient { get; }
        DiscordSocketClient Client { get; }
        public MethodHelper(HttpClient httpClient, DiscordSocketClient client)
        {
            HttpClient = httpClient;
            Client = client;
        }

        public async Task<IReadOnlyCollection<GithubModel>> GetCommitsAsync()
        {
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var Request = await HttpClient.GetAsync("https://api.github.com/repos/Yucked/Valerie/commits");
            var Content = JsonConvert.DeserializeObject<IReadOnlyCollection<GithubModel>>(await Request.Content.ReadAsStringAsync());
            HttpClient.DefaultRequestHeaders.Clear();
            return Content;
        }
    }
}