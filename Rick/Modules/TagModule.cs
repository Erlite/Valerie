﻿using System;
using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using Rick.Handlers.GuildHandler;
using Rick.Handlers.GuildHandler.Enum;
using Rick.Extensions;

namespace Rick.Modules
{
    [Group("Tag"), RequireBotPermission(Discord.GuildPermission.SendMessages)]
    public class TagModule : ModuleBase
    {
        [Command, Summary("Executes a tag."), Priority(0)]
        public async Task TagAsync(string TagName)
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
            await ServerDB.TagsHandlerAsync(Context.Guild.Id, ModelEnum.TagAdd, Name, Response, Context.User.Id, DateTime.Now.ToString());
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
            if (Exists.Owner != Context.User.Id)
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

        [Command("Info"), Summary("Shows information about a tag."), Priority(1)]
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
                ThumbUrl: (await Context.Guild.GetUserAsync(GetTag.Owner)).GetAvatarUrl());
            embed.AddInlineField("Name", GetTag.Name);
            embed.AddInlineField("Owner", await Context.Guild.GetUserAsync(GetTag.Owner));
            embed.AddInlineField("Uses", GetTag.Uses);
            embed.AddInlineField("Creation Date", GetTag.CreationDate);
            embed.AddInlineField("Response", GetTag.Response);
            await ReplyAsync("", embed: embed);
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
            await ReplyAsync(string.Join(",", Config.TagsList.Select(x => x.Name)));
        }
    }
}
