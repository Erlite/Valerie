using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;
using Rick.Models;
using Discord.WebSocket;

namespace Rick.Modules
{
    [Group("Guild")]
    public class GuildModule : ModuleBase
    {
        private GuildModel model;
        private EventService Log;

        public GuildModule(GuildModel gld, EventService Logger)
        {
            model = gld;
            Log = Logger;
        }

        [Command("ModChannel"), Summary("ModChannel #ChannelName"), Remarks("Sets the Modchannel to log bans, etc")]
        public async Task SetModLogChannelAsync(ITextChannel channel)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildModel.GuildConfigs[Guild.Id];
            gldConfig.ModChannelID = channel.Id;
            GuildModel.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
            await ReplyAsync($"Mod Channel has been set to **{channel.Name}**");
        }

        [Command("SetPrefix"), Summary("SetPrefix ?"), Remarks("Sets Guild prefix")]
        public async Task SetPrefixAsync(string prefix)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildModel.GuildConfigs[Guild.Id];
            gldConfig.GuildPrefix = prefix;
            GuildModel.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
            await ReplyAsync($"Guild Prefix has been set to: **{prefix}**");
        }

        [Command("WelcomeMsg"), Summary("WelcomeMsg This is a welcome Msg"), Remarks("Sets welcome message for your server")]
        public async Task WelcomeMessageAsync([Remainder]string msg)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildModel.GuildConfigs[Guild.Id];
            gldConfig.WelcomeMessage = msg;
            GuildModel.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
            await ReplyAsync($"Guild Welcome Message has been set to:\n```{msg}```");
        }

        [Command("Logs"), Summary("Normal Command"), Remarks("Shows what Actions are being logged")]
        public async Task ListLogActionsAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.IconUrl = Guild.IconUrl;
                    x.Name = $"{Guild.Name} || {Guild.Owner.Username}";
                })
                .WithDescription($"**Server Mod Channel:** {GuildModel.GuildConfigs[Guild.Id].ModChannelID}\n**Guild Prefix:** {GuildModel.GuildConfigs[Guild.Id].GuildPrefix}\n"  +
                $"**Welcome Message:** {GuildModel.GuildConfigs[Guild.Id].WelcomeMessage}\n**User Join Logging:** {GuildModel.GuildConfigs[Guild.Id].JoinLogs}\n**User Leave Logging:** {GuildModel.GuildConfigs[Guild.Id].LeaveLogs}\n" +
                $"**Username Change Logging:** {GuildModel.GuildConfigs[Guild.Id].NameChangesLogged}\n **Nickname Change Logging:** {GuildModel.GuildConfigs[Guild.Id].NickChangesLogged}\n" +
                $"**User Ban Logging:** {GuildModel.GuildConfigs[Guild.Id].UserBannedLogged}\n**Auto Respond:** {GuildModel.GuildConfigs[Guild.Id].AutoRespond}")
                .WithColor(new Color(66, 244, 232));
            await ReplyAsync("", embed: embed);
        }

        [Command("Joins"), Summary("Normal Command"), Remarks("Toggles Join logging")]
        public async Task LogJoinsAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildModel.GuildConfigs[Guild.Id];
            if (!GuildModel.GuildConfigs[Guild.Id].JoinLogs)
            {
                Log.EnableJoinLogging();
                gldConfig.JoinLogs = true;
                await ReplyAsync(":white_check_mark:  Now logging joins.");
            }
            else
            {
                Log.DisableJoinLogging();
                gldConfig.JoinLogs = false;
                await ReplyAsync(":anger:   No longer logging joins.");
            }
            GuildModel.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
        }

        [Command("Leaves"), Summary("Normal Command"), Remarks("Toggle Leaves logging")]
        public async Task LogLeavesAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildModel.GuildConfigs[Guild.Id];
            if (!GuildModel.GuildConfigs[Guild.Id].LeaveLogs)
            {
                gldConfig.LeaveLogs = true;
                Log.EnableLeaveLogging();
                await ReplyAsync(":white_check_mark:  Now logging leaves.");
            }
            else
            {
                gldConfig.LeaveLogs = false;
                Log.DisableLeaveLogging();
                await ReplyAsync(":anger:  No longer logging leaves.");
            }
            GuildModel.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
        }

        [Command("Name"), Summary("Normal Command"), Remarks("Toggles Name change logging")]
        public async Task LogNameChangesAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildModel.GuildConfigs[Guild.Id];
            if (!GuildModel.GuildConfigs[Guild.Id].NameChangesLogged)
            {
                gldConfig.NameChangesLogged = true;
                Log.EnableNameChangeLogging();
                await ReplyAsync(":white_check_mark:  Now logging username changes.");
            }
            else
            {
                gldConfig.NameChangesLogged = false;
                Log.DisableNameChangeLogging();
                await ReplyAsync(":anger:  No longer logging username changes.");
            }
            GuildModel.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
        }

        [Command("Nick"), Summary("Normal Command"), Remarks("Toggles Nickname changes loggig")]
        public async Task LogNickChangesAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildModel.GuildConfigs[Guild.Id];
            if (!GuildModel.GuildConfigs[Guild.Id].NickChangesLogged)
            {
                gldConfig.NickChangesLogged = true;
                Log.EnableNickChangeLogging();
                await ReplyAsync(":white_check_mark:  Now logging nickname changes.");
            }
            else
            {
                gldConfig.NickChangesLogged = false;
                Log.DisableNickChangeLogging();
                await ReplyAsync(":anger:   No longer logging nickname changes.");
            }
            GuildModel.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
        }

        [Command("Bans"), Summary("Normal Command"), Remarks("Toggles ban logging")]
        public async Task BanLogAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildModel.GuildConfigs[Guild.Id];
            if (!GuildModel.GuildConfigs[Guild.Id].UserBannedLogged)
            {
                gldConfig.UserBannedLogged = true;
                await ReplyAsync(":white_check_mark:  Now logging bans.");
            }
            else
            {
                gldConfig.UserBannedLogged = false;
                await ReplyAsync(":anger:  No longer logging bans.");
            }
            GuildModel.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
        }

        [Command("AutoRespond"), Summary("Normal Command"), Remarks("Autoresponds to certain words")]
        public async Task AutoRespondAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildModel.GuildConfigs[Guild.Id];
            if (!GuildModel.GuildConfigs[Guild.Id].AutoRespond)
            {
                gldConfig.AutoRespond = true;
                Log.EnableAutoRespond();
                await ReplyAsync("I will now auto respond to certain messages");
            }
            else
            {
                gldConfig.AutoRespond = false;
                Log.DisableAutoRespond();
                await ReplyAsync(":anger: Auto respond have been disabled!");
            }
            GuildModel.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
        }
    }
}
