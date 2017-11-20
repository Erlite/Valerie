using System;
using System.Linq;
using Discord.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules
{
    [Name("Help Commands For Command Help"), RequireBotPermission(Discord.ChannelPermission.SendMessages)]
    public class HelpModule : ValerieBase
    {
        IServiceProvider Provider { get; }
        CommandService CommandService { get; }
        HelpModule(CommandService CmdParam, IServiceProvider ProviderParam)
        {
            CommandService = CmdParam;
            Provider = ProviderParam;
        }

        [Command("Commands"), Alias("Cmds", "Help"), Summary("Shows a list of all commands that you have permission to run.")]
        public async Task CommandsAsync()
        {
            var Embed = ValerieEmbed.Embed(EmbedColor.Red, Title: "List Of All Commands",
                FooterText: $"For More Information On A Command's Usage: {Context.Config.Prefix}Detail CommandName");
            var Module = (await CheckPermsAsync()).GroupBy(x => x.Module.Name).OrderBy(y => y.Key);
            foreach (var Commands in Module)
                Embed.AddField(Commands.Key, string.Join(", ", Commands.Select(x => x.Name).Distinct()));
            await ReplyAsync(string.Empty, embed: Embed.Build());
        }

        [Command("Detail"), Alias("Cmd", "Help"), Summary("Shows more information about a command and it's usage.")]
        public async Task CommandInfoAsync([Remainder] string CommandName)
        {
            var Search = CommandService.Search(Context, CommandName);
            string Description = null;
            foreach (var MatchedCommand in Search.Commands)
            {
                var Command = MatchedCommand.Command;
                string Aliases = !Command.Aliases.Any() ? "Command has no aliases." : string.Join(", ", Command.Aliases);
                string Parameters = !Command.Parameters.Any() ? "Command has no parameters." : string.Join(", ", Command.Parameters.Select(x => x.Name));
                Description += $"**{Aliases}** | **Arguments:** {Parameters}\n**Summary:** {Command.Summary}\n" +
                    $"**Usage: ** {Context.Config.Prefix}{Command.Name} {string.Join(" ", Command.Parameters.Select(x => $"`<{x.Name}>`"))}\n\n";
            }
            await ReplyAsync(Description);
        }

        async Task<IEnumerable<CommandInfo>> CheckPermsAsync()
        {
            var CanRun = new List<CommandInfo>();
            foreach (var Command in CommandService.Commands)
                if ((await Command.CheckPreconditionsAsync(Context, Provider)).IsSuccess)
                    CanRun.Add(Command);
            return CanRun;
        }
    }
}