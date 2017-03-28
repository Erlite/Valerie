using Discord;
using Discord.Addons.EmojiTools;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using GPB.Services.TagServices;

namespace GPB.Modules
{
    public class TagModule : ModuleBase
    {
        private readonly TagService _service;
        private readonly InteractiveService _interactive;

        public TagModule(TagService service, InteractiveService interactive)
        {
            _service = service;
            _interactive = interactive;
        }

        [Command("create", RunMode = RunMode.Async)]
        public async Task AddTagAsync()
        {
            await ReplyAsync("**What is the name of your tag?** _'cancel' to cancel_");
            var nameResponse = await _interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(180));
            if (nameResponse.Content == "cancel") return;
            string name = nameResponse.Content;

            await ReplyAsync("**List any semicolon separated aliases:** _'cancel' to cancel; 'none' for no aliases_");
            var aliasResponse = await _interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(180));
            if (aliasResponse.Content == "cancel") return;
            string[] aliases = new string[0];
            if (aliasResponse.Content != "none")
                aliases = aliasResponse.Content.Split(';');

            await ReplyAsync("**Enter the tag body:** _'cancel' to cancel_");
            var contentResponse = await _interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(180));
            if (contentResponse.Content == "cancel") return;
            string content = contentResponse.Content;

            var tag = new Tag
            {
                Name = name,
                Aliases = aliases.ToList(),
                Content = content,
                OwnerId = Context.User.Id,
            };

            var embed = new EmbedBuilder()
                .WithTitle(tag.Name)
                .WithDescription(tag.Content)
                .WithFooter(x => { x.Text = $"Aliases: {string.Join(", ", aliases)}"; })
                .WithAuthor(x => { x.IconUrl = Context.User.GetAvatarUrl(); x.Name = Context.User.Username; })
                .Build();
            await ReplyAsync("**Is This OK?**", embed: embed);
            var okResponse = await _interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(30));
            if (okResponse.Content.Trim().ToLower() == "yes")
                await _service.AddTag(tag);
            else
                await ReplyAsync($"{UnicodeEmoji.FromText(":put_litter_in_its_place:")} **Tag disposed**");
        }

        [Command("remove", RunMode = RunMode.Async)]
        public async Task RemoveTagAsync([Remainder] string name)
        {
            var tag = _service.Database.Tags.FirstOrDefault(t => t.Name == name || t.Aliases.Contains(name));
            if (tag == null)
            {
                await ReplyAsync("**No tags found.**");
                return;
            }

            var confirmation = await _interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(30));
            if (confirmation.Content.Contains("y"))
            {
                await _service.RemoveTag(tag);
                await ReplyAsync($"{UnicodeEmoji.FromText(":put_litter_in_its_place:")}");
            }
            else
                await ReplyAsync($"{UnicodeEmoji.FromText(":wheelchair:")}");
        }

        [Command("modify", RunMode = RunMode.Async)]
        public async Task ModifyTagAsync(ModifyType modify, [Remainder] string name)
        {
            var tag = _service.Database.Tags.FirstOrDefault(t => t.Name == name || t.Aliases.Contains(name));
            if (tag == null)
            {
                await ReplyAsync("**No tags found.**");
                return;
            }

            switch (modify)
            {
                case ModifyType.Name:
                    {
                        await ReplyAsync("**What is the name of your tag?** _'cancel' to cancel_");
                        var nameResponse = await _interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(180));
                        if (nameResponse.Content == "cancel") return;
                        tag.Name = nameResponse.Content;
                        break;
                    }
                case ModifyType.Alias:
                    {
                        await ReplyAsync("**List any semicolon separated aliases:** _'cancel' to cancel; 'none' for no aliases_");
                        var aliasResponse = await _interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(180));
                        if (aliasResponse.Content == "cancel") return;
                        string[] aliases = new string[0];
                        if (aliasResponse.Content != "none")
                            aliases = aliasResponse.Content.Split(';');
                        tag.Aliases = aliases.ToList();
                        break;
                    }
                case ModifyType.Body:
                    {
                        await ReplyAsync("**Enter the modified body:** _'cancel' to cancel_");
                        await ReplyAsync($"```\n{tag.Content}```");
                        var contentResponse = await _interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(180));
                        if (contentResponse.Content == "cancel") return;
                        tag.Content = contentResponse.Content;
                        break;
                    }
            }

            _service.Database.Save();
            await _service.BuildCommands();
            await ReplyAsync(UnicodeEmoji.FromText(":ok:"));
        }

        [Command("set", RunMode = RunMode.Async)]
        public async Task SetTagAsync(ModifyType modify, string name, [Remainder] string value)
        {
            var tag = _service.Database.Tags.FirstOrDefault(t => t.Name == name || t.Aliases.Contains(name));
            if (tag == null)
            {
                await ReplyAsync("**No tags found.**");
                return;
            }

            switch (modify)
            {
                case ModifyType.Name:
                    {
                        tag.Name = value;
                        break;
                    }
                case ModifyType.Alias:
                    {
                        tag.Aliases = value.Split(';').ToList();
                        break;
                    }
                case ModifyType.Body:
                    {
                        tag.Content = value;
                        break;
                    }
            }
            _service.Database.Save();
            await _service.BuildCommands();
            await ReplyAsync(UnicodeEmoji.FromText(":ok:"));
        }

        [Command("info")]
        public async Task GetTagAsync([Remainder] string name)
        {
            var tag = _service.Database.Tags.FirstOrDefault(t => t.Name == name || t.Aliases.Contains(name));
            if (tag == null)
            {
                await ReplyAsync("**No tags found.**");
                return;
            }

            var builder = new EmbedBuilder()
                .WithTitle(tag.Name);
            builder.AddField(f =>
            {
                f.Name = "Aliases";
                f.IsInline = true;
                f.Value = string.Join(", ", tag.Aliases);
            });
            builder.AddField(f =>
            {
                f.Name = "Owner";
                f.IsInline = true;
                f.Value = tag.OwnerId;
            });
            builder.AddField(f =>
            {
                f.Name = "Content";
                f.Value = tag.Content;
            });
            await ReplyAsync("", embed: builder);
        }

        [Command("list")]
        [Alias("tags")]
        public async Task ListTagsAsync()
        {
            await ReplyAsync($"**Tags:** {string.Join(", ", _service.Database.Tags.Select(t => t.Name))}");
        }

        public enum ModifyType
        {
            Name,
            Alias,
            Body
        }
    }
}