using Discord;
using Discord.Commands;
using System.Threading.Tasks;


namespace GPB.Services.TagServices
{
    public class TagService
    {
        public readonly TagDb Database;

        private readonly CommandService commands;

        private ModuleInfo tagModule;

        public TagService(IDependencyMap map)
        {
            commands = map.Get<CommandService>();
            Database = TagDb.Load();
        }

        public async Task BuildCommands()
        {
            if (tagModule != null)
                await commands.RemoveModuleAsync(tagModule);

            tagModule = await commands.CreateModuleAsync("", module =>
            {
                module.Name = "Tags";

                foreach (var tag in Database.Tags)
                {
                    module.AddCommand(tag.Name, async (context, args, map) =>
                    {
                        var builder = new EmbedBuilder()
                            .WithTitle(tag.Name)
                            .WithDescription(tag.Content);
                        var user = await context.Channel.GetUserAsync(tag.OwnerId);
                        if (user != null)
                            builder.Author = new EmbedAuthorBuilder()
                                .WithIconUrl(user.GetAvatarUrl())
                                .WithName(user.Username);

                        await context.Channel.SendMessageAsync("", embed: builder.Build());
                    }, builder =>
                    {
                        builder.AddAliases(tag.Aliases.ToArray());
                    });
                }
            });
        }

        public async Task AddTag(Tag tag)
        {
            Database.Tags.Add(tag);
            Database.Save();
            await BuildCommands();
        }
        public async Task RemoveTag(Tag tag)
        {
            Database.Tags.Remove(tag);
            Database.Save();
            await BuildCommands();
        }
    }
}