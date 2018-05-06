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

        [Command("Create"), Priority(10), Summary("Initiates Tag Creation wizard.")]
        public async Task CreateAsync()
        {
            await ReplyAsync($"**Welcome to Tag Creation Wizard!**\n{Emotes.Next}What will be the name of your tag? (Type c to cancel)");
            var Name = CheckResponse(await WaitForReaponseAsync(), "Tag creation", true);
            if (Name.Item1 == false) { await ReplyAsync(Name.Item2); return; }
            await ReplyAsync($"{Emotes.Next} What is going to be the response for *{Name.Item2}*?");
            var Content = CheckResponse(await WaitForReaponseAsync(), "Tag Creation");
            if (Content.Item1 == false) { await ReplyAsync(Content.Item2); return; }
            await ReplyAsync($"{Emotes.Next} Do you want {Name.Item2} to be auto-responsive? (Y/N)");
            var Toggle = CheckResponse(await WaitForReaponseAsync(), "Tag Creation");
            var Check = Toggle.Item2.ToLower() == "y" ? true : false;
            Context.Server.Tags.Add(new TagWrapper
            {
                Uses = 1,
                Name = Name.Item2,
                AutoRespond = Check,
                Owner = Context.User.Id,
                Content = Content.Item2,
                CreationDate = DateTime.Now
            });
            await ReplyAsync($"Tag `{Name.Item2}` has been created!", Document: DocumentType.Server);
        }

        [Command("Update"), Priority(10), Summary("Updates an existing tag.")]
        public async Task UpdateAsync(string TagName)
        {
            if (!CheckTag(TagName)) return;
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == TagName);
            if (Tag.Owner != Context.User.Id) { await ReplyAsync($"You are not the owner of tag `{TagName}`."); return; }
            await ReplyAsync($"What would you like to modify (Use number to specifiy)?\n" +
                $"{Emotes.Next} Change  `{TagName}` Name\n{Emotes.Next} Change `{TagName}` Content\n{Emotes.Next} Enable/Disable Auto `{TagName}` Execution");
            var Options = CheckResponse(await WaitForReaponseAsync(), "Tag Modification");
            if (!Options.Item1) { await ReplyAsync(Options.Item2); return; }
            (string, DocumentType) Message;
            switch (Convert.ToInt32(Options.Item2))
            {
                case 1:
                    await ReplyAsync($"What would you like the new name to be?");
                    var NewName = CheckResponse(await WaitForReaponseAsync(), "Tag Modification", true);
                    if (!NewName.Item1) { await ReplyAsync(NewName.Item2); return; }
                    Tag.Name = NewName.Item2;
                    Message = ($"{TagName}  {Emotes.Next} {NewName}", DocumentType.Server);
                    break;
                case 2:
                    await ReplyAsync($"What would you like new content to be?");
                    var NewContent = CheckResponse(await WaitForReaponseAsync(), "Tag Modification");
                    if (!NewContent.Item1) { await ReplyAsync(NewContent.Item2); return; }
                    Tag.Content = NewContent.Item2;
                    Message = ($"{TagName}'s content has been updated {Emotes.ThumbUp}", DocumentType.Server);
                    break;
                case 3:
                    await ReplyAsync($"Do you want {TagName} to be auto-responsive? (Y/N)");
                    var NewExe = CheckResponse(await WaitForReaponseAsync(), "Tag Modification");
                    if (!NewExe.Item1) { await ReplyAsync(NewExe.Item2); return; }
                    Tag.AutoRespond = NewExe.Item2.ToLower() == "y" ? true : false;
                    Message = ($"{TagName} auto-responsiveness has been {(NewExe.Item2.ToLower() == "y" ? "enabled" : "disabled")}.", DocumentType.Server);
                    break;
                default: Message = ($"Invalid choice {Emotes.ThumbDown}.", DocumentType.None); break;
            }
            await ReplyAsync(Message.Item1, Document: Message.Item2);
        }

        [Command("Remove"), Priority(1), Summary("Deletes a tag.")]
        public Task RemoveAsync(string Name)
        {
            if (!CheckTag(Name, true)) return Task.CompletedTask;
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == Name);
            if (Tag.Owner != Context.User.Id) return ReplyAsync($"You are not the owner of tag `{Name}` {Emotes.Shout}");
            Context.Server.Tags.Remove(Tag);
            return ReplyAsync($"Tag `{Name}` has been removed {Emotes.ThumbUp}", Document: DocumentType.Server);
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
            if (Exists) _
                    = ReplyAsync($"Tag `{Name}` already exists.");
            return true;
        }

        Task SuggestAsync(string Name)
        {
            string Message = $"Tag `{Name}` doesn't exist. ";
            var Tags = Context.Server.Tags.Where(x => x.Name.ToLower().Contains(Name.ToLower()));
            if (!Context.Server.Tags.Any() || !Tags.Any()) return ReplyAsync(Message);
            Message = $"{Emotes.Shout} **Tag `{Name}` wasn't found. Try these perhaps?**\n {string.Join(", ", Tags.Select(x => x.Name))}";
            return ReplyAsync(Message);
        }

        (bool, string) CheckResponse(SocketMessage Message, string OperationName = null,
            bool IsName = false)
        {
            var ReservedNames = new[] { "help", "about", "tag", "delete", "remove", "delete", "info", "modify", "update", "user" };
            if (Message.Content.ToLower() == "c") return (false, $"{Emotes.Cross} {OperationName} has been cancelled.");
            else if (Message.Content == null) return (false, $"{OperationName} timed out.");
            if (IsName && ReservedNames.Any(x => Message.Content.ToLower().StartsWith(x))) return (false, $"Tag name is reserved. Try another name? {Emotes.Squint}");
            return (true, Message.Content);
        }
    }
}