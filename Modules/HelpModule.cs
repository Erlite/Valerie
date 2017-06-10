using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Linq;
using Rick.Attributes;
using Rick.Enums;
using Rick.Extensions;
using System.Text;

namespace Rick.Modules
{
    [CheckBlacklist]
    public class HelpModule : ModuleBase
    {
        private CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("Commands"), Summary("Normal Command"), Remarks("Shows all the commands!"), Alias("Cmds")]
        public async Task CommandsAsync()
        {
            string Description = 
                "**Admin Commands:** Kick, Ban, Mute Delete\n" + 
                "**Bing Commands:** BImage, BSearch\n" +
                "**Bot Commands:** [Group Name = Bot] Username, Nickname, Avatar, Game, Status, Latency, Prefix, Debug, Mention\n" +
                "**General Commands:** GuildInfo, RoleInfo, UserInfo, Ping, Ping, Embed, GenId, Coinflip, Afk, About, Encrypt, Decrypt\n" + 
                "**Github Commands:** [Group Name = Github] Userinfo\n" +
                "**Google Commands:** Google, GImage, Youtube\n" +
                "**Guild Commands:** [Group Name = Guild] Modchannel, SetPrefix, WelcomeMsg, Actions, ToggleJoins, ToggleLeaves, ToggleUsername, ToggleNicknames, ToggleBans, Channel, Role, ToggleKarma\n" +
                "**Karma Commands:** Karma, Rank, Top\n" + 
                "**Nsfw Commands:** Boobs, Bum, E621\n" +
                "**Owner Commands:** Serverlist, Leave, Boardcast, GetInvite, Archive, Blacklist, Whitelist, Eval, EvalList, EvalRemove, EvalAdd, Reconnect\n" +
                "**Search Commands:** Gif, Urban, Lmgtfy, Imgur, Catfacts, Robohash, Leet, Cookie\n" +
                "**Tag Commands:** [Group Name = Tag] Create, Remove, Execute, Info, Modify, List";
            var embed = EmbedExtension.Embed(EmbedColors.Gold, $"{Context.Client.CurrentUser.Username} Commands List", Context.Client.CurrentUser.GetAvatarUrl(), Description: Description, FooterText: "For more info on command use: ?>Help CommandName");
            await ReplyAsync("", embed: embed);
        }

        [Command("Help")]
        public async Task HelpAsync(string CommandName)
        {
            var result = _service.Search(Context, CommandName);
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

                string Remarks = null;
                if (cmd.Remarks == null || string.IsNullOrWhiteSpace(cmd.Remarks))
                    Remarks = "This command doesn't require any parameters.";
                else
                    Remarks = cmd.Remarks;

                string Aliases = null;
                if (cmd.Aliases == null || cmd.Aliases.Count <= 0)
                    Aliases = "This command has no Aliases.";
                else
                    Aliases = string.Join(", ", cmd.Aliases);

                builder.Title = cmd.Name.ToUpper();
                builder.Description = $"**Aliases:** {Aliases}\n**Parameters:** {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n"+
                    $"**Remarks:** {Remarks}\n**Summary:** {cmd.Summary}";
            }
            await ReplyAsync("", false, builder.Build());
        }
    }
}