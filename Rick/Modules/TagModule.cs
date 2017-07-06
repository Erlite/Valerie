using System;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Discord;
using Discord.Commands;
using Rick.Handlers;
using Rick.Attributes;
using Rick.Models;
using Rick.Extensions;
using Rick.Enums;

namespace Rick.Modules
{
    [Group("Tag"), CheckBlacklist]
    public class TagModule : ModuleBase
    {
        [Command, Summary("Executes a tag."), Remarks("Tag TagName"), Priority(0)]
        public async Task TagAsync(string Name)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            var getTag = gldTags.FirstOrDefault(x => x.Name == Name);
            if (getTag == null)
            {
                await ReplyAsync($"Tag with name **{Name}** doesn't exist or couldn't be found!");
                return;
            }
            await ReplyAsync(getTag.Response);
            getTag.Uses++;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("Create"), Summary("Creates a tag with tag response."), Remarks("Tag Create TagName This is Tag response."), Priority(1)]
        public async Task CreateAsync(string Name, [Remainder] string response)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var MakeTags = gldConfig.TagsList;
            var Exists = MakeTags.FirstOrDefault(x => x.Name == Name);
            if (MakeTags.Contains(Exists))
            {
                await ReplyAsync("Tag already exists in the dictionary!");
                return;
            }
            var tag = new TagsModel
            {
                Name = Name,
                Response = response,
                Owner = Context.User.Id,
                CreationDate = DateTime.Now.ToString()
            };
            MakeTags.Add(tag);
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            string Description = $"**Tag Name:** {Name}\n**Tag Response:**```{response}```";
            var embed = EmbedExtension.Embed(EmbedColors.Green, $"{Context.User.Username} added new Tag!", 
                Context.User.GetAvatarUrl(), Description: Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("Remove"), Summary("Deletes a tag from the tag list."), Remarks("Tag Delete TagName"), Priority(1)]
        public async Task RemoveAsync(string Name)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            var getTag = gldTags.FirstOrDefault(x=> x.Name == Name);
            if(getTag == null)
            {
                await ReplyAsync($"Tag with name **{Name}** doesn't exist or couldn't be found!");
                return;
            }
            await RemoveTag(getTag);
            await ReplyAsync("Tag Removed :put_litter_in_its_place: ");
        }

        [Command("Info"), Summary("Shows information about a tag."), Remarks("Tag Info TagName"), Priority(1)]
        public async Task TagInfoAsync(string Name)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            var getTag = gldTags.FirstOrDefault(x => x.Name == Name);
            if (getTag == null)
            {
                await ReplyAsync($"Tag with name **{Name}** doesn't exist or couldn't be found!");
                return;
            }
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = getTag.Name;
                    x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                })
                .AddInlineField("Tag Response", getTag.Response)
                .AddInlineField("Tag Owner", await Context.Guild.GetUserAsync(getTag.Owner))
                .AddInlineField("Tag Uses", getTag.Uses)
                .AddInlineField("Creation Date", getTag.CreationDate)
                .WithColor(new Color(153, 255, 255));
            await ReplyAsync("", embed: embed);
        }

        [Command("Modify"), Summary("Modifies a tag"), Remarks("Tag Modify TagName TagResponse"), Priority(1)]
        public async Task ModifyTagAsync(GlobalEnums prop, string Name, string Response)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            var getTag = gldTags.FirstOrDefault(x => x.Name == Name);
            if (getTag == null)
            {
                await ReplyAsync($"Tag with name **{Name}** doesn't exist or couldn't be found!");
                return;
            }
            switch(prop)
            {
                case GlobalEnums.TagName:
                    getTag.Name = Name;
                    break;

                case GlobalEnums.TagResponse:
                    getTag.Response = Response;
                    break;
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync(":gears: Done");
        }

        [Command("List"), Summary("Shows a list of all tags in the guild"), Remarks("Tag List"), Priority(1)]
        public async Task ListAsync()
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            if (!gldTags.Any())
            {
                await ReplyAsync($"{Context.Guild.Name} has no tags!");
                return;
            }
            await ReplyAsync($"**Tags List:** {string.Join(", ", gldTags.Select(x => x.Name))}");
        }

        [Command("Find"), Summary("Finds tag with a specific word or letter"), Remarks("Tag Find Meme"), Priority(1)]
        public async Task FindAsync(string name)
        {
            var GldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var GetTags = GldConfig.TagsList;
            if (!GetTags.Any() || (GetTags.Where(x => !x.Name.Contains(name)) == null))
            {
                await ReplyAsync($"No tags were found matching: **{name}** OR **{Context.Guild.Name}** doesn't have any tags!");
                return;
            }
            var Sb = new StringBuilder();
            foreach(var Name in GetTags.Where(x => x.Name.Contains(name)))
            {
                Sb.Append($"{Name.Name}, ");
            }
            await ReplyAsync($"Tags matching **{name}**: \n{Sb.ToString()}");
        }

        private async Task RemoveTag(TagsModel tag)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            gldTags.Remove(tag);
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }
    }
}