using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nygma.Handlers;
using Nygma.Utils;
using Discord;
using Discord.Commands;
using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using Nygma.Services;


namespace Nygma.Modules
{
    [Group("G")]
    public class GoogleModule : ModuleBase
    {
        private static GoogleService g;
        [Command("Youtube"), Alias("YT")]
        public async Task Youtube([Remainder] string query = null)
        {
            if (!(await Misc.ValidateQuery(Context.Channel, query))) return;
            var result = (await g.GetVideosByKeywordsAsync(query, 1)).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(result))
            {
                await Context.Channel.SendMessageAsync("No results found for that query.");
                return;
            }
            await Context.Channel.SendMessageAsync(result);
        }

        [Command]
        public async Task GoogleAsync([Remainder] string terms = null)
        {
            terms = terms?.Trim();
            if (string.IsNullOrWhiteSpace(terms))
                throw new ArgumentException("Please enter a search term!");

            terms = WebUtility.UrlEncode(terms).Replace(' ', '+');

            var fullQueryLink = $"https://www.google.com/search?q={ terms }&gws_rd=cr,ssl";
            var config = Configuration.Default.WithDefaultLoader();
            var document = await BrowsingContext.New(config).OpenAsync(fullQueryLink);

            var elems = document.QuerySelectorAll("div.g");

            var resultsElem = document.QuerySelectorAll("#resultStats").FirstOrDefault();
            var totalResults = resultsElem?.TextContent;


            if (!elems.Any())
                return;

            var results = elems.Select<IElement, GoogleSearchResult?>(elem =>
            {
                var aTag = (elem.Children.FirstOrDefault().Children.FirstOrDefault() as IHtmlAnchorElement);
                var href = aTag?.Href;
                var name = aTag?.TextContent;
                if (href == null || name == null)
                    return null;

                var txt = elem.QuerySelectorAll(".st").FirstOrDefault()?.TextContent;

                if (txt == null)
                    return null;

                return new GoogleSearchResult(name, href, txt);
            }).Where(x => x != null).Take(5);

            var desc = await Task.WhenAll(results.Select(async res => $"[{Format.Bold(res?.Title)}]({((res?.Link))})\n{res?.Text}\n\n"));
            var embed = new EmbedBuilder()
                .WithColor(Misc.RandColor())
                .WithAuthor(eab => eab.WithName("Search For: " + terms)
                .WithUrl(fullQueryLink)
                .WithIconUrl("http://i.imgur.com/G46fm8J.png"))
                .WithTitle(Context.User.Mention)
                .WithFooter(efb => efb.WithText(totalResults))
                .WithDescription(String.Concat(desc));


            await ReplyAsync("", false, embed);
        }

        [Command("Image")]
        public async Task Image([Remainder] string terms = null)
        {
            terms = terms?.Trim();
            if (string.IsNullOrWhiteSpace(terms))
                return;

            terms = WebUtility.UrlEncode(terms).Replace(' ', '+');

            try
            {
                var res = await g.GetImageAsync(terms);
                var embed = new EmbedBuilder()
                    .WithColor(Misc.RandColor())
                    .WithAuthor(eab => eab.WithName("Image Search For: " + terms.TrimTo(50))
                        .WithUrl("https://www.google.rs/search?q=" + terms + "&source=lnms&tbm=isch")
                        .WithIconUrl("http://i.imgur.com/G46fm8J.png"))
                    .WithDescription(res.Link)
                    .WithImageUrl(res.Link)
                    .WithTitle(Context.User.Mention);
                await Context.Channel.SendMessageAsync("", embed: embed);
            }
            catch
            {
                IConsole.Log(LogSeverity.Error, "API FAILURE","Falling back to Imgur search.");

                var fullQueryLink = $"http://imgur.com/search?q={ terms }";
                var config = Configuration.Default.WithDefaultLoader();
                var document = await BrowsingContext.New(config).OpenAsync(fullQueryLink);

                var elems = document.QuerySelectorAll("a.image-list-link");

                if (!elems.Any())
                    return;

                var img = (elems.FirstOrDefault()?.Children?.FirstOrDefault() as IHtmlImageElement);

                if (img?.Source == null)
                    return;

                var source = img.Source.Replace("b.", ".");

                var embed = new EmbedBuilder()
                    .WithColor(Misc.RandColor())
                    .WithAuthor(eab => eab.WithName("Image Search For: " + terms.TrimTo(50))
                        .WithUrl(fullQueryLink)
                        .WithIconUrl("http://s.imgur.com/images/logo-1200-630.jpg?"))
                    .WithDescription(source)
                    .WithImageUrl(source)
                    .WithTitle(Context.User.Mention);
                await Context.Channel.SendMessageAsync("", embed: embed);
            }
        }

        [Command("RandomImage")]
        public async Task RandomImage([Remainder] string terms = null)
        {
            terms = terms?.Trim();
            if (string.IsNullOrWhiteSpace(terms))
                return;
            terms = WebUtility.UrlEncode(terms).Replace(' ', '+');
            try
            {
                var res = await g.GetImageAsync(terms, new Random().Next(0, 50));
                var embed = new EmbedBuilder()
                    .WithColor(Misc.RandColor())
                    .WithAuthor(eab => eab.WithName("Image Search For: " + terms.TrimTo(50))
                        .WithUrl("https://www.google.rs/search?q=" + terms + "&source=lnms&tbm=isch")
                        .WithIconUrl("http://i.imgur.com/G46fm8J.png"))
                    .WithDescription(res.Link)
                    .WithImageUrl(res.Link)
                    .WithTitle(Context.User.Mention);
                await Context.Channel.SendMessageAsync("", embed: embed);
            }
            catch
            {
                IConsole.Log(LogSeverity.Error, "API FAILURE", "Falling back to Imgur search.");
                terms = WebUtility.UrlEncode(terms).Replace(' ', '+');

                var fullQueryLink = $"http://imgur.com/search?q={ terms }";
                var config = Configuration.Default.WithDefaultLoader();
                var document = await BrowsingContext.New(config).OpenAsync(fullQueryLink);

                var elems = document.QuerySelectorAll("a.image-list-link").ToList();

                if (!elems.Any())
                    return;

                var img = (elems.ElementAtOrDefault(new Random().Next(0, elems.Count))?.Children?.FirstOrDefault() as IHtmlImageElement);

                if (img?.Source == null)
                    return;

                var source = img.Source.Replace("b.", ".");

                var embed = new EmbedBuilder()
                    .WithColor(Misc.RandColor())
                    .WithAuthor(eab => eab.WithName("Image Search For: " + terms.TrimTo(50))
                        .WithUrl(fullQueryLink)
                        .WithIconUrl("http://s.imgur.com/images/logo-1200-630.jpg?"))
                    .WithDescription(source)
                    .WithImageUrl(source)
                    .WithTitle(Context.User.Mention);
                await Context.Channel.SendMessageAsync("", embed: embed);
            }
        }
    }
}