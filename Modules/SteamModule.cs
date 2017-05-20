using System;
using System.Collections.Generic;
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

namespace Rick.Modules
{
    [CheckBlacklist, Group("Steam")]
    public class SteamModule : ModuleBase
    {
        [Command("News")]
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
    }
}
