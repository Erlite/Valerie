using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using Rick.Handlers;
using Discord.Addons.InteractiveCommands;
using Rick.Attributes;

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
        public async Task CreateTagAsync(string name, [Remainder] string content)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var tags = gldConfig.Tags;
            if (tags.ContainsKey(name))
                await ReplyAsync("A response with the exact name already exist! :skull_crossbones:");
            else
            {
                tags.Add(name, content);
                GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
                await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);

                var embed = new EmbedBuilder()
                    .WithTitle(name)
                    .WithDescription(content)
                    .WithColor(new Color(255, 255, 255))
                    .WithAuthor(x => { x.IconUrl = Context.User.GetAvatarUrl(); x.Name = $"New Tag added by {Context.User.Username}"; });
                await ReplyAsync("", embed: embed);
            }
        }

        [Command("Execute"), Summary("Tag Execute TagName"), Remarks("Executes a tag")]        
        public async Task ExecuteTagAsync(string TagName)
        {
            string content;
            var tagExists = GuildHandler.GuildConfigs[Context.Guild.Id].Tags.TryGetValue(TagName, out content);
            if (tagExists)
            {
                await ReplyAsync(content);
            }
            else
            {
                await ReplyAsync("Tag couldn't be found :skull_crossbones: ");
            }
        }

        [Command("Delete"), Summary("Tag Delete TagName"), Remarks("Delete an existing tag")]
        public async Task Delete(string TagName)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var tags = gldConfig.Tags;
            if (!tags.ContainsKey(TagName))
                await ReplyAsync("Tag with **{TagName}** doesn't exist!");
            else
            {
                tags.Remove(TagName);
                GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
                await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
                await ReplyAsync($"Tag with **{TagName}** name has been removed!");
            }
        }
    }
}