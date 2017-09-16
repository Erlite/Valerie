using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Valerie.Extensions;

namespace Valerie.Modules
{
    [RequireBotPermission(Discord.ChannelPermission.SendMessages)]
    public class HelpModule : ValerieContext
    {
        private CommandService CommandService;
        HelpModule(CommandService Service)
        {
            CommandService = Service;
        }

        [Command("Cmds"), Summary("Shows a list of all commands."), Alias("Help")]
        public async Task HelpAsync()
        {
            var embed = ValerieEmbed.Embed(VmbedColors.Pastel, Context.Client.CurrentUser.GetAvatarUrl(), "HELP | Commands");
            string AdminCommands = null;
            string BotCommands = null;
            foreach (var Admin in CommandService.Modules.Where(x => x.Name == "AdminModule"))
                AdminCommands += string.Join(", ", Admin.Commands.Select(x => x.Name));
            foreach (var Set in CommandService.Modules.Where(x => x.Name == "Set"))
                AdminCommands += $", {string.Join(", ", Set.Commands.Select(x => $"Set {x.Name}"))}";
            foreach (var Bot in CommandService.Modules.Where(x => x.Name == "Bot"))
                BotCommands = string.Join(", ", Bot.Commands.Select(x => $"Bot {x.Name}"));

            embed.AddField("Admin Commands", AdminCommands);
            embed.AddField("Bot Commands", BotCommands);

            foreach (var Module in CommandService.Modules.Where(x => x.Name != "ValerieContext" && x.Name != "AdminModule"
            && x.Name != "Set" && x.Name != "Tag" && x.Name != "Bot"))
            {
                string ModuleName = null;
                ModuleName = Module.Name.EndsWith("Module") ? Module.Name.Remove(Module.Name.LastIndexOf("Module", StringComparison.Ordinal)) : Module.Name;
                embed.AddField($"{ModuleName} Commands", string.Join(", ", Module.Commands.Select(Cmd => Cmd.Name)));
            }
            string TagCommands = "Tag, ";
            foreach (var Tag in CommandService.Modules.Where(x => x.Name == "Tag"))
            {
                var Commands = Tag.Commands.Where(x => x.Name != "Tag");
                TagCommands += string.Join(", ", Commands.Select(x => $"Tag {x.Name}"));
            }
            embed.AddField("Tag Commands", TagCommands);
            await ReplyAsync("", embed: embed.Build());
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

            var embed = ValerieEmbed.Embed(VmbedColors.Pastel);

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
            await ReplyAsync("", embed: embed.Build());
        }
    }
}
