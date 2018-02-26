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
        public Task TagAsync([Remainder] string TagName)
        {
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == TagName);
            if (!NotExists(TagName)) return Task.CompletedTask;
            Tag.Uses++;
            return SaveAsync(ModuleEnums.Server, Tag.Response);
        }

        [Command("TagCreate"), Alias("TagNew", "TagAdd"), Summary("Creates a new tag for this server.")]
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

        [Command("TagMake"), Summary("Interactive tag creation wizard.")]
        public async Task MakeAsync()
        {
            await ReplyAsync($"**Welcome to Tag Creation Wizard!**\nWhat will be the name of your tag?");
            var GetName = await ResponseWaitAsync();
            if (GetName == null || Exists(GetName.Content)) return;
            await ReplyAsync($"What is going to be the response for *{GetName.Content}*?");
            var GetContent = await ResponseWaitAsync();
            if (GetContent == null) return;
            Context.Server.Tags.Add(new TagWrapper
            {
                Uses = 1,
                Name = GetName.Content,
                Response = GetContent.Content,
                Owner = $"{Context.User.Id}",
                CreationDate = DateTime.Now
            });
            await SaveAsync(ModuleEnums.Server, $"**{GetName.Content}** tag has been created!");
        }

        [Command("TagModify"), Alias("TagChange", "TagUpdate"), Summary("Updates an existing tag")]
        public Task ModifyAsync(string Name, string Response)
        {
            if (!NotExists(Name)) return Task.CompletedTask;
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == Name);
            if (Convert.ToUInt64(Tag.Owner) != Context.User.Id)
                return ReplyAsync($"You are not the owner of tag `{Name}`.");
            Context.Server.Tags.FirstOrDefault(x => x.Name == Name).Response = Response;
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("TagDelete"), Alias("TagRemove"), Summary("Deletes a tag.")]
        public Task DeleteAsync(string Name)
        {
            if (!NotExists(Name)) return Task.CompletedTask;
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == Name);
            if (Convert.ToUInt64(Tag.Owner) != Context.User.Id || Context.Server.Admins.Contains(Context.User.Id))
                return ReplyAsync($"You are not the owner of tag `{Name}`.");
            Context.Server.Tags.Remove(Tag);
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("TagUser"), Summary("Shows all tags owned by you or a given user.")]
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

        [Command("Tags"), Summary("Shows all tags for this server.")]
        public async Task TagsAsync()
        {
            string Tags = string.Join(", ", Context.Server.Tags.Select(x => x.Name));
            if (Tags.Length > 1995)
            {
                var ListTags = Tags.ToList();
                await ReplyAsync($"Here is a list of all tags for {Context.Guild}\n{string.Join(", ", ListTags.GetRange(0, ListTags.Count / 2))}");
                await ReplyAsync(string.Join(", ", ListTags.GetRange(ListTags.Count / 2, ListTags.Count)));
            }
            await ReplyAsync(!string.IsNullOrWhiteSpace(Tags) ? $"Here is a list of all tags for {Context.Guild}\n{Tags}" : $"{Context.Guild} doesn't have any tags.");
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