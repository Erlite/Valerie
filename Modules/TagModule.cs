using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using Rick.Handlers;
using Discord.Addons.InteractiveCommands;
using Rick.Attributes;
using Rick.Classes;
using System.Linq;
using Rick.Services;

namespace Rick.Modules
{
    [Group("Tag"), CheckBlacklist]
    public class TagModule : ModuleBase
    {
        private readonly InteractiveService Interactive;
        public TagModule(InteractiveService Inter)
        {
            Interactive = Inter;
        }

        [Command("Create"), Summary("Tag Create TagName Tag Response"), Remarks("Creates a tag for you")]
        public async Task CreateAsync(string Name, [Remainder] string response)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var MakeTags = gldConfig.TagsList;
            var tag = new Tags
            {
                TagName = Name,
                TagResponse = response,
                OwnerId = Context.User.Id,
                CreationDate = DateTime.Now.ToString()
            };
            MakeTags.Add(tag);
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            string Description = $"**Tag Name:** {Name}\n**Tag Response:**```{response}```";
            var embed = EmbedService.Embed(EmbedColors.Green, $"{Context.User.Username} added new Tag!", Context.User.GetAvatarUrl(), null, Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("Remove"), Summary("Tag Delete TagName"), Remarks("Delete an existing tag")]
        public async Task RemoveAsync(string Name)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            var getTag = gldTags.FirstOrDefault(x=> x.TagName == Name);
            if(getTag == null)
            {
                await ReplyAsync($"Tag with name **{Name}** doesn't exist or couldn't be found!");
                return;
            }
            await RemoveTag(getTag);
            await ReplyAsync("Tag Removed :put_litter_in_its_place: ");
        }

        [Command("Execute"), Summary("Tag Execute TagName"), Remarks("Execute a tag")]
        public async Task ExecuteAsync(string Name)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            var getTag = gldTags.FirstOrDefault(x => x.TagName == Name);
            if (getTag == null)
            {
                await ReplyAsync($"Tag with name **{Name}** doesn't exist or couldn't be found!");
                return;
            }
            await ReplyAsync(getTag.TagResponse);
            getTag.TagUses++;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("Info"), Summary("Tag Info TagName"), Remarks("Returns Tag info")]
        public async Task TagInfoAsync(string Name)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            var getTag = gldTags.FirstOrDefault(x => x.TagName == Name);
            if (getTag == null)
            {
                await ReplyAsync($"Tag with name **{Name}** doesn't exist or couldn't be found!");
                return;
            }
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = getTag.TagName;
                    x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                })
                .AddInlineField("Tag Response", getTag.TagResponse)
                .AddInlineField("Tag Owner", await Context.Guild.GetUserAsync(getTag.OwnerId))
                .AddInlineField("Tag Uses", getTag.TagUses)
                .AddInlineField("Creation Date", getTag.CreationDate)
                .WithColor(new Color(153, 255, 255));
            await ReplyAsync("", embed: embed);
        }

        [Command("Modify"), Summary("Tag Modify Name/Response"), Remarks("Modifies Tag's info")]
        public async Task ModifyTagAsync(GlobalEnums prop, string Name, string Response)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            var getTag = gldTags.FirstOrDefault(x => x.TagName == Name);
            if (getTag == null)
            {
                await ReplyAsync($"Tag with name **{Name}** doesn't exist or couldn't be found!");
                return;
            }
            switch(prop)
            {
                case GlobalEnums.TagName:
                    getTag.TagName = Name;
                    break;

                case GlobalEnums.TagResponse:
                    getTag.TagResponse = Response;
                    break;
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await ReplyAsync(":gears: Done");
        }

        [Command("List"), Summary("Tag List"), Remarks("Lists all tags")]
        public async Task ListTags()
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            await ReplyAsync($"**Tags List:** {string.Join(", ", gldTags.Select(x => x.TagName))}");
        }

        public async Task RemoveTag(Tags tag)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var gldTags = gldConfig.TagsList;
            gldTags.Remove(tag);
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }
    }
}