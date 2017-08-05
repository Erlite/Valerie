using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
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
            foreach (var Module in CommandService.Modules)
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
            var SB = new StringBuilder();

            if (!result.IsSuccess)
            {
                await ReplyAsync($"**Command Name:** {CommandName}\n**Error:** Not Found!\n**Reason:** Wubbalubbadubdub!");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Color(179, 56, 216)
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                string Aliases = null;
                if (cmd.Aliases == null || cmd.Aliases.Count <= 0)
                    Aliases = "This command has no Aliases.";
                else
                    Aliases = string.Join(", ", cmd.Aliases);

                builder.Title = cmd.Name.ToUpper();
                builder.Description = $"**Aliases:** {Aliases}\n**Parameters:** {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                    $"**Summary:** {cmd.Summary}";
            }
            await ReplyAsync("", false, builder.Build());
        }
    }
}
