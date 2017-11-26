using System;
using Discord;
using System.Linq;
using Discord.Commands;
using Valerie.JsonModels;
using Valerie.Modules.Addons;
using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules
{
    [Name("Tag Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class TagModule : ValerieBase
    {
        [Command("Tag"), Summary("Shows a tag with the given name.")]
        public Task TagAsync(string TagName)
        {
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == TagName);
            if (!NotExists(TagName)) return Task.CompletedTask;
            Tag.Uses++;
            return SaveAsync(ModuleEnums.Server, Tag.Response);
        }

        [Command("Tag Create"), Alias("Tag Make", "Tag New", "Tag Add"), Summary("Creates a new tag for this server.")]
        public Task CreateAsync(string Name, string Response)
        {
            if (Exists(Name)) return Task.CompletedTask;
            Context.Server.Tags.Add(new TagWrapper
            {
                Uses = 1,
                Name = Name,
                Response = Response,
                Owner = $"{Context.User.Id}",
                CreationDate = DateTime.Now
            });
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Tag Modify"), Alias("Tag Change", "Tag Update"), Summary("Updates an existing tag")]
        public Task ModifyAsync(string Name, string Response)
        {
            if (!NotExists(Name)) return Task.CompletedTask;
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == Name);
            if (Convert.ToUInt64(Tag.Owner) != Context.User.Id)
                return ReplyAsync($"You are not the owner of tag `{Name}`.");
            Context.Server.Tags.FirstOrDefault(x => x.Name == Name).Response = Response;
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Tag Delete"), Alias("Tag Remove"), Summary("Deletes a tag.")]
        public Task DeleteAsync(string Name)
        {
            if (!NotExists(Name)) return Task.CompletedTask;
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == Name);
            if (Convert.ToUInt64(Tag.Owner) != Context.User.Id || Context.Server.Admins.Contains(Context.User.Id))
                return ReplyAsync($"You are not the owner of tag `{Name}`.");
            Context.Server.Tags.Remove(Tag);
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Tag User"), Summary("Shows all tags owned by you or a given user.")]
        public Task UserAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            var UserTag = Context.Server.Tags.Where(x => x.Owner == $"{User.Id}");
            if (!Context.Server.Tags.Any() || !UserTag.Any()) return ReplyAsync($"{User} doesn't have any tags.");
            return ReplyAsync($"{User} owns {UserTag.Count()} tags.\n```{string.Join(", ", UserTag.Select(x => x.Name))}```");
        }

        [Command("Tag Info"), Alias("Tag About"), Summary("Displays information about a given tag.")]
        public async Task InfoAsync(string Name)
        {
            if (!NotExists(Name)) return;
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == Name);
            await ReplyAsync($"```" +
                $"Name       :  {Tag.Name}\n" +
                $"Owner      :  {(await Context.Guild.GetUserAsync(Convert.ToUInt64(Tag.Owner))).Username ?? "Unkown User."}\n" +
                $"Uses       :  {Tag.Uses}\n" +
                $"Created At :  {Tag.CreationDate}\n" +
                $"Response   :  {Tag.Response}\n```");
        }

        bool Exists(string Name)
        {
            if (Context.Server.Tags.FirstOrDefault(x => x.Name == Name) == null) return false;
            _ = ReplyAsync($"Tag `{Name}` already exists.");
            return true;
        }

        bool NotExists(string Name)
        {
            if (Context.Server.Tags.FirstOrDefault(x => x.Name == Name) != null) return true;
            _ = SuggestAsync(Name);
            return false;
        }

        Task SuggestAsync(string Name)
        {
            string Message = $"Tag `{Name}` doesn't exist. ";
            var Tags = Context.Server.Tags.Where(x => x.Name.Contains(Name));
            if (!Context.Server.Tags.Any() || !Tags.Any()) return ReplyAsync(Message);
            Message += $"Maybe try these: {string.Join("\n", Tags.Select(x => x.Name))}";
            return ReplyAsync(Message);
        }
    }
}