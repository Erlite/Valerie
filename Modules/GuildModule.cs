using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rick.Handlers;
using Discord.WebSocket;
using Rick.Enums;
using Rick.Attributes;
using System.Text;
using Rick.Extensions;
using System;
using Rick.Controllers;

namespace Rick.Modules
{
    [RequireUserPermission(GuildPermission.Administrator), CheckBlacklist]
    public class GuildModule : ModuleBase
    {
        [Command("SetPrefix"), Summary("SetPrefix ?"), Remarks("Sets Guild prefix")]
        public async Task SetPrefixAsync(char prefix)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.Prefix = prefix;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync($"Guild Prefix has been set to: **{prefix}**");
        }

        [Command("WelcomeMsg"), Summary("WelcomeMsg This is a welcome Msg"), Remarks("Sets welcome message for your server")]
        public async Task WelcomeMessageAsync([Remainder]string msg)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.WelcomeMessages.Add(msg);
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync($"Guild Welcome Message has been set to:\n```{msg}```");
        }

        [Command("Actions"), Summary("Normal Command"), Remarks("Shows what Actions are being logged")]
        public async Task ActionsAsync()
        {
            var SB = new StringBuilder();
            var GConfig = GuildHandler.GuildConfigs[Context.Guild.Id];

            string AFKList = null;
            if (GConfig.AFKList.Count <= 0)
                AFKList = $"{Context.Guild.Name}'s AFK list is empty.";
            else
                AFKList = $"{Context.Guild.Name}'s AFK list contains {GConfig.AFKList.Count} members.";

            string TagList = null;
            if (GConfig.TagsList.Count <= 0)
                TagList = $"{Context.Guild.Name}'s Tag list is empty.";
            else
                TagList = $"{Context.Guild.Name}'s Tag list contains {GConfig.TagsList.Count} tags.";

            string KarmaList = null;
            if (GConfig.KarmaList.Count <= 0)
                KarmaList = $"{Context.Guild.Name}'s Karma list is empty.";
            else
                KarmaList = $"{Context.Guild.Name}'s Karma list contains {GConfig.KarmaList.Count} members.";

            var Joins = GConfig.JoinEvent.IsEnabled ? "Enabled" : "Disabled";
            var Leaves = GConfig.LeaveEvent.IsEnabled ? "Enabled" : "Disabled";
            var Bans = GConfig.AdminLog.IsEnabled ? "Enabled" : "Disabled";
            var Karma = GConfig.IsKarmaEnabled ? "Enabled" : "Disabled";
            var IsChatterBotEnabled = GConfig.Chatterbot.IsEnabled ? "Enabled" : "Disabled";



            SocketGuildChannel JoinChannel;
            SocketGuildChannel LeaveChannel;
            SocketGuildChannel BanChannel;

            foreach( var Welcome in GConfig.WelcomeMessages)
            {
                SB = SB.AppendLine(Welcome);
            }

            if (string.IsNullOrWhiteSpace(SB.ToString()))
                SB = SB.AppendLine("Guild has no welcome message/s!");

            if (GConfig.JoinEvent.TextChannel != 0 || GConfig.LeaveEvent.TextChannel != 0 || GConfig.AdminLog.TextChannel != 0)
            {
                JoinChannel = await Context.Guild.GetChannelAsync(GConfig.JoinEvent.TextChannel) as SocketGuildChannel;
                LeaveChannel = await Context.Guild.GetChannelAsync(GConfig.LeaveEvent.TextChannel) as SocketGuildChannel;
                BanChannel = await Context.Guild.GetChannelAsync(GConfig.AdminLog.TextChannel) as SocketGuildChannel;
            }
            else
            {
                JoinChannel = null;
                LeaveChannel = null;
                BanChannel = null;
            }

            string Description = $"**Prefix:** {GConfig.Prefix}\n" +
                $"**Welcome Message:**\n{SB.ToString()}\n" +
                $"**Mute Role:** {GConfig.MuteRoleID}\n" +
                $"**Kick/Ban Cases:** {GConfig.AdminCases}\n" +
                $"**Ban Logging:** {Bans} [{BanChannel}]\n" +
                $"**Join Logging:** {Joins} [{JoinChannel}]\n" +
                $"**Leave Logging:** {Leaves} [{LeaveChannel}]\n" +
                $"**Chatter Bot:** {IsChatterBotEnabled}\n" +
                $"**Chat Karma:** {Karma}\n" +
                $"**Karma List:** {KarmaList}\n" +
                $"**AFK List:** {AFKList}\n" +
                $"**Tags List** {TagList}";

            var embed = EmbedExtension.Embed(EmbedColors.Teal, Context.Guild.Name, new Uri(Context.Guild.IconUrl), Description: Description, ThumbUrl: new Uri(Context.Guild.IconUrl));
            await ReplyAsync("", embed: embed);
        }

        [Command("ToggleJoins"), Summary("ToggleJoins #ChannelName"), Remarks("Toggles Join logging")]
        public async Task ToggleJoinsAsync(ITextChannel Channel = null)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            if (!gldConfig.JoinEvent.IsEnabled)
            {
                gldConfig.JoinEvent.IsEnabled = true;
                gldConfig.JoinEvent.TextChannel = Channel.Id;
                (Context.Client as DiscordSocketClient).UserJoined += Events.UserJoinedAsync;
                await ReplyAsync(":gear: Joins logging enabled!");
            }
            else
            {
                (Context.Client as DiscordSocketClient).UserJoined -= Events.UserJoinedAsync;
                gldConfig.JoinEvent.IsEnabled = false;
                await ReplyAsync(":skull_crossbones:   No longer logging joins.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("ToggleLeaves"), Summary("ToggleLeaves #ChannelName"), Remarks("Toggle Leaves logging")]
        public async Task ToggleLeavesAsync(ITextChannel Channel = null)
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            if (!gldConfig.LeaveEvent.IsEnabled)
            {
                (Context.Client as DiscordSocketClient).UserLeft += Events.UserLeftAsync;
                gldConfig.LeaveEvent.IsEnabled = true;
                gldConfig.LeaveEvent.TextChannel = Channel.Id;
                await ReplyAsync(":gear:   Now logging leaves.");
            }
            else
            {
                (Context.Client as DiscordSocketClient).UserLeft -= Events.UserLeftAsync;
                gldConfig.LeaveEvent.IsEnabled = false;
                await ReplyAsync(":skull_crossbones:  No longer logging leaves.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("ToggleBans"), Summary("Normal Command"), Remarks("Toggles ban logging")]
        public async Task ToggleBansAsync(ITextChannel Channel = null)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.AdminLog.IsEnabled)
            {
                gldConfig.AdminLog.IsEnabled = true;
                gldConfig.AdminLog.TextChannel = Channel.Id;
                await ReplyAsync(":gear:   Now logging bans.");
            }
            else
            {
                gldConfig.AdminLog.IsEnabled = false;
                await ReplyAsync(":skull_crossbones:  No longer logging bans.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("ToggleKarma"), Summary("Normal Command"), Remarks("Toggles Chat Karma")]
        public async Task ToggleKarmaAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.IsKarmaEnabled)
            {
                gldConfig.IsKarmaEnabled = true;
                await ReplyAsync(":gear: Users will now be awarded random Karma based on their chat activity!");
            }
            else
            {
                gldConfig.IsKarmaEnabled = false;
                await ReplyAsync(":skull_crossbones: Auto Karma disabled!.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("ToggleChatterbot"), Summary("Normal Command"), Remarks("Toggles Chatter Bot")]
        public async Task ToggleIsChatterBotEnabledAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.Chatterbot.IsEnabled)
            {
                gldConfig.Chatterbot.IsEnabled = true;
                await ReplyAsync(":gear: Chatter bot enabled!");
            }
            else
            {
                gldConfig.Chatterbot.IsEnabled = false;
                await ReplyAsync(":skull_crossbones: Chatter bot disabled!");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }
    }
}
