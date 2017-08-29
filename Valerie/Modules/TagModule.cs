using System;
using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using Valerie.Handlers.GuildHandler;
using Valerie.Handlers.GuildHandler.Enum;
using Valerie.Extensions;

namespace Valerie.Modules
{
    [Group("Tag"), RequireBotPermission(Discord.ChannelPermission.SendMessages)]
    public class TagModule : CommandBase
    {
        [Command, Summary("Executes a tag."), Priority(0)]
        public async Task Tag(string TagName)
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            var Tag = Config.TagsList.FirstOrDefault(x => x.Name == TagName);
            if (Tag == null)
            {
                await ReplyAsync($"Tag with name **{TagName}** doesn't exist.");
                return;
            }
            await ReplyAsync(Tag.Response);
            await ServerDB.TagsHandlerAsync(Context.Guild.Id, ModelEnum.TagUpdate, TagName);
        }

        [Command("Create"), Summary("Creates a tag."), Priority(1)]
        public async Task CreateAsync(string Name, [Remainder]string Response)
        {
            var Exists = ServerDB.GuildConfig(Context.Guild.Id).TagsList.FirstOrDefault(x => x.Name == Name);
            if (ServerDB.GuildConfig(Context.Guild.Id).TagsList.Contains(Exists))
            {
                await ReplyAsync($"**{Name}** tag already exists.");
                return;
            }
            await ServerDB.TagsHandlerAsync(Context.Guild.Id, ModelEnum.TagAdd, Name, Response, Context.User.Id.ToString(), DateTime.Now.ToString());
            await ReplyAsync($"**{Name}** tag has been created.");
        }

        [Command("Remove"), Alias("Delete"), Summary("Deletes a tag."), Priority(1)]
        public async Task RemoveAsync(string Name)
        {
            var Exists = ServerDB.GuildConfig(Context.Guild.Id).TagsList.FirstOrDefault(x => x.Name == Name);
            if (Exists == null)
            {
                await ReplyAsync($"**{Name}** tag doesn't exists.");
                return;
            }
            if (Convert.ToUInt64(Exists.Owner) != Context.User.Id)
            {
                await ReplyAsync($"You are not the owner of **{Name}**.");
                return;
            }
            await ServerDB.TagsHandlerAsync(Context.Guild.Id, ModelEnum.TagRemove, Name);
            await ReplyAsync($"**{Name}** tag has been removed.");
        }

        [Command("Modify"), Summary("Changes Tag's response"), Priority(1)]
        public async Task ModifyAsync(string Name, [Remainder]string Response)
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            var Tag = Config.TagsList.FirstOrDefault(x => x.Name == Name);
            if (Tag == null)
            {
                await ReplyAsync($"**{Name}** doesn't exist.");
                return;
            }
            await ServerDB.TagsHandlerAsync(Context.Guild.Id, ModelEnum.TagModify, Name, Response);
            await ReplyAsync($"**{Name}** has been updated.");
        }

        [Command("Info"), Alias("About"), Summary("Shows information about a tag."), Priority(1)]
        public async Task InfoAsync(string Name)
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            var GetTag = Config.TagsList.FirstOrDefault(x => x.Name == Name);
            if (GetTag == null)
            {
                await ReplyAsync($"**{Name}** doesn't exist.");
                return;
            }
            var embed = Vmbed.Embed(VmbedColors.Cyan, Title: $"TAG INFO | {Name}",
                ThumbUrl: (await Context.Guild.GetUserAsync(Convert.ToUInt64(GetTag.Owner))).GetAvatarUrl());
            embed.AddInlineField("Name", GetTag.Name);
            embed.AddInlineField("Owner", await Context.Guild.GetUserAsync(Convert.ToUInt64(GetTag.Owner)));
            embed.AddInlineField("Uses", GetTag.Uses);
            embed.AddInlineField("Creation Date", GetTag.CreationDate);
            embed.AddInlineField("Response", GetTag.Response);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("List"), Summary("Shows a list of all tags."), Priority(1)]
        public async Task ListAsync()
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            if (!Config.TagsList.Any())
            {
                await ReplyAsync($"**{Context.Guild.Name}** doesn't have any tags.");
                return;
            }
            await ReplyAsync(string.Join(", ", Config.TagsList.Select(x => x.Name)));
        }

        [Command("User"), Summary("Shows all tags owned by you."), Priority(1)]
        public async Task UserAsync(Discord.IGuildUser User = null)
        {
            User = User ?? Context.User as Discord.IGuildUser;
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            if (!Config.TagsList.Any())
            {
                await ReplyAsync($"**{Context.Guild.Name}** doesn't have any tags.");
                return;
            }
            var UserTag = Config.TagsList.Where(x => x.Owner == User.Id.ToString());
            if (UserTag.Count() == 0)
            {
                await ReplyAsync($"{User} has no tags.");
                return;
            }
            var embed = Vmbed.Embed(VmbedColors.Gold, Title: $"{User} owns {UserTag.Count()} tags.", Description: string.Join(", ", UserTag.Select(y => y.Name)));
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Top"), Summary("Shows the top 5 tags."), Priority(1)]
        public async Task TopAsync()
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            if (!Config.TagsList.Any())
            {
                await ReplyAsync("Guild has no tags."); return;
            }
            var Top5 = Config.TagsList.OrderByDescending(x => x.Uses).Take(5);
            await ReplyAsync($"{Context.Guild.Name} Top 5 Tags:\n{string.Join(", ", Top5.Select(x => x.Name))}");
        }
    }
}
