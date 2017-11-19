using System;
using System.Linq;
using Discord.Commands;
using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler;
using System.Collections.Generic;

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