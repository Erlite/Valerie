using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Linq;
using Rick.Attributes;
using Rick.Services;
using Rick.Classes;

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

        [Command("Help")]
        public async Task HelpAsync()
        {
            string Description = "**Admin Commands:** Kick, Ban, Mute Delete\n" + 
                "**Bot Commands:** [Group Name = Bot] Username, Nickname, Avatar, Game, Status, Latency, Prefix, Debug, Mention\n" +
                "**General Commands:** GuildInfo, RoleInfo, UserInfo, Ping, Embed, GenId, Coinflip, Afk\n" + 
                "**Github Commands:** [Group Name = Github] Userinfo\n" +
                "**Guild Commands:** [Group Name = Guild] Modchannel, SetPrefix, WelcomeMsg, Actions, ToggleJoins, ToggleLeaves, ToggleUsername, ToggleNicknames, ToggleBans, Channel, Role, ToggleKarma\n" +
                "**Karma Commands:** Karma, Rank, Top\n" + 
                "**Nsfw Commands:** Boobs, Bum, E621\n" +
                "**Owner Commands:** Serverlist, Leave, Boardcast, GetInvite, Archive, Blacklist, Whitelist, Eval, EvalList, EvalRemove, EvalAdd, Reconnect\n" +
                "**Search Commands:** Gif, Urban, Image, Lmgtfy, Imgur, Catfacts, Robohash, Leet, Cookie\n" +
                "**Tag Commands:** [Group Name = Tag] Create, Remove, Execute, Info, Modify, List\n\n For more info on command use: ?>Help CommandName";
            var embed = EmbedService.Embed(EmbedColors.Gold, $"{Context.Client.CurrentUser.Username} Commands List", Context.Client.CurrentUser.GetAvatarUrl(), null, Description, "Full Command List: https://exceptiondev.github.io/Docs");
            await ReplyAsync("", embed: embed);
        }

        [Command("Help")]
        public async Task HelpAsync(string command)
        {
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"**Command Name:** {command}\n**Error:** Not Found!\n**Reason:** Wubbalubbadubdub!");
                return;
            }
            var builder = new EmbedBuilder()
            {
                Color = new Color(179, 56, 216)
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;
                builder.Title = cmd.Name.ToUpper();
                builder.Description = $"**Aliases:** {string.Join(", ", cmd.Aliases)}\n**Parameters:** {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n"+
                    $"**Remarks:** {cmd.Remarks}\n**Summary:** {cmd.Summary}";
            }
            await ReplyAsync("", false, builder.Build());
        }
    }
}