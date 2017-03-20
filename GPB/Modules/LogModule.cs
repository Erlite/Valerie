using System.Threading.Tasks;
using System.Text;
using Discord;
using Discord.Commands;
using GPB.Services;
using GPB.Handlers;

namespace GPB.Modules
{
    [Group("Log")]
    public class LogModule : ModuleBase
    {
        private LogService _log;

        [Command("ModChannel")]
        [Summary("Set the mod log channel")]
        public async Task SetModLogChannelAsync([Summary("Channel to which to set the mod log")] ITextChannel channel)
        {
            _log.ModLogChannelId = channel.Id;
            await _log.SaveConfigurationAsync();
            await ReplyAsync(":white_check_mark: ");
        }

        [Command("ServerChannel")]
        [Summary("Set the server log channel")]
        public async Task SetServerLogChannelAsync([Summary("Channel to which to set the server log")] ITextChannel channel)
        {
            _log.ServerLogChannelId = channel.Id;
            await _log.SaveConfigurationAsync();
            await ReplyAsync(":white_check_mark: ");
        }

        [Command("Actions")]
        [Summary("Returns which actions are currently logged in the server log channel")]
        public async Task ListLogActionsAsync()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Current Log Actions");
            embed.WithDescription($"**Server Log Channel:** {_log.ServerLogChannelId}\n**Server Mod Channel:** {_log.ModLogChannelId}\n"+
                $"**User Join Logging:** {_log.JoinsLogged}\n**User Leave Logging:** {_log.LeavesLogged}\n"+
                $"**Username Change Logging:** {_log.NameChangesLogged}\n **Nickname Change Logging:** {_log.NickChangesLogged}\n"+
                $"**User Ban Logging:** {_log.UserBannedLogged}\n**Latency Monitoring:** {_log.ClientLatency}");
            embed.Color = new Color(66, 244, 232);
            embed.WithFooter(x => { x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl(); x.Text = "These actions will reset upon restart."; });
            await ReplyAsync("", embed: embed);
        }

        [Command("Joins")]
        [Summary("Toggle logging users joining")]
        public async Task LogJoinsAsync()
        {
            if (!_log.JoinsLogged)
            {
                _log.EnableJoinLogging();
                await ReplyAsync(":white_check_mark:  Now logging joins.");
            }
            else
            {
                _log.DisableJoinLogging();
                await ReplyAsync(":white_check_mark:  No longer logging joins.");
            }

            await _log.SaveConfigurationAsync();
        }

        [Command("Leaves")]
        [Summary("Toggle logging users leaving")]
        public async Task LogLeavesAsync()
        {
            if (!_log.LeavesLogged)
            {
                _log.EnableLeaveLogging();
                await ReplyAsync(":white_check_mark:  Now logging leaves.");
            }
            else
            {
                _log.DisableLeaveLogging();
                await ReplyAsync(":white_check_mark:  No longer logging leaves.");
            }

            await _log.SaveConfigurationAsync();
        }

        [Command("NameChange")]
        [Summary("Toggle logging users changing usernames")]
        public async Task LogNameChangesAsync()
        {
            if (!_log.NameChangesLogged)
            {
                _log.EnableNameChangeLogging();
                await ReplyAsync(":white_check_mark:  Now logging username changes.");
            }
            else
            {
                _log.DisableNameChangeLogging();
                await ReplyAsync(":white_check_mark:  No longer logging username changes.");
            }

            await _log.SaveConfigurationAsync();
        }

        [Command("NickChange")]
        [Summary("Toggle logging users changing nicknames")]
        public async Task LogNickChangesAsync()
        {
            if (!_log.NickChangesLogged)
            {
                _log.EnableNickChangeLogging();
                await ReplyAsync(":white_check_mark:  Now logging nickname changes.");
            }
            else
            {
                _log.DisableNickChangeLogging();
                await ReplyAsync(":white_check_mark:  No longer logging nickname changes.");
            }

            await _log.SaveConfigurationAsync();
        }

        [Command("BanLog")]
        [Summary("Toggles ban logging")]
        public async Task BanLogAsync()
        {
            if (!_log.UserBannedLogged)
            {
                _log.EnableUserBannedLogging();
                await ReplyAsync(":white_check_mark:  Now logging bans.");
            }
            else
            {
                _log.DisableUserBannedLogging();
                await ReplyAsync(":white_check_mark:  No longer logging bans.");
            }
            await _log.SaveConfigurationAsync();
        }

        [Command("Latency")]
        public async Task LatencyAsync()
        {
            if (!_log.ClientLatency)
            {
                _log.EnableSmartConnection();
                await ReplyAsync("I'm now using Smart latency monitoring");
            }
            else
            {
                _log.DisableSmartConnection();
                await ReplyAsync("Smart Connection monitoring disabled!");
            }
            await _log.SaveConfigurationAsync();
        }

        public LogModule(LogService l)
        {
            _log = l;
        }
    }
}