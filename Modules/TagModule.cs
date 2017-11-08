using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Valerie.Handlers.ModuleHandler;
using Models;
using System.Collections.Generic;
using System;

namespace Valerie.Modules
{
    [Group("Tag"), Name("Tag Commands")]
    public class TagModule : ValerieBase
    {
        [Command, Summary("Shows a tag with the given name."), Priority(0)]
        public Task TagAsync(string TagName)
        {
            var Tag = Context.Server.Tags.FirstOrDefault(x => x.Name == TagName);
            if (!Exists(TagName)) return Task.CompletedTask;
            Context.Server.Tags.FirstOrDefault(x => x.Name == TagName).Uses++;
            return SaveAsync(Tag.Response);
        }

        [Command("Create"), Summary("Creates a new tag for this server."), Priority(1)]
        public Task CreateAsync(string Name, string Response)
        {
            if (NotExists(Name))
                return Task.CompletedTask;
            Context.Server.Tags.Add(new TagWrapper
            {
                CreationDate = DateTime.Now,
                Name = Name,
                Owner = $"{Context.User.Id}",
                Response = Response,
                Uses = 1
            });
            return SaveAsync();
        }


        bool NotExists(string Name)
        {
            if (Context.Server.Tags.FirstOrDefault(x => x.Name == Name) == null) return false;
            _ = ReplyAsync($"Tag `{Name}` already exists.");
            return true;
        }

        bool Exists(string Name)
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