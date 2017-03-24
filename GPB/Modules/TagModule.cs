using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GPB.Services.TagServices;
using Discord.Addons.InteractiveCommands;
using Discord.Addons.EmojiTools;

namespace GPB.Modules
{
    [Group("Tag")]
    public class TagModule :  ModuleBase
    {
        private readonly TagService _service;
        private readonly InteractiveService _interactive;

        public TagModule(TagService service, InteractiveService interactive)
        {
            _service = service;
            _interactive = interactive;
        }

        [Command("Create", RunMode = RunMode.Async)]
        [Priority(1000)]
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

        [Command("Remove", RunMode = RunMode.Async)]
        [Priority(1000)]
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

        [Command("Modify", RunMode = RunMode.Async)]
        [Priority(1000)]
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

        [Command("List")]
        [Alias("tags")]
        [Priority(1000)]
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