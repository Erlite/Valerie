﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rick.Handlers;
using Discord.WebSocket;
using Rick.Enums;
using Rick.Attributes;
using System.Text;
using Rick.Extensions;

namespace Rick.Modules
{
    [RequireUserPermission(GuildPermission.ManageGuild), RequireBotPermission(GuildPermission.ManageGuild | GuildPermission.SendMessages), CheckBlacklist]
    public class GuildModule : ModuleBase
    {
        [Command("Settings"), Summary("Displays all settings for your Guild.")]
        public async Task SettingsAsync()
        {
            var WM = new StringBuilder();
            var LM = new StringBuilder();
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

            string Roles = null;
            if (GConfig.AssignableRoles.Count <= 0)
                Roles = $"There are no assignable roles for {Context.Guild.Name}.";
            else
                Roles = $"{Context.Guild.Name}'s has {GConfig.AssignableRoles.Count} assignable roles!";

            var Joins = GConfig.JoinEvent.IsEnabled ? "Enabled" : "Disabled";
            var Leaves = GConfig.LeaveEvent.IsEnabled ? "Enabled" : "Disabled";
            var Bans = GConfig.ModLog.IsEnabled ? "Enabled" : "Disabled";
            var Karma = GConfig.IsKarmaEnabled ? "Enabled" : "Disabled";
            var IsChatterBotEnabled = GConfig.Chatterbot.IsEnabled ? "Enabled" : "Disabled";
            var SBEnabled = GConfig.Starboard.IsEnabled ? "Enabled" : "Disabled";

            SocketGuildChannel JoinChannel;
            SocketGuildChannel LeaveChannel;
            SocketGuildChannel BanChannel;
            SocketGuildChannel ChatterBotChannel;
            SocketGuildChannel SBChannel;

            foreach (var Welcome in GConfig.WelcomeMessages)
            {
                WM = WM.AppendLine($":fleur_de_lis: {Welcome}");
            }
            foreach (var Leave in GConfig.LeaveMessages)
                LM = LM.AppendLine($":fleur_de_lis: {Leave}");

            if (string.IsNullOrWhiteSpace(WM.ToString()))
                WM = WM.Append("Guild has no custom welcome message(s)!");
            if (string.IsNullOrWhiteSpace(LM.ToString()))
                LM = LM.Append("Guild has no custom leave message(s)");

            if (GConfig.JoinEvent.TextChannel != 0 || GConfig.LeaveEvent.TextChannel != 0 || GConfig.ModLog.TextChannel != 0 || GConfig.Starboard.TextChannel != 0)
            {
                JoinChannel = await Context.Guild.GetChannelAsync(GConfig.JoinEvent.TextChannel) as SocketGuildChannel;
                LeaveChannel = await Context.Guild.GetChannelAsync(GConfig.LeaveEvent.TextChannel) as SocketGuildChannel;
                BanChannel = await Context.Guild.GetChannelAsync(GConfig.ModLog.TextChannel) as SocketGuildChannel;
                ChatterBotChannel = await Context.Guild.GetChannelAsync(GConfig.Chatterbot.TextChannel) as SocketGuildChannel;
                SBChannel = await Context.Guild.GetChannelAsync(GConfig.Starboard.TextChannel) as SocketGuildChannel;
            }
            else
            {
                JoinChannel = null;
                LeaveChannel = null;
                BanChannel = null;
                ChatterBotChannel = null;
                SBChannel = null;
            }


            string Description = $"**Prefix:** {GConfig.Prefix}\n" +
                $"**Welcome Message(s):**\n{WM.ToString()}\n" +
                $"**Leave Message(s):**\n{LM.ToString()}\n" +
                $"**Mute Role:** {GConfig.MuteRoleID}\n" +
                $"**Kick/Ban Cases:** {GConfig.ModCases}\n" +
                $"**Ban Logging:** {Bans} [{BanChannel}]\n" +
                $"**Join Logging:** {Joins} [{JoinChannel}]\n" +
                $"**Leave Logging:** {Leaves} [{LeaveChannel}]\n" +
                $"**Chatter Bot:** {IsChatterBotEnabled} [{ChatterBotChannel}]\n" +
                $"**Starboard:** {SBEnabled} [{SBChannel}]\n" +
                $"**AntiAdvertisement:** {GConfig.NoInvites}\n" +
                $"**Chat Karma:** {Karma}\n" +
                $"**Karma List:** {KarmaList}\n" +
                $"**AFK List:** {AFKList}\n" +
                $"**Tags List** {TagList}\n" +
                $"**Assignable Roles:** {Roles}";

            var embed = EmbedExtension.Embed(EmbedColors.Teal, Context.Guild.Name, Context.Guild.IconUrl, Description: Description, ThumbUrl: Context.Guild.IconUrl);
            await ReplyAsync("", embed: embed);
        }

        [Command("Prefix"), Summary("Sets guild prefix. Prefix can only be a character."), Remarks("Prefix .")]
        public async Task SetPrefixAsync(string prefix)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.Prefix = prefix;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync($"Guild Prefix has been set to: **{prefix}**");
        }

        [Command("WelcomeMsg"),
            Summary("Sets a welcome message for your server and adds it to the welcome list. You can also remove a welcome message " +
            "from the list by using list index."),
            Remarks("Welcome Add Heyo welcome to our server! OR Welcome Remove 2")]
        public async Task WelcomeMessageAsync([Remainder]string msg = null)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.WelcomeMessages.Add(msg);
            await ReplyAsync("Welcome message has been added to Guild's welcome messages list.");
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("LeaveMsg"),
            Summary("Sets a welcome message for your server and adds it to the welcome list. You can also remove a welcome message from the list by using list index."),
            Remarks("Welcome Add Heyo welcome to our server! OR Welcome Remove 2")]
        public async Task LeaveMessageAsync([Remainder]string msg = null)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.LeaveMessages.Add(msg);
            await ReplyAsync("Leave message has been added to Guild's leave messages list.");
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("ToggleJoins"), Alias("TJ"), Summary("Enables/Disables logging joins.")]
        public async Task ToggleJoinsAsync()
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            if (!gldConfig.JoinEvent.IsEnabled)
            {
                gldConfig.JoinEvent.IsEnabled = true;
                await ReplyAsync(":gear: Joins logging enabled!");
            }
            else
            {
                gldConfig.JoinEvent.IsEnabled = false;
                await ReplyAsync(":skull_crossbones:   No longer logging joins.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("ToggleLeaves"), Alias("TL"), Summary("Enables/Disables logging leaves.")]
        public async Task ToggleLeavesAsync()
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            if (!gldConfig.LeaveEvent.IsEnabled)
            {
                gldConfig.LeaveEvent.IsEnabled = true;
                await ReplyAsync(":gear:   Now logging leaves.");
            }
            else
            {
                gldConfig.LeaveEvent.IsEnabled = false;
                await ReplyAsync(":skull_crossbones:  No longer logging leaves.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("ToggleBans"), Alias("TB"), Summary("Enables/Disables logging admin actions such as Kick/Ban.")]
        public async Task ToggleBansAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.ModLog.IsEnabled)
            {
                gldConfig.ModLog.IsEnabled = true;
                await ReplyAsync(":gear:   Now logging bans.");
            }
            else
            {
                gldConfig.ModLog.IsEnabled = false;
                await ReplyAsync(":skull_crossbones:  No longer logging bans.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("ToggleKarma"), Alias("TK"), Summary("Enables/Disables giving random karma to users.")]
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

        [Command("ToggleChatterbot"), Alias("TCB"), Summary("Enables/Disables chatter bot.")]
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

        [Command("ToggleAntiad"), Alias("TA"), Summary("Enables/Disables NoInvites. If user posts an invite link it will be removed")]
        public async Task ToggleAntiad()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.NoInvites)
            {
                gldConfig.NoInvites = true;
                await ReplyAsync(":gear: Anti Invites has now been enabled!");
            }
            else
            {
                gldConfig.NoInvites = false;
                await ReplyAsync(":skull_crossbones: Anti Invites has been disabled!.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("ToggleStarboard"), Alias("TS"), Summary("Enables/Disables starboard")]
        public async Task ToggleStarboardAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.Starboard.IsEnabled)
            {
                gldConfig.Starboard.IsEnabled = true;
                await ReplyAsync(":gear: Starboard has now been enabled!");
            }
            else
            {
                gldConfig.Starboard.IsEnabled = false;
                await ReplyAsync(":skull_crossbones: Starboard has been disabled!.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("SetChannel"), Summary("Sets channel for events/logs. ConfigChannel Types: Mod, Join, Leave, CB, Starboard"), Remarks("SetChannel AdminChannel #Channelname")]
        public async Task SetChannelAsync(GlobalEnums ConfigChannel, SocketGuildChannel Channel)
        {
            var Config = GuildHandler.GuildConfigs[Context.Guild.Id];
            switch (ConfigChannel)
            {
                case GlobalEnums.Mod:
                    Config.ModLog.TextChannel = Channel.Id;
                    await ReplyAsync($"Mod log channel has been set to: **{Channel.Name}**");
                    break;

                case GlobalEnums.CB:
                    Config.Chatterbot.TextChannel = Channel.Id;
                    await ReplyAsync($"Chatterbot channel has been set to: **{Channel.Name}**");
                    break;

                case GlobalEnums.Join:
                    Config.JoinEvent.TextChannel = Channel.Id;
                    await ReplyAsync($"Join log channel has been set to: **{Channel.Name}**");
                    break;

                case GlobalEnums.Leave:
                    Config.LeaveEvent.TextChannel = Channel.Id;
                    await ReplyAsync($"Leave log channel has been set to: **{Channel.Name}**");
                    break;

                case GlobalEnums.Starboard:
                    Config.Starboard.TextChannel = Channel.Id;
                    await ReplyAsync($"Starboard channel has been set to: **{Channel.Name}**");
                    break;
            }

            GuildHandler.GuildConfigs[Context.Guild.Id] = Config;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("Roles"), Summary("Adds/removes a role from assignable role list."), Remarks("Roles Add @RoleName")]
        public async Task RolesAsync(GlobalEnums Action, IRole Role)
        {
            var GuildConfig = GuildHandler.GuildConfigs[Context.Guild.Id];

            switch (Action)
            {
                case GlobalEnums.Add:
                    GuildConfig.AssignableRoles.Add(Role.Name);
                    await ReplyAsync($"{Role.Name} has been added to assignable roles list.");
                    break;

                case GlobalEnums.Remove:
                    if (!GuildConfig.AssignableRoles.Contains(Role.Name))
                    {
                        await ReplyAsync($"{Role.Name} doesn't exist in Assignable roles list.!");
                        return;
                    }
                    GuildConfig.AssignableRoles.Remove(Role.Name);
                    await ReplyAsync($"{Role.Name} has been removed from assignable roles list.");
                    break;
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = GuildConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }
    }
}
