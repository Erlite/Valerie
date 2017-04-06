using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using Meeseeks.ModulesAddon;
using Meeseeks.GuildHandlers;

namespace Meeseeks.Modules
{
    [RequireContext(ContextType.Guild)]
    public class TagModule : ModuleBase<CustomCommandContext>
    {
        [Command("Tag")]
        [Summary("Show a tag")]
        public async Task Tag([ Remainder] string tag = null)
        {
            if (!Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag))
            {
                await ReplyAsync("Tag not found.");
                return;
            }
            Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).GetTag(tag);
            IUser user = await Context.Client.GetUserAsync(_tag.creator);
            var embed = new EmbedBuilder()
                .WithTitle($"Tag: {_tag.tag}")
                .WithFooter(x =>
                {
                    x.Text = $"Created by: {user.Username}#{user.Discriminator}";
                    x.IconUrl = user.GetAvatarUrl();
                })
                .WithTimestamp(_tag.when);
            string text = "";
            Match m;
            if (Regex.Match(_tag.text, "(https?://)?((www.)?youtube.com|youtu.?be)/.+").Success)
            {
                embed = null;
                text = _tag.text;
            }
            else if ((m = Regex.Match(_tag.text, "(http)?s?:?(//[^\"']*\\.(?:png|jpg|jpeg|gif|png|svg))")).Success)
            {
                embed.ImageUrl = m.Captures[0].Value;
                embed.Description = _tag.text;
            }
            else
                embed.Description = _tag.text;
            await ReplyAsync(text, embed: embed);
        }

        [Command("Create")]
        [Summary("Create a new tag")]
        public async Task Create(string tag = null, [Remainder] string text = null)
        {
            if (Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag) || tag.Equals("create", StringComparison.OrdinalIgnoreCase) || tag.Equals("delete", StringComparison.OrdinalIgnoreCase) || tag.Equals("edit", StringComparison.OrdinalIgnoreCase) || tag.Equals("info", StringComparison.OrdinalIgnoreCase) || tag.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                await ReplyAsync("This tag already exists.");
                return;
            }
            if (text.Length > 1900)
            {
                await ReplyAsync("Text exceeds limit (> 1900).");
                return;
            }
            Context.MainHandler.GuildTagHandler(Context.Guild).CreateTag(Context.User, tag, text);
            await ReplyAsync($"Tag {tag} created!");
        }

        [Command("Delete")]
        [Summary("Delete an existing tag")]
        public async Task Delete(string tag = null)
        {
            if (!Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag))
            {
                await ReplyAsync("Tag not found.");
                return;
            }
            Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).GetTag(tag);
            if (!await Context.MainHandler.PermissionHandler.IsAdminAsync(Context.User) && _tag.creator != Context.User.Id)
            {
                await ReplyAsync("This tag isn't yours.");
                return;
            }
            Context.MainHandler.GuildTagHandler(Context.Guild).RemoveTag(tag);
            await ReplyAsync($"Tag {tag} deleted.");
        }

        [Command("Edit")]
        [Summary("Edit an existing tag")]
        public async Task Edit( string tag = null, [Remainder] string text = null)
        {
            if (!Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag))
            {
                await ReplyAsync("Tag not found.");
                return;
            }
            Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).GetTag(tag);
            if (!await Context.MainHandler.PermissionHandler.IsAdminAsync(Context.User) && _tag.creator != Context.User.Id)
            {
                await ReplyAsync("This tag isn't yours.");
                return;
            }
            Context.MainHandler.GuildTagHandler(Context.Guild).EditTag(tag, text);
            await ReplyAsync($"Tag {tag} edited.");
        }
    }
}