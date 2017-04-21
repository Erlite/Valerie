using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;

namespace Rick.Modules
{
    [Group("Guild")]
    public class GuildModule : ModuleBase
    {
        private GuildHandler GuildHandler;
        private LogService Log;

        public GuildModule(GuildHandler gld, LogService log)
        {
            GuildHandler = gld;
            Log = log;
        }

        [Command("ModChannel"), Summary("ModChannel #ChannelName"), Remarks("Sets the Modchannel to log bans, etc.")]
        public async Task SetModLogChannelAsync(ITextChannel channel)
        {
            GuildHandler.ModChannelID = channel.Id;
            await GuildHandler.SaveConfigurationAsync();
            await ReplyAsync($"Mod Channel has been set to **{channel.Name}**");
        }

        [Command("Logs"), Summary("Normal Command"), Remarks("Shows what Actiosn are being logged")]
        public async Task ListLogActionsAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Current Log Actions")
                .WithDescription($"**Server Mod Channel:** {GuildHandler.ModChannelID}\n" +
                $"**User Join Logging:** {GuildHandler.JoinLogs}\n**User Leave Logging:** {GuildHandler.LeaveLogs}\n" +
                $"**Username Change Logging:** {GuildHandler.NameChangesLogged}\n **Nickname Change Logging:** {GuildHandler.NickChangesLogged}\n" +
                $"**User Ban Logging:** {GuildHandler.UserBannedLogged}\n**Latency Monitoring:** {GuildHandler.ClientLatency}\n**Auto Respond:** {GuildHandler.MessageRecieve}")
                .WithColor(new Color(66, 244, 232));
            await ReplyAsync("", embed: embed);
        }

        [Command("Joins"), Summary("Normal Command"), Remarks("Toggles Join logging")]
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
                await ReplyAsync(":anger:   No longer logging joins.");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("Leaves"), Summary("Normal Command"), Remarks("Toggle Leaves logging")]
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
                await ReplyAsync(":anger:  No longer logging leaves.");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("Name"), Summary("Normal Command"), Remarks("Toggles Name change logging")]
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
                await ReplyAsync(":anger:  No longer logging username changes.");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("Nick"), Summary("Normal Command"), Remarks("Toggles Nickname changes loggig")]
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
                await ReplyAsync(":anger:   No longer logging nickname changes.");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("BanLog"), Summary("Normal Command"), Remarks("Toggles ban logging")]
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
                await ReplyAsync(":anger:  No longer logging bans.");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("Latency"), Summary("Normal Command"), Remarks("Toggles client latency. Changes client latency based on connection")]
        public async Task LatencyAsync()
        {
            if (!GuildHandler.ClientLatency)
            {
                Log.EnableLatencyMonitor();
                await ReplyAsync("I'm now monitoring client latency");
            }
            else
            {
                Log.DisableLatencyMonitor();
                await ReplyAsync("client latency monitoring disabled!");
            }
            await GuildHandler.SaveConfigurationAsync();
        }

        [Command("AutoRespond"), Summary("Normal Command"), Remarks("Autoresponds to certain words")]
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
