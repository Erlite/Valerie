using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using RedditSharp;
using RedditSharp.Things;
using Rick.Extensions;
using Rick.Enums;
using Rick.Attributes;
using Rick.Handlers;

namespace Rick.Modules
{
    [CheckBlacklist, Group("Reddit")]
    public class RedditModule : ModuleBase
    {
        [Command("SubInfo")]
        public async Task TestAsync(string SubName)
        {
            var Reddit = new Reddit();
            var SubRed = await Reddit.GetSubredditAsync($"r/{SubName}");
            var Posts = SubRed.GetPosts(5);
            string Desc = $"**Active Users:** {SubRed.ActiveUsers}\n" +
                $"**Description:** {SubRed.Description}\n" +
                $"**Created:** {SubRed.Created}\n" +
                $"**Url:** {SubRed.Url}";
            var embed = EmbedExtension.Embed(EmbedColors.Blurple,
                SubRed.Title, SubRed.HeaderImage,
                Description: Desc, ThumbUrl: SubRed.HeaderImage);
            await ReplyAsync("", embed: embed);
        }
    }
}
