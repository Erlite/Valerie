using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Rick.Attributes;
using Rick.Services;


namespace Rick.Modules
{
    [RequireChannel, RequiredBlacklist]
    public class NSFWModule : ModuleBase
    {
        [Command("Boobs"), Remarks("Fetches Female breasts from the api.")]
        public async Task BoobsAsync()
        {
            try
            {
                JToken obj;
                using (var http = new HttpClient())
                {
                    obj = JArray.Parse(await http.GetStringAsync($"http://api.oboobs.ru/boobs/{ new Random().Next(0, 10229) }"))[0];
                }
                await Context.Channel.SendMessageAsync($"http://media.oboobs.ru/{ obj["preview"].ToString() }");
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("Bum"), Summary("MotherOfGod"), Remarks("I can't believe you need help with this command..")]
        public async Task BumsAsync()
        {
            try
            {
                JToken obj;
                using (var http = new HttpClient())
                {
                    obj = JArray.Parse(await http.GetStringAsync($"http://api.obutts.ru/butts/{ new Random().Next(0, 4222) }"))[0];
                }
                await Context.Channel.SendMessageAsync($"http://media.obutts.ru/{ obj["preview"].ToString() }");
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("E621"), Summary("E621 Kawaii"), Remarks("Never used this command. Don't ask me")]
        public async Task E621Async(string search = null)
        {
            if (string.IsNullOrWhiteSpace(search))
                throw new NullReferenceException("Please provide me a search term!");
            search = search?.Trim() ?? "";
            var url = await MethodService.GetE621ImageLink(search);
            if (url == null)
                await ReplyAsync(Context.User.Mention + " No results found! Try another term?");
            else
            {
                var embed = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x.Name = $"{Context.User.Username} searched for {search}";
                        x.IconUrl = url;
                    })
                    .WithImageUrl(url);
                await ReplyAsync("", embed: embed);
            }
        }
    }
}