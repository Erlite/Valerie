using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Valerie.Extensions;

namespace Valerie.Modules
{
    public class HelpModule : CommandBase
    {
        private CommandService CommandService;
        HelpModule(CommandService Service)
        {
            CommandService = Service;
        }

        [Command("Cmds"), Summary("Shows a list of all commands."), Alias("Help")]
        public async Task CommandsAsync()
        {
            var embed = Vmbed.Embed(VmbedColors.Pastel, Context.Client.CurrentUser.GetAvatarUrl(), "HELP | Commands");
            foreach (var Module in CommandService.Modules.Where(x => x.Name != "CommandBase"))
            {
                string ModuleName = null;
                ModuleName = Module.Name.EndsWith("Module") ? Module.Name.Remove(Module.Name.LastIndexOf("Module", StringComparison.Ordinal)) : Module.Name;
                embed.AddField(x =>
                {
                    x.Name = ModuleName;
                    x.Value = string.Join(", ", Module.Commands.Select(Cmd => Cmd.Name));
                    x.IsInline = false;
                });
            }
            await ReplyAsync("", embed: embed);
        }

        [Command("Help"), Summary("Displays information about a specific command.")]
        public async Task HelpAsync(string CommandName)
        {
            var result = CommandService.Search(Context, CommandName);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"**Command Name:** {CommandName}\n**Error:** Not Found!\n**Reason:** Wubbalubbadubdub!");
                return;
            }

            var embed = Vmbed.Embed(VmbedColors.Pastel);

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                string Aliases = null;
                if (string.IsNullOrWhiteSpace(string.Join(", ", cmd.Aliases)))
                    Aliases = "Command has no Aliases.";
                else
                    Aliases = string.Join(", ", cmd.Aliases);

                string Parameters = null;
                if (string.IsNullOrWhiteSpace(string.Join(", ", cmd.Parameters.Select(p => p.Name))))
                    Parameters = "Command requires no parameters.";
                else
                    Parameters = string.Join(", ", cmd.Parameters.Select(p => p.Name));

                embed.Title = $"COMMAND INFO | {cmd.Name}";
                embed.Description = $"**Aliases:** {Aliases}\n**Parameters:** {Parameters}\n**Summary:** {cmd.Summary}";
            }
            await ReplyAsync("", embed: embed);
        }
    }
}
