using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using Rick.Classes;
using Rick.Models;
using Discord.Addons.InteractiveCommands;

namespace Rick.Modules
{
    [Group("Tag")]
    public class TagModule : ModuleBase
    {
        private readonly InteractiveService Interactive;
        public TagModule(InteractiveService Inter)
        {
            Interactive = Inter;
        }

        [Command("Create", RunMode = RunMode.Async), Summary("Tag Create"), Remarks("Creates a tag for you")]
        public async Task CreateTagAsync()
        {
            await ReplyAsync("**Please enter the name of your tag** _'cancel' to cancel_");
            var TagName = await Interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(20));
            if (TagName.Content == "cancel") return;
            string name = TagName.Content;

            await ReplyAsync("**Please enter the response of your tag** _'cancel' to cancel_");
            var TagResponse = await Interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(20));
            if (TagResponse.Content == "cancel") return;
            string content = TagResponse.Content;

            var embed = new EmbedBuilder()
                .WithTitle(name)
                .WithDescription(content)
                .WithColor(new Color(255, 255, 255))
                .WithAuthor(x => { x.IconUrl = Context.User.GetAvatarUrl(); x.Name = Context.User.Username; });
            try
            {
                GuildModel.GuildConfigs[Context.Guild.Id].Tags.Add(name, content);
                await ReplyAsync("Tag Added", embed: embed);
            }
            catch
            {
                await ReplyAsync("Failed to add tag");
            }

        }

        [Command("Execute", RunMode = RunMode.Async)]
        public async Task ExecuteTagAsync(string TagName)
        {
            string content;
            var tagExists = GuildModel.GuildConfigs[Context.Guild.Id].Tags.TryGetValue(TagName, out content);
            if (tagExists)
            {
                await ReplyAsync(content);
            }
            else
            {
                await ReplyAsync("Tag couldn't be found :anger: ");
            }
        }

        //[Command("Tag")]
        //[Summary("Show a tag")]
        //public async Task Tag([ Remainder] string tag = null)
        //{
        //    if (!Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag))
        //    {
        //        await ReplyAsync("Tag not found.");
        //        return;
        //    }
        //    Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).GetTag(tag);
        //    IUser user = await Context.Client.GetUserAsync(_tag.creator);
        //    var embed = new EmbedBuilder()
        //        .WithTitle($"Tag: {_tag.tag}")
        //        .WithFooter(x =>
        //        {
        //            x.Text = $"Created by: {user.Username}#{user.Discriminator}";
        //            x.IconUrl = user.GetAvatarUrl();
        //        })
        //        .WithTimestamp(_tag.when);
        //    string text = "";
        //    Match m;
        //    if (Regex.Match(_tag.text, "(https?://)?((www.)?youtube.com|youtu.?be)/.+").Success)
        //    {
        //        embed = null;
        //        text = _tag.text;
        //    }
        //    else if ((m = Regex.Match(_tag.text, "(http)?s?:?(//[^\"']*\\.(?:png|jpg|jpeg|gif|png|svg))")).Success)
        //    {
        //        embed.ImageUrl = m.Captures[0].Value;
        //        embed.Description = _tag.text;
        //    }
        //    else
        //        embed.Description = _tag.text;
        //    await ReplyAsync(text, embed: embed);
        //}

        //[Command("Create")]
        //[Summary("Create a new tag")]
        //public async Task Create(string tag = null, [Remainder] string text = null)
        //{
        //    if (Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag) || tag.Equals("create", StringComparison.OrdinalIgnoreCase) || tag.Equals("delete", StringComparison.OrdinalIgnoreCase) || tag.Equals("edit", StringComparison.OrdinalIgnoreCase) || tag.Equals("info", StringComparison.OrdinalIgnoreCase) || tag.Equals("help", StringComparison.OrdinalIgnoreCase))
        //    {
        //        await ReplyAsync("This tag already exists.");
        //        return;
        //    }
        //    if (text.Length > 1900)
        //    {
        //        await ReplyAsync("Text exceeds limit (> 1900).");
        //        return;
        //    }
        //    Context.MainHandler.GuildTagHandler(Context.Guild).CreateTag(Context.User, tag, text);
        //    await ReplyAsync($"Tag {tag} created!");
        //}

        //[Command("Delete")]
        //[Summary("Delete an existing tag")]
        //public async Task Delete(string tag = null)
        //{
        //    if (!Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag))
        //    {
        //        await ReplyAsync("Tag not found.");
        //        return;
        //    }
        //    Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).GetTag(tag);
        //    if (!await Context.MainHandler.PermissionHandler.IsAdminAsync(Context.User) && _tag.creator != Context.User.Id)
        //    {
        //        await ReplyAsync("This tag isn't yours.");
        //        return;
        //    }
        //    Context.MainHandler.GuildTagHandler(Context.Guild).RemoveTag(tag);
        //    await ReplyAsync($"Tag {tag} deleted.");
        //}

        //[Command("Edit")]
        //[Summary("Edit an existing tag")]
        //public async Task Edit( string tag = null, [Remainder] string text = null)
        //{
        //    if (!Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag))
        //    {
        //        await ReplyAsync("Tag not found.");
        //        return;
        //    }
        //    Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).GetTag(tag);
        //    if (!await Context.MainHandler.PermissionHandler.IsAdminAsync(Context.User) && _tag.creator != Context.User.Id)
        //    {
        //        await ReplyAsync("This tag isn't yours.");
        //        return;
        //    }
        //    Context.MainHandler.GuildTagHandler(Context.Guild).EditTag(tag, text);
        //    await ReplyAsync($"Tag {tag} edited.");
        //}
    }
}