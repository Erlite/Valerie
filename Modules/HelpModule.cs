using System;
using Discord;
using System.Linq;
using Valerie.Addons;
using Discord.Commands;
using System.Threading.Tasks;
using static Valerie.Addons.Embeds;

namespace Valerie.Modules
{
    [Name("Assistance Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class HelpModule : Base
    {
        IServiceProvider Provider { get; }
        CommandService CommandService { get; }
        HelpModule(CommandService command, IServiceProvider provider)
        {
            CommandService = command;
            Provider = provider;
        }

        [Command("Help"), Summary("Displays all of the commands.")]
        public Task CommandsAsync()
        {
            var Embed = GetEmbed(Paint.Aqua)
                .WithAuthor(x =>
                {
                    x.Name = "List of all commands";
                    x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                })
                .WithFooter(x =>
               {
                   x.Text = $"For More Information On A Command's Usage: {Context.Config.Prefix}Info CommandName";
                   x.IconUrl = Emotes.DSupporter.Url;
               });
            foreach (var Commands in CommandService.Commands.GroupBy(x => x.Module.Name).OrderBy(y => y.Key))
            {
                Embed.AddField(Commands.Key, string.Join(", ", Commands.Select(x => x.Name).Distinct()));
            }
            return ReplyAsync(string.Empty, Embed.Build());
        }

        [Command("Info"), Alias("Help"), Summary("Shows more information about a command and it's usage.")]
        public Task CommandInfoAsync([Remainder] string CommandName)
        {
            var Command = CommandService.Search(Context, CommandName).Commands?.FirstOrDefault().Command;
            if (Command == null) return ReplyAsync($"{CommandName} command doesn't exist.");
            var Embed = GetEmbed(Paint.Magenta)
                .WithAuthor(x =>
               {
                   x.Name = "Detailed Command Information";
                   x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
               })
                .AddField("Name", Command.Name, true)
                .AddField("Aliases", string.Join(", ", Command.Aliases), true)
                .AddField("Arguments", Command.Parameters.Any() ? string.Join(", ", Command.Parameters) : "No arguments.")
                .AddField("Usages", $"{Context.Config.Prefix}{Command.Name} {string.Join(" ", Command.Parameters)}")
                .AddField("Summary", Command.Summary)
                .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                .Build();
            return ReplyAsync(string.Empty, Embed);
        }
    }
}