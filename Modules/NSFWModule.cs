using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Rick.Attributes;

namespace Rick.Modules
{
    [RequiredChannel]
    public class NSFWModule : ModuleBase
    {
        [Command("Tests")]
        public async Task TestAsync()
        {
            await ReplyAsync("Worked");
        }

        [Command("Breasts"), Remarks("Fetches Female breasts from the api.")]
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

        [Command("Rear"), Summary("MotherOfGod"), Remarks("I can't believe you need help with this command..")]
        public async Task Butts()
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
    }
}