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
                .WithAuthor("List of all commands", Context.Client.CurrentUser.GetAvatarUrl())
                .WithFooter($"For More Information On A Command's Usage: {Context.Config.Prefix}Info CommandName", Emotes.Squint.Url);
            foreach (var Commands in CommandService.Commands.GroupBy(x => x.Module.Name).OrderBy(y => y.Key))
                Embed.AddField(Commands.Key, string.Join(", ", Commands.Select(x => $"{x.Module.Group ?? null} {(x.Name.Contains("Async") ? null : x.Name)}").Distinct()));
            return ReplyAsync(string.Empty, Embed.Build());
        }


        [Command("Info"), Alias("Help"), Summary("Shows more information about a command and it's usage.")]
        public Task CommandInfoAsync([Remainder] string CommandName)
        {
            var Command = CommandService.Search(Context, CommandName).Commands?.FirstOrDefault().Command;
            if (Command == null) return ReplyAsync($"{CommandName} command doesn't exist.");
            string Name = Command.Name.Contains("Async") ? Command.Module.Group : Command.Name;
            var Embed = GetEmbed(Paint.Magenta)
                .WithAuthor("Detailed Command Information", Context.Client.CurrentUser.GetAvatarUrl())
                .AddField("Name", Name, true)
                .AddField("Aliases", string.Join(", ", Command.Aliases), true)
                .AddField("Arguments", Command.Parameters.Any() ? string.Join(", ", Command.Parameters.Select(x => $"`{x.Type.Name}` {x.Name}")) : "No arguments.")
                .AddField("Usage", $"{Context.Config.Prefix} {Name} {string.Join(" ", Command.Parameters)}")
                .AddField("Summary", Command.Summary)
                .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());
            var GetChar = Command.Parameters.Where(x => x.Type == typeof(char));
            if (GetChar.Any()) Embed.AddField($"{GetChar.FirstOrDefault()?.Name} Values", "a, r, m. a = Add, r = remove, m = Modify.");
            var GetEnum = Command.Parameters.Where(x => x.Type.IsEnum == true);
            if (GetEnum.Any()) Embed.AddField($"{GetEnum.FirstOrDefault()?.Name} Values", string.Join(", ", GetEnum?.FirstOrDefault().Type.GetEnumNames()));
            return ReplyAsync(string.Empty, Embed.Build());
        }
    }
}