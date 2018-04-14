using System;
using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Helpers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static Valerie.Addons.Embeds;

namespace Valerie.Modules
{
    [Name("Tag Commands"), Group("Tag"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class TagModule : Base
    {
        [Command, Priority(0), Summary("Executes a tag with the given name.")]
        public Task TagAsync([Remainder] string Name)
        {
            if (!CheckTag(Name, true)) return Task.CompletedTask;
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == Name);
            Tag.Uses++;
            return ReplyAsync(Tag.Content, Document: DocumentType.Server);
        }

        [Command("Create"), Priority(1), Summary("Initiates Tag Creation wizard.")]
        public async Task CreateAsync()
        {
            await ReplyAsync($"**Welcome to Tag Creation Wizard!**\nWhat will be the name of your tag?");
            var GetName = await ResponseWaitAsync();
            if (GetName == null || CheckTag(GetName.Content, Exists: true)) return;
            await ReplyAsync($"What is going to be the response for *{GetName.Content}*?");
            var GetContent = await ResponseWaitAsync();
            if (GetContent == null) return;
            await ReplyAsync($"Do you want {GetName.Content} to be auto-responsive? (Y/N)");
            var GetToggle = await ResponseWaitAsync();
            var Check = GetToggle.Content.ToLower() == "y" ? true : false;
            Context.Server.Tags.Add(new TagWrapper
            {
                Uses = 1,
                AutoRespond = Check,
                Owner = Context.User.Id,
                Name = GetName.Content,
                Content = GetContent.Content,
                CreationDate = DateTime.Now
            });
            await ReplyAsync($"Tag `{GetName.Content}` has been created!", Document: DocumentType.Server);
        }

        [Command("Update"), Priority(1), Summary("Updates an existing tag.")]
        public Task UpdateAsync(string Name, string Response)
        {
            if (!CheckTag(Name)) return Task.CompletedTask;
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == Name);
            if (Tag.Owner != Context.User.Id) return ReplyAsync($"You are not the owner of tag `{Name}`.");
            Context.Server.Tags.FirstOrDefault(x => x.Name == Name).Content = Response;
            return ReplyAsync($"Tag `{Name}'s` content has been updated!", Document: DocumentType.Server);
        }

        [Command("Remove"), Priority(1), Summary("Deletes a tag.")]
        public Task RemoveAsync(string Name)
        {
            if (!CheckTag(Name, true)) return Task.CompletedTask;
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == Name);
            if (Tag.Owner != Context.User.Id) return ReplyAsync($"You are not the owner of tag `{Name}`.");
            Context.Server.Tags.Remove(Tag);
            return ReplyAsync($"Tag `{Name}` has been removed.", Document: DocumentType.Server);
        }

        [Command("User"), Priority(1), Summary("Shows all tags owned by you or a given user.")]
        public Task UserAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            var UserTag = Context.Server.Tags.Where(x => x.Owner == User.Id);
            if (!Context.Server.Tags.Any() || !UserTag.Any()) return ReplyAsync($"{User} doesn't have any tags.");
            return ReplyAsync($"{User} owns {UserTag.Count()} tags.\n```{string.Join(", ", UserTag.Select(x => x.Name))}```");
        }

        [Command("Info"), Priority(1), Summary("Displays information about a given tag.")]
        public async Task InfoAsync(string Name)
        {
            if (!CheckTag(Name, true)) return;
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == Name);
            var User = StringHelper.CheckUser(Context.Client, Tag.Owner);
            await ReplyAsync(string.Empty, GetEmbed(Paint.Lime)
                .WithAuthor($"Tag Information", Context.Client.CurrentUser.GetAvatarUrl())
                .AddField("Name", Tag.Name, true)
                .AddField("Owner", User, true)
                .AddField("Uses", Tag.Uses, true)
                .AddField("Created On", Tag.CreationDate, true)
                .AddField("Content", Tag.Content, false)
                .WithThumbnailUrl("https://png.icons8.com/office/80/000000/fingerprint.png")
                .Build());
        }

        bool CheckTag(string Name, bool Suggest = false, bool Exists = false)
        {
            if (Context.Server.Tags.FirstOrDefault(x => x.Name == Name) == null)
            {
                if (Suggest) _ = SuggestAsync(Name);
                return false;
            }
            if (Exists) _ = ReplyAsync($"Tag `{Name}` already exists.");
            return true;
        }

        Task SuggestAsync(string Name)
        {
            string Message = $"Tag `{Name}` doesn't exist. ";
            var Tags = Context.Server.Tags.Where(x => x.Name.ToLower().Contains(Name.ToLower()));
            if (!Context.Server.Tags.Any() || !Tags.Any()) return ReplyAsync(Message);
            Message = $"{Emotes.PepeSad} **Tag `{Name}` wasn't found. Try these perhaps?**\n {string.Join(", ", Tags.Select(x => x.Name))}";
            return ReplyAsync(Message);
        }
    }
}