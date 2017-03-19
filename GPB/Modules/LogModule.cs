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
        public async Task SetModLogChannel([Summary("Channel to which to set the mod log")] ITextChannel channel)
        {
            _log.ModLogChannelId = channel.Id;
            await _log.SaveConfigurationAsync();
            await ReplyAsync(":white_check_mark: ");
        }

        [Command("ServerChannel")]
        [Summary("Set the server log channel")]
        public async Task SetServerLogChannel([Summary("Channel to which to set the server log")] ITextChannel channel)
        {
            _log.ServerLogChannelId = channel.Id;
            await _log.SaveConfigurationAsync();
            await ReplyAsync(":white_check_mark: ");
        }

        [Command("Actions")]
        [Summary("Returns which actions are currently logged in the server log channel")]
        public async Task ListLogActions()
        {
            var response = new StringBuilder("**CURRENTLY LOGGED ACTIONS:**\n");
            response.AppendLine($"User joins: {_log.JoinsLogged}");
            response.AppendLine($"User leaves: {_log.LeavesLogged}");
            response.AppendLine($"Username changes: {_log.NameChangesLogged}");
            response.AppendLine($"Nickname changes: {_log.NickChangesLogged}");
            response.AppendLine($"User Ban Logs: {_log.UserBannedLogged}");
            await ReplyAsync(response.ToString());
        }

        [Command("Joins")]
        [Summary("Toggle logging users joining")]
        public async Task LogJoins()
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
        public async Task LogLeaves()
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
        public async Task LogNameChanges()
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
        public async Task LogNickChanges()
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

        public LogModule(LogService l)
        {
            _log = l;
        }
    }
}