using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using Rick.Models;
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

            var gldConfig = GuildModel.GuildConfigs[Context.Guild.Id];
            var resp = gldConfig.Responses;
            if (resp.ContainsKey(name))
                await ReplyAsync("A response with the exact name already exist! :skull_crossbones:");
            resp.Add(name, content);
            GuildModel.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);

            var embed = new EmbedBuilder()
                .WithTitle(name)
                .WithDescription(content)
                .WithColor(new Color(255, 255, 255))
                .WithAuthor(x => { x.IconUrl = Context.User.GetAvatarUrl(); x.Name = $"New Tag added by {Context.User.Username}"; });
                GuildModel.GuildConfigs[Context.Guild.Id].Tags.Add(name, content);
                await ReplyAsync("", embed: embed);
        }

        [Command("Execute"), Summary("Executes a tag"), Remarks("Tag Execute TagName")]
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
                await ReplyAsync("Tag couldn't be found :skull_crossbones: ");
            }
        }

        [Command("Delete"), Summary("Delete an existing tag")]
        public async Task Delete(string TagName)
        {
            var tagExists = GuildModel.GuildConfigs[Context.Guild.Id].Tags.Remove(TagName);
            await ReplyAsync($"{TagName} has been removed!");
        }
    }
}