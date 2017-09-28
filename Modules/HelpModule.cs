using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Valerie.Handlers;
using Valerie.Extensions;

namespace Valerie.Modules
{
    [RequireBotPermission(Discord.ChannelPermission.SendMessages)]
    public class HelpModule : ValerieBase<ValerieContext>
    {
        private CommandService CommandService;
        HelpModule(CommandService Service)
        {
            CommandService = Service;
        }

        [Command("Cmds"), Summary("Shows a list of all commands."), Alias("Help")]
        public async Task HelpAsync()
        {
            var embed = ValerieEmbed.Embed(EmbedColor.Pastel, Context.Client.CurrentUser.GetAvatarUrl(), "HELP | Commands",
                FooterText: $"For more information on command use {Context.ValerieConfig.Prefix}CommandName");
            string BotCommands = null;
            foreach (var Bot in CommandService.Modules.Where(x => x.Name == "Bot"))
                BotCommands = string.Join(", ", Bot.Commands.Select(x => $"Bot {x.Name}"));
            embed.AddField("Bot Commands", BotCommands);

            foreach (var Module in CommandService.Modules.Where(x => x.Name != "Tag" && x.Name != "Bot" && x.Name != "ValerieBase`1"))
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
            var Search = CommandService.Search(Context, CommandName);
            var embed = ValerieEmbed.Embed(EmbedColor.Pastel, ThumbUrl: "https://png.icons8.com/question/dusk/256");
            foreach (var MatchedCommand in Search.Commands)
            {
                var Command = MatchedCommand.Command;
                string Aliases = !Command.Aliases.Any() ? "Command has no aliases." : string.Join(", ", Command.Aliases);
                string Parameters = !Command.Parameters.Any() ? "Command has no parameters." : string.Join(", ", Command.Parameters);
                string Permissions = !Command.Preconditions.Any() ? "Command requires no permissions." : string.Join(", ", Command.Preconditions);
                embed.Title = $"COMMAND INFO | {Command.Name}";
                embed.AddField("Aliases", Aliases, true);
                embed.AddField("Arguments", Parameters, true);
                embed.AddField("Permissions", Permissions, true);
                embed.AddField("Usage", $"{Context.ValerieConfig.Prefix}{string.Join(" ", Command.Parameters.Select(x => $"`<{x.Name}>`"))}", true);
                embed.AddField("Summary", Command.Summary);
            }
            await ReplyAsync("", embed: embed.Build());
        }
    }
}
