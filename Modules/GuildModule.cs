using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;

namespace Rick.Modules
{
    public class GuildModule : ModuleBase
    {
        private GuildHandler GuildHandler;
        private LogService Log;

        public GuildModule(GuildHandler gld, LogService log)
        {
            GuildHandler = gld;
            Log = log;
        }

        [Command("ModChannel"), Summary("Set the mod log channel")]
        public async Task SetModLogChannelAsync([Summary("Channel to which to set the mod log")] ITextChannel channel)
        {
            GuildHandler.ModChannelID = channel.Id;
            await GuildHandler.SaveConfigurationAsync();
            await ReplyAsync(":white_check_mark: ");
        }

        [Command("Actions")]
        [Summary("Returns which actions are currently logged in the server log channel")]
        public async Task ListLogActionsAsync()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Current Log Actions");
            embed.WithDescription($"**Server Mod Channel:** {GuildHandler.ModChannelID}\n" +
                $"**User Join Logging:** {GuildHandler.JoinLogs}\n**User Leave Logging:** {GuildHandler.LeaveLogs}\n" +
                $"**Username Change Logging:** {GuildHandler.NameChangesLogged}\n **Nickname Change Logging:** {GuildHandler.NickChangesLogged}\n" +
                $"**User Ban Logging:** {GuildHandler.UserBannedLogged}\n**Latency Monitoring:** {GuildHandler.ClientLatency}\n**Auto Respond:** {GuildHandler.MessageRecieve}");
            embed.Color = new Color(66, 244, 232);
            await ReplyAsync("", embed: embed);
        }

        [Command("Joins"), Summary("Toggle logging users joining")]
        public async Task LogJoinsAsync()
        {
            if (!GuildHandler.JoinLogs)
            {
                Log.EnableJoinLogging();
                await ReplyAsync(":white_check_mark:  Now logging joins.");
            }
            else
            {
                Log.DisableJoinLogging();
                await ReplyAsync(":white_check_mark:  No longer logging joins.");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("Leaves"), Summary("Toggle logging users leaving")]
        public async Task LogLeavesAsync()
        {
            if (!GuildHandler.LeaveLogs)
            {
                Log.EnableLeaveLogging();
                await ReplyAsync(":white_check_mark:  Now logging leaves.");
            }
            else
            {
                Log.DisableLeaveLogging();
                await ReplyAsync(":white_check_mark:  No longer logging leaves.");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("NameChange"), Summary("Toggle logging users changing usernames")]
        public async Task LogNameChangesAsync()
        {
            if (!GuildHandler.NameChangesLogged)
            {
                Log.EnableNameChangeLogging();
                await ReplyAsync(":white_check_mark:  Now logging username changes.");
            }
            else
            {
                Log.DisableNameChangeLogging();
                await ReplyAsync(":white_check_mark:  No longer logging username changes.");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("NickChange"), Summary("Toggle logging users changing nicknames")]
        public async Task LogNickChangesAsync()
        {
            if (!GuildHandler.NickChangesLogged)
            {
                Log.EnableNickChangeLogging();
                await ReplyAsync(":white_check_mark:  Now logging nickname changes.");
            }
            else
            {
                Log.DisableNickChangeLogging();
                await ReplyAsync(":white_check_mark:  No longer logging nickname changes.");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("BanLog"), Summary("Toggles ban logging")]
        public async Task BanLogAsync()
        {
            if (!GuildHandler.UserBannedLogged)
            {
                Log.EnableUserBannedLogging();
                await ReplyAsync(":white_check_mark:  Now logging bans.");
            }
            else
            {
                Log.DisableUserBannedLogging();
                await ReplyAsync(":white_check_mark:  No longer logging bans.");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("Latency")]
        public async Task LatencyAsync()
        {
            if (!GuildHandler.ClientLatency)
            {
                Log.EnableLatencyMonitor();
                await ReplyAsync("I'm now using Smart latency monitoring");
            }
            else
            {
                Log.DisableLatencyMonitor();
                await ReplyAsync("Smart Connection monitoring disabled!");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("AutoRespond")]
        public async Task AutoRespondAsync()
        {
            if (!GuildHandler.MessageRecieve)
            {
                Log.EnableMessageRecieve();
                await ReplyAsync("I will now auto respond to certain messages");
            }
            else
            {
                Log.DisableMessageRecieve();
                await ReplyAsync("Auto respond have been disabled!");
            }
            await GuildHandler.SaveConfigurationAsync();
        }
    }
}
