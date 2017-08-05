using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Handlers.GuildHandler;
using Valerie.Handlers.GuildHandler.Enum;
using Valerie.Extensions;
using Valerie.Attributes;
using Valerie.Modules.Enums;

namespace Valerie.Modules
{
    [RequireBotPermission(GuildPermission.KickMembers | GuildPermission.BanMembers | GuildPermission.SendMessages),
        CustomPermission]
    public class AdminModule : CommandBase
    {
        [Command("Prefix"), Summary("Changes guild's prefix.")]
        public async Task PrefixAsync(string NewPrefix)
        {
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.Prefix, NewPrefix);
            await ReplyAsync($"Guild's prefix has been set to: {NewPrefix}");
        }

        [Command("RoleAdd"), Alias("RA"), Summary("Adds a role to assignable role list.")]
        public async Task RoleAddAsync(IRole Role)
        {
            if (ServerDB.GuildConfig(Context.Guild.Id).AssignableRoles.Contains(Role.Id.ToString()))
            {
                await ReplyAsync($"{Role.Name} already exists in assignable roles list.");
                return;
            }
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.RolesAdd, $"{Role.Id}");
            await ReplyAsync($"**{Role.Name}** has been added to assignable roles list.");
        }

        [Command("RoleRemove"), Alias("RR"), Summary("Removes a role from assignable role list.")]
        public async Task RoleRemoveAsync(IRole Role)
        {
            if (!ServerDB.GuildConfig(Context.Guild.Id).AssignableRoles.Contains(Role.Id.ToString()))
            {
                await ReplyAsync($"{Role.Name} doesn't exist in assignable roles list.");
                return;
            }
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.RolesRemove, $"{Role.Id}");
            await ReplyAsync($"**{Role.Name}** has been removed from assignable role list.");
        }

        [Command("WelcomeAdd"), Alias("WA"), Summary("Adds a welcome message to welcome messages list.")]
        public async Task WelcomeAddAsync([Remainder] string WelcomeMessage)
        {
            if (ServerDB.GuildConfig(Context.Guild.Id).WelcomeMessages.Contains(WelcomeMessage))
            {
                await ReplyAsync("Welcome message already exist in the Welcome Messages list.");
                return;
            }
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.WelcomeAdd, WelcomeMessage);
            await ReplyAsync("Welcome message has been added to Welcome Messages list.");
        }

        [Command("WelcomeRemove"), Alias("WR"), Summary("Removes a welcome message from welcome messages list.")]
        public async Task WelcomeRemoveAsync([Remainder] string WelcomeMessage)
        {
            if (!ServerDB.GuildConfig(Context.Guild.Id).WelcomeMessages.Contains(WelcomeMessage))
            {
                await ReplyAsync("Welcome message doesn't exist in the Welcome Messages list.");
                return;
            }
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.WelcomeRemove, WelcomeMessage);
            await ReplyAsync("Welcome message has been removed from Welcome Messages list.");
        }

        [Command("LeaveAdd"), Alias("LA"), Summary("Adds a leave message to leave messages list.")]
        public async Task LeaveAddAsync([Remainder] string LeaveMessage)
        {
            if (ServerDB.GuildConfig(Context.Guild.Id).LeaveMessages.Contains(LeaveMessage))
            {
                await ReplyAsync("Leave message already exists in the Leave Messages list.");
                return;
            }
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.LeaveAdd, LeaveMessage);
            await ReplyAsync("Leave message has been added to leave messages list.");
        }

        [Command("LeaveRemove"), Alias("LR"), Summary("Removes a leave message from leaves message list.")]
        public async Task LeaveRemoveAsync([Remainder] string LeaveMessage)
        {
            if (!ServerDB.GuildConfig(Context.Guild.Id).LeaveMessages.Contains(LeaveMessage))
            {
                await ReplyAsync("Leave message doesn't exists in the Leave Messages list.");
                return;
            }
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.LeaveRemove, LeaveMessage);
            await ReplyAsync("Leave message has been removed");
        }

        [Command("Toggle"), Summary("Enables/Disables various guild's actions. ValueType include: CB, Join, Karma, Leave, Starboard, Mod, NoAds.")]
        public async Task ToggleAsync(CommandEnums ValueType)
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            switch (ValueType)
            {
                case CommandEnums.CB:
                    if (!Config.Chatterbot.IsEnabled)
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.CBEnabled, "true");
                        await ReplyAsync("Chatterbot has been enabled.");
                    }
                    else
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.CBEnabled, "false");
                        await ReplyAsync("Chatterbot has been disabled.");
                    }
                    break;
                case CommandEnums.Join:
                    if (!Config.JoinEvent.IsEnabled)
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.JoinEnabled, "true");
                        await ReplyAsync("Join event has been enabled.");
                    }
                    else
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.JoinEnabled, "false");
                        await ReplyAsync("Join event has been disabled.");
                    }
                    break;
                case CommandEnums.Karma:
                    if (!Config.IsKarmaEnabled)
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.KarmaEnabled, "true");
                        await ReplyAsync("Karma has been enabled.");
                    }
                    else
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.KarmaEnabled, "false");
                        await ReplyAsync("Karma has been disabled.");
                    }
                    break;
                case CommandEnums.Leave:
                    if (!Config.LeaveEvent.IsEnabled)
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.LeaveEnabled, "true");
                        await ReplyAsync("Leave logging has been enabled.");
                    }
                    else
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.LeaveEnabled, "false");
                        await ReplyAsync("Leave logging has been disabled.");
                    }
                    break;
                case CommandEnums.Starboard:
                    if (!Config.Starboard.IsEnabled)
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.StarEnabled, "true");
                        await ReplyAsync("Starboard has been enabled.");
                    }
                    else
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.StarEnabled, "false");
                        await ReplyAsync("Starboard has been disabled.");
                    }
                    break;
                case CommandEnums.Mod:
                    if (!Config.ModLog.IsEnabled)
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.ModEnabled, "true");
                        await ReplyAsync("Mod log has been enabled.");
                    }
                    else
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.ModEnabled, "false");
                        await ReplyAsync("Mod log has been disabled.");
                    }
                    break;
                case CommandEnums.NoAds:
                    if (!Config.AntiAdvertisement)
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.AntiAdvertisement, "true");
                        await ReplyAsync("AntiAdvertisement has been enabled.");
                    }
                    else
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.AntiAdvertisement, "false");
                        await ReplyAsync("AntiAdvertisement has been disabled.");
                    }
                    break;
            }
        }

        [Command("Channel"), Summary("Sets channel for varios guild's actions. ValueType include: CB, Join, Karma, Leave, Starboard, Mod.")]
        public async Task ChannelAsync(CommandEnums ValueType, ITextChannel Channel)
        {
            switch (ValueType)
            {
                case CommandEnums.CB:
                    await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.CBChannel, $"{Channel.Id}");
                    await ReplyAsync($"Chatterbot channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Join:
                    await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.JoinChannel, $"{Channel.Id}");
                    await ReplyAsync($"Join log channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Leave:
                    await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.LeaveChannel, $"{Channel.Id}");
                    await ReplyAsync($"Leave log channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Starboard:
                    await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.StarChannel, $"{Channel.Id}");
                    await ReplyAsync($"Starboard channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Mod:
                    await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.ModChannel, $"{Channel.Id}");
                    await ReplyAsync($"Mod log channel has been set to: {Channel.Mention}");
                    break;
            }
        }

        [Command("Kick"), Summary("Kicks user from the guild.")]
        public async Task KickAsync(IGuildUser User, [Remainder]string Reason = "No reason provided by moderator.")
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            await User.KickAsync(Reason);
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.ModCases);
            if (Config.ModLog.IsEnabled && Config.ModLog.TextChannel != null)
            {
                var embed = Vmbed.Embed(VmbedColors.Red, ThumbUrl: User.GetAvatarUrl(), FooterText: $"Kick Date: {DateTime.Now}");
                embed.AddInlineField("User", $"{User.Username}#{User.Discriminator}\n{User.Id}");
                embed.AddInlineField("Responsible Moderator", Context.User.Username);
                embed.AddInlineField("Case No.", Config.ModCases);
                embed.AddInlineField("Case Type", "Kick");
                embed.AddInlineField("Reason", Reason);
                var msg = await (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.ModLog.TextChannel))).SendMessageAsync("", embed: embed);
            }
            else
                await ReplyAsync($"***{User.Username} got kicked*** :ok_hand:");
        }

        [Command("Ban"), Summary("Bans user from the guild.")]
        public async Task BanAsync(IGuildUser User, [Remainder] string Reason = "No reason provided by moderator.")
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            await Context.Guild.AddBanAsync(User, 7, Reason);
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.ModCases);
            if (Config.ModLog.IsEnabled && Config.ModLog.TextChannel != null)
            {
                var embed = Vmbed.Embed(VmbedColors.Red, ThumbUrl: User.GetAvatarUrl(), FooterText: $"Ban Date: {DateTime.Now}");
                embed.AddInlineField("User", $"{User.Username}#{User.Discriminator}\n{User.Id}");
                embed.AddInlineField("Responsible Moderator", Context.User.Username);
                embed.AddInlineField("Case No.", Config.ModCases);
                embed.AddInlineField("Case Type", "Ban");
                embed.AddInlineField("Reason", Reason);
                var msg = await (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.ModLog.TextChannel))).SendMessageAsync("", embed: embed);
            }
            else
                await ReplyAsync($"***{User.Username} got kicked*** :ok_hand:");
        }

        [Command("PurgeChannel"), Summary("Purges 500 messages from a channel."), Remarks("PurgeChannel #ChannelName"), Alias("PChannel", "PurgeC")]
        public async Task PurgeChannelAsync(ITextChannel Channel)
        {
            var Messages = await Channel.GetMessagesAsync(500).Flatten();
            await Channel.DeleteMessagesAsync(Messages);
        }

        [Command("Addrole"), Alias("Arole"), Summary("Adds user to the specified role.")]
        public async Task AaddroleAsync(IGuildUser User, IRole Role)
        {
            await User.AddRoleAsync(Role);
            await ReplyAsync($"{User} has been added to {Role.Name}");
        }

        [Command("Removerole"), Alias("Rrole"), Summary("Removes user from the specified role.")]
        public async Task RemoveRoleAsync(IGuildUser User, IRole Role)
        {
            await User.RemoveRoleAsync(Role);
            await ReplyAsync($"{User} has been removed from {Role.Name}");
        }

        [Command("Settings"), Summary("Displays Guild's settings.")]
        public async Task SettingsAsync()
        {
            var GConfig = ServerDB.GuildConfig(Context.Guild.Id);

            string AFKList = null;
            if (GConfig.AFKList.Count <= 0)
                AFKList = $"{Context.Guild.Name}'s AFK list is empty.";
            else
                AFKList = $"{GConfig.AFKList.Count} members in AFK List.";

            string TagList = null;
            if (GConfig.TagsList.Count <= 0)
                TagList = $"{Context.Guild.Name}'s Tag list is empty.";
            else
                TagList = $"{Context.Guild.Name} has {GConfig.TagsList.Count} tags.";

            string KarmaList = null;
            if (GConfig.KarmaList.Count <= 0)
                KarmaList = $"{Context.Guild.Name}'s Karma list is empty.";
            else
                KarmaList = $"{GConfig.KarmaList.Count} members in Karma list.";

            string Roles = null;
            if (GConfig.AssignableRoles.Count <= 0)
                Roles = $"There are no assignable roles for {Context.Guild.Name}.";
            else
                Roles = $"{GConfig.AssignableRoles.Count} assignable roles.";

            var Joins = GConfig.JoinEvent.IsEnabled ? "Enabled" : "Disabled";
            var Leaves = GConfig.LeaveEvent.IsEnabled ? "Enabled" : "Disabled";
            var Bans = GConfig.ModLog.IsEnabled ? "Enabled" : "Disabled";
            var Karma = GConfig.IsKarmaEnabled ? "Enabled" : "Disabled";
            var IsChatterBotEnabled = GConfig.Chatterbot.IsEnabled ? "Enabled" : "Disabled";
            var SBEnabled = GConfig.Starboard.IsEnabled ? "Enabled" : "Disabled";
            var AntiAd = GConfig.AntiAdvertisement ? "Enabled" : "Disabled";

            SocketGuildChannel JoinChannel;
            SocketGuildChannel LeaveChannel;
            SocketGuildChannel BanChannel;
            SocketGuildChannel ChatterBotChannel;
            SocketGuildChannel SBChannel;

            if (GConfig.JoinEvent.TextChannel != null || GConfig.LeaveEvent.TextChannel != null ||
                GConfig.ModLog.TextChannel != null || GConfig.Starboard.TextChannel != null)
            {
                JoinChannel = await Context.Guild.GetChannelAsync(Convert.ToUInt64(GConfig.JoinEvent.TextChannel)) as SocketGuildChannel;
                LeaveChannel = await Context.Guild.GetChannelAsync(Convert.ToUInt64(GConfig.LeaveEvent.TextChannel)) as SocketGuildChannel;
                BanChannel = await Context.Guild.GetChannelAsync(Convert.ToUInt64(GConfig.ModLog.TextChannel)) as SocketGuildChannel;
                ChatterBotChannel = await Context.Guild.GetChannelAsync(Convert.ToUInt64(GConfig.Chatterbot.TextChannel)) as SocketGuildChannel;
                SBChannel = await Context.Guild.GetChannelAsync(Convert.ToUInt64(GConfig.Starboard.TextChannel)) as SocketGuildChannel;
            }
            else
            {
                JoinChannel = null;
                LeaveChannel = null;
                BanChannel = null;
                ChatterBotChannel = null;
                SBChannel = null;
            }

            string Settings =
                $"**Prefix:** {GConfig.Prefix}\n" +
                $"**Welcome Message(s):**\n{string.Join("\n", GConfig.WelcomeMessages.Select(x => x)) ?? "None."}\n" +
                $"**Leave Message(s):**\n{string.Join("\n", GConfig.LeaveMessages.Select(x => x)) ?? "None."}\n" +
                $"**Mute Role:** {GConfig.MuteRoleID}\n" +
                $"**Kick/Ban Cases:** {GConfig.ModCases}\n" +
                $"**Ban Logging:** {Bans} [{BanChannel}]\n" +
                $"**Join Logging:** {Joins} [{JoinChannel}]\n" +
                $"**Leave Logging:** {Leaves} [{LeaveChannel}]\n" +
                $"**Chatter Bot:** {IsChatterBotEnabled} [{ChatterBotChannel}]\n" +
                $"**Starboard:** {SBEnabled} [{SBChannel}]\n" +
                $"**AntiAdvertisement:** {AntiAd}\n" +
                $"**Chat Karma:** {Karma}\n" +
                $"**Karma List:** {KarmaList}\n" +
                $"**AFK List:** {AFKList}\n" +
                $"**Tags List** {TagList}\n" +
                $"**Assignable Roles:** {Roles}";

            var embed = Vmbed.Embed(VmbedColors.Gold, Description: Settings, Title: $"SETTINGS | {Context.Guild}",
                ThumbUrl: Context.Guild.IconUrl ?? "https://png.icons8.com/discord/dusk/256");
            await ReplyAsync("", embed: embed);
        }

        [Command("Purge"), Summary("Better than the other command.")]
        public async Task PurgeAsync(int Count = 10, PurgeType PurgeType = PurgeType.Self, PurgeStrategy PurgeStrategy = PurgeStrategy.BulkDelete, IGuildUser User = null)
        {
            int index = 0;
            var deleteMessages = new List<IMessage>(Count);
            var messages = Context.Channel.GetMessagesAsync();
            await messages.ForEachAsync(async m =>
            {
                IEnumerable<IMessage> MessagesToDelete = null;
                if (PurgeType == PurgeType.Self)
                    MessagesToDelete = m.Where(msg => msg.Author.Id == Context.User.Id);
                else if (PurgeType == PurgeType.Bot)
                    MessagesToDelete = m.Where(msg => msg.Author.IsBot);
                else if (PurgeType == PurgeType.All)
                    MessagesToDelete = m;
                else if (PurgeType == PurgeType.User)
                    MessagesToDelete = m.Where(msg => msg.Author.Id == User.Id);

                foreach (var msg in MessagesToDelete.OrderByDescending(msg => msg.Timestamp))
                {
                    if (index >= Count) { await EndClean(deleteMessages, PurgeStrategy); return; }
                    deleteMessages.Add(msg);
                    index++;
                }
            });
        }

        internal async Task EndClean(IEnumerable<IMessage> messages, PurgeStrategy strategy)
        {
            if (strategy == PurgeStrategy.BulkDelete)
                await Context.Channel.DeleteMessagesAsync(messages);
            else if (strategy == PurgeStrategy.Manual)
            {
                foreach (var msg in messages.Cast<IUserMessage>())
                {
                    await msg.DeleteAsync();
                }
            }
        }
    }
}
