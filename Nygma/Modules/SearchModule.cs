using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.CSharp.RuntimeBinder;
using System.Net;
using System.Text.RegularExpressions;
using Nygma.Utils;
using Nygma.Handlers;

namespace Nygma.Modules
{
    public class SearchModule  : ModuleBase
    {
        private DependencyMap _map;
        private ConfigHandler config;
        public SearchModule(ConfigHandler Con)
        {
            config = Con;
        }

        [Command("Gif"), Summary("Gif Cute kittens"), Remarks("Searches gif for your Gifs??")]
        public async Task gifs([Remainder] string keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords))
                throw new ArgumentException("What do you want me to search for?");

            var getUrl = new Uri("http://api.giphy.com/");
            using (var client = new HttpClient())
            {
                client.BaseAddress = getUrl;
                var response = await client.GetAsync(Uri.EscapeDataString($"v1/gifs/random?api_key=dc6zaTOxFJmzC&tag={Uri.UnescapeDataString(keywords)}"));
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(jsonResponse);

                await Context.Message.DeleteAsync();
                var embed = new EmbedBuilder()
                    .WithTitle($"Searched for => {keywords}")
                    .WithImageUrl(obj["data"]["image_original_url"].ToString())
                    .WithColor(Misc.RandColor());
                await ReplyAsync("", false, embed);
            }
        }

        [Command("Yodify"), Summary("Yodify Abalbhahah LOLOLOl? Foxbot"), Remarks("Yodifies your sentence"), Alias("Yod")]
        public async Task Yodify([Remainder] string query)
        {
            var channel = (ITextChannel)Context.Channel;

            if (string.IsNullOrWhiteSpace(config.MAPI))
            {
                await channel.SendMessageAsync("Bot owner didn't specify MashapeApiKey. You can't use this functionality.");
                return;
            }
            var arg = query;
            if (string.IsNullOrWhiteSpace(arg))
            {
                await channel.SendMessageAsync("Please enter a sentence.");
                return;
            }
            await Context.Channel.TriggerTypingAsync();
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Clear();
                http.DefaultRequestHeaders.Add("X-Mashape-Key", config.MAPI);
                http.DefaultRequestHeaders.Add("Accept", "text/plain");
                var res = await http.GetStringAsync($"https://yoda.p.mashape.com/yoda?sentence={Uri.EscapeUriString(arg)}");
                await Context.Message.DeleteAsync();
                try
                {
                    var embed = new EmbedBuilder()
                        .WithUrl("http://www.yodaspeak.co.uk/")
                        .WithAuthor(au => au.WithName("Yoda").WithIconUrl("http://www.yodaspeak.co.uk/yoda-small1.gif"))
                        .WithDescription(res)
                        .WithColor(Misc.RandColor());
                    await channel.SendMessageAsync("", false, embed);
                }
                catch
                {
                    await channel.SendMessageAsync("Failed to yodify your sentence.");
                }
            }
        }

        [Command("Lmgtfy"), Summary("Lmg How To Google"), Remarks("Googles something for that special person who is crippled"), Alias("Lmg")]
        public async Task Lmgtfy([Remainder] string ffs = null)
        {
            if (string.IsNullOrWhiteSpace(ffs))
                throw new ArgumentException("This cmd is to search something for an idiot! YOU ARE BEING AN IDIOT!");
            await Context.Message.DeleteAsync();
            await ReplyAsync($"<http://lmgtfy.com/?q={ Uri.EscapeUriString(ffs) }>");
        }

        [Command("Catfacts"), Summary("Catfacts"), Remarks("Catfacts for cat lovers"), Alias("CF")]
        public async Task Catfact()
        {
            using (var http = new HttpClient())
            {
                var response = await http.GetStringAsync("http://catfacts-api.appspot.com/api/facts");
                if (response == null)
                    return;
                var fact = JObject.Parse(response)["facts"][0].ToString();
                await Context.Message.DeleteAsync();
                var embed = new EmbedBuilder();
                embed.Title = "Random Cat Fact";
                embed.Description = $":feet: {fact} :feet:";
                embed.Color = Misc.RandColor();
                await ReplyAsync("", false, embed);
            }
        }

        [Command("Wiki"), Summary("Wiki Putin"), Remarks("WIKIPEDIA's")]
        public async Task Wiki([Remainder]string search = null)
        {
            if (string.IsNullOrWhiteSpace(search))
                throw new ArgumentException("What do you want me to search for?");

            HttpClient httpClient = new HttpClient();
            var c = Context.Message.Content;
            var responseText = httpClient.HttpGet($"https://en.wikipedia.org/w/api.php?action=opensearch&search={search}");
            dynamic responseObject = JsonConvert.DeserializeObject(responseText);
            await Context.Message.DeleteAsync();
            try
            {
                string title = responseObject[1][0];
                string url = responseObject[3][0];
                string firstParagraph = responseObject[2][0];

                var embed = new EmbedBuilder()
                    .WithTitle(title)
                    .WithUrl(url)
                    .WithDescription(firstParagraph)
                    .WithColor(Misc.RandColor());
                await ReplyAsync("", false, embed);
            }
            catch (ArgumentException)
            {
                await ReplyAsync("Search returned no result");

            }
            catch (RuntimeBinderException)
            {
                await ReplyAsync("Search returned no result");
            }
        }

        [Command("Define"), Summary("Define Stupid"), Remarks("Do you really need this?"), Alias("Def")]
        public async Task Define([Remainder] string word = null)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentException("You don't want me to define anything?");

            await Context.Message.DeleteAsync();
            using (var http = new HttpClient())
            {
                var res = await http.GetStringAsync("http://api.pearson.com/v2/dictionaries/entries?headword=" + WebUtility.UrlEncode(word.Trim()));

                var data = JsonConvert.DeserializeObject<DefineModel>(res);

                var sense = data.Results.Where(x => x.Senses != null && x.Senses[0].Definition != null).FirstOrDefault()?.Senses[0];

                if (sense?.Definition == null)
                    return;

                string definition = sense.Definition.ToString();
                if (!(sense.Definition is string))
                    definition = ((JArray)JToken.Parse(sense.Definition.ToString())).First.ToString();

                var embed = new EmbedBuilder()
                    .WithTitle("Define: " + word)
                    .WithDescription(definition)
                    .WithFooter(efb => efb.WithText(sense.Gramatical_info?.type))
                    .WithColor(Misc.RandColor());

                if (sense.Examples != null)
                    embed.AddField(efb => efb.WithName("Example").WithValue(sense.Examples.First().text));

                await ReplyAsync("", false, embed);
            }
        }

        [Command("image")]
        public async Task Image([Remainder] string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                throw new ArgumentException("Not sure what I should search for??");

            await Context.Message.DeleteAsync();
            using (var httpClient = new HttpClient())
            {
                var link = $"https://api.cognitive.microsoft.com/bing/v5.0/images/search?q={search}&count=10&offset=0&mkt=en-us&safeSearch=Moderate";
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.BAPI);
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
                JObject image = (JObject)arr[0];
                EmbedBuilder eb = new EmbedBuilder();
                eb.Title = $"Image: {search}";
                eb.Color = Misc.RandColor();
                eb.Description = $"[Image link]({(string)image["contentUrl"]})";
                eb.ImageUrl = (string)image["contentUrl"];
                await ReplyAsync("", false, eb);
            }
        }

        [Command("Urban")]
        public async Task Urban([Remainder] string urban)
        {
            await Context.Message.DeleteAsync();
            var data = new EmbedBuilder();
            var vc = new HttpClient();
            data.WithAuthor(x =>
            {
                x.Name = "Urban Dictionary";
                x.WithIconUrl("http://is1.mzstatic.com/image/thumb/Purple49/v4/51/d1/2d/51d12d50-2991-00f6-6702-354ccb849a80/source/100x100bb.jpg");
            });
            string req = await vc.GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + urban);
            data.WithColor(Misc.RandColor());

            MatchCollection col = Regex.Matches(req, @"(?<=definition"":"")[ -z~-🧀]+(?="",""permalink)");
            MatchCollection col2 = Regex.Matches(req, @"(?<=example"":"")[ -z~-🧀]+(?="",""thumbs_down)");
            if (col.Count == 0)
            {
                await ReplyAsync("Eh NOTHING FOUND :((((( KMS");
                return;
            }
            Random r = new Random();
            string outpt = "Failed fetching data from Urban Dictionary, please try later!";
            string outpt2 = "No Example";
            int max = r.Next(0, col.Count);
            for (int i = 0; i <= max; i++)
            {
                outpt = urban + "\r\n\r\n" + col[i].Value;
            }

            for (int i = 0; i <= max; i++)
            {
                outpt2 = "\r\n\r\n" + col2[i].Value;
            }

            outpt = outpt.Replace("\\r", "\r");
            outpt = outpt.Replace("\\n", "\n");
            outpt2 = outpt2.Replace("\\r", "\r");
            outpt2 = outpt2.Replace("\\n", "\n");

            data.AddField(x =>
            {
                x.Name = $"Definition";
                x.Value = outpt;
            });

            data.AddField(x =>
            {
                x.Name = "Example";
                x.Value = outpt2;
            });

            await ReplyAsync("", embed: data);
        }
    }
}