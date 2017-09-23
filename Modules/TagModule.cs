using System;
using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using Valerie.Handlers;
using Valerie.Extensions;
using Valerie.Handlers.Server.Models;

namespace Valerie.Modules
{
    [Group("Tag"), RequireBotPermission(Discord.ChannelPermission.SendMessages)]
    public class TagModule : ValerieBase<ValerieContext>
    {    
        [Command, Summary("Executes a tag."), Priority(0)]
        public async Task Tag(string TagName)
        {
            var Tag = Context.Config.TagsList.FirstOrDefault(x => x.Name == TagName);
            if (!Context.Config.TagsList.Contains(Tag))
            {
                await ReplyAsync($"Tag with name **{TagName}** doesn't exist.");
                return;
            }
            Context.Config.TagsList.FirstOrDefault(x => x.Name == TagName).Uses += 1;
            await ReplyAsync(Tag.Response);
        }

        [Command("Create"), Summary("Creates a tag."), Priority(1)]
        public async Task CreateAsync(string Name, [Remainder]string Response)
        {
            var Exists = Context.Config.TagsList.FirstOrDefault(x => x.Name == Name);
            if (Context.Config.TagsList.Contains(Exists))
            {
                await ReplyAsync($"**{Name}** tag already exists.");
                return;
            }
            Context.Config.TagsList.Add(new TagWrapper
            {
                Name = Name,
                Response = Response,
                CreationDate = $"{DateTime.Now}",
                Owner = $"{Context.User.Id}",
                Uses = 0
            });
            await ReplyAsync($"**{Name}** tag has been created.");
        }

        [Command("Remove"), Alias("Delete"), Summary("Deletes a tag."), Priority(1)]
        public async Task RemoveAsync(string Name)
        {
            var Exists = Context.Config.TagsList.FirstOrDefault(x => x.Name == Name);
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
            Context.Config.TagsList.Remove(Exists);
            await ReplyAsync($"**{Name}** tag has been removed.");
        }

        [Command("Modify"), Summary("Changes Tag's response"), Priority(1)]
        public async Task ModifyAsync(string Name, [Remainder]string Response)
        {
            var Tag = Context.Config.TagsList.FirstOrDefault(x => x.Name == Name);
            if (Tag == null)
            {
                await ReplyAsync($"**{Name}** doesn't exist.");
                return;
            }
            if (Convert.ToUInt64(Tag.Owner) != Context.User.Id)
            {
                await ReplyAsync($"You are not the owner of **{Name}**.");
                return;
            }
            Context.Config.TagsList.FirstOrDefault(x => x.Name == Name).Response = Response;
            await ReplyAsync($"**{Name}** has been updated.");
        }

        [Command("Info"), Alias("About"), Summary("Shows information about a tag."), Priority(1)]
        public async Task InfoAsync(string Name)
        {
            var GetTag = Context.Config.TagsList.FirstOrDefault(x => x.Name == Name);
            if (GetTag == null)
            {
                await ReplyAsync($"**{Name}** doesn't exist.");
                return;
            }
            var embed = ValerieEmbed.Embed(EmbedColor.Cyan, Title: $"TAG INFO | {Name}",
                ThumbUrl: (await Context.Guild.GetUserAsync(Convert.ToUInt64(GetTag.Owner))).GetAvatarUrl());
            embed.AddField("Name", GetTag.Name, true);
            embed.AddField("Owner", await Context.Guild.GetUserAsync(Convert.ToUInt64(GetTag.Owner)), true);
            embed.AddField("Uses", GetTag.Uses, true);
            embed.AddField("Creation Date", GetTag.CreationDate, true);
            embed.AddField("Response", GetTag.Response, true);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("List"), Summary("Shows a list of all tags."), Priority(1)]
        public async Task ListAsync()
        {
            if (!Context.Config.TagsList.Any())
            {
                await ReplyAsync($"**{Context.Guild.Name}** doesn't have any tags.");
                return;
            }
            await ReplyAsync($"{Context.Guild} Tag's List:\n{string.Join(", ", Context.Config.TagsList.Select(x => x.Name))}");
        }

        [Command("User"), Summary("Shows all tags owned by you."), Priority(1)]
        public async Task UserAsync(Discord.IGuildUser User = null)
        {
            User = User ?? Context.User as Discord.IGuildUser;
            if (!Context.Config.TagsList.Any())
            {
                await ReplyAsync($"**{Context.Guild.Name}** doesn't have any tags.");
                return;
            }
            var UserTag = Context.Config.TagsList.Where(x => x.Owner == User.Id.ToString());
            if (UserTag.Count() == 0)
            {
                await ReplyAsync($"{User} has no tags.");
                return;
            }
            var embed = ValerieEmbed.Embed(EmbedColor.Gold, Title: $"{User} owns {UserTag.Count()} tags.", Description: string.Join(", ", UserTag.Select(y => y.Name)));
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Top"), Summary("Shows the top 5 tags."), Priority(1)]
        public async Task TopAsync()
        {
            if (!Context.Config.TagsList.Any())
            {
                await ReplyAsync("Guild has no tags."); return;
            }
            var Top5 = Context.Config.TagsList.OrderByDescending(x => x.Uses).Take(5);
            await ReplyAsync($"{Context.Guild.Name} Top 5 Tags:\n{string.Join(", ", Top5.Select(x => x.Name))}");
        }
    }
}
