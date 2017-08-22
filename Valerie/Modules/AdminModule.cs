using System;
using System.Linq;
using System.Threading.Tasks;
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
    [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.ManageMessages),
        CustomUserPermission]
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

        [Command("Toggle"), Summary("Enables/Disables various guild's actions. ValueType include: CB, Join, Eridium, Leave, Starboard, Mod, NoAds.")]
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
                case CommandEnums.Eridium:
                    if (!Config.EridiumHandler.IsEridiumEnabled)
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.EridiumEnabled, "true");
                        await ReplyAsync("Eridium has been enabled.");
                    }
                    else
                    {
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.EridiumEnabled, "false");
                        await ReplyAsync("Eridium has been disabled.");
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

        [Command("Channel"), Summary("Sets channel for varios guild's actions. ValueType include: CB, Join, Eridium, Leave, Starboard, Mod.")]
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

        [Command("Kick"), Summary("Kicks user from the guild."), RequireBotPermission(GuildPermission.KickMembers)]
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

        [Command("Ban"), Summary("Bans user from the guild."), RequireBotPermission(GuildPermission.BanMembers)]
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

        [Command("Unban"), Summary("Unbans user from the guild"), RequireBotPermission(GuildPermission.BanMembers)]
        public async Task UnBanAsync(ulong Id)
        {
            await Context.Guild.RemoveBanAsync(Id);
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.ModCases);
            await ReplyAsync("User has been unbanned from Guild.");
        }

        [Command("PurgeChannel"), Summary("Purges 500 messages from a channel."), Alias("PC")]
        public async Task PurgeChannelAsync(ITextChannel Channel)
        {
            var Messages = await Channel.GetMessagesAsync(500).Flatten();
            await Channel.DeleteMessagesAsync(Messages);
        }

        [Command("PurgeUser"), Summary("Purges User messages from current channel."), Alias("PU")]
        public async Task PurgeUserAsync(int Amount, IGuildUser User)
        {
            var GetMessages = (await Context.Channel.GetMessagesAsync(Amount).Flatten()).Where(x => x.Author.Id == User.Id);
            if (Amount <= 100)
                await Context.Channel.DeleteMessagesAsync(GetMessages);
            else if (Amount > 100)
                foreach (var msg in GetMessages)
                    await msg.DeleteAsync();
        }

        [Command("Purge"), Summary("Deletes all messages from a channel."), Alias("Del")]
        public async Task Purge(int Amount)
        {
            var GetMessages = await Context.Channel.GetMessagesAsync(Amount).Flatten();
            if (Amount <= 100)
                await Context.Channel.DeleteMessagesAsync(GetMessages);
            else if (Amount > 100)
                foreach (var msg in GetMessages)
                    await msg.DeleteAsync();
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

            string WelcomeMessages = null;
            if (!GConfig.WelcomeMessages.Any())
                WelcomeMessages = "No Welcome Message(s).";
            else
                WelcomeMessages = string.Join("\n", GConfig.WelcomeMessages.Select(x => x));

            string LeaveMessages = null;
            if (!GConfig.LeaveMessages.Any())
                LeaveMessages = "No Leave Message(s).";
            else
                LeaveMessages = string.Join("\n", GConfig.LeaveMessages.Select(x => x));

            string AssignableRoles = null;
            if (GConfig.AssignableRoles.Count <= 0)
                AssignableRoles = $"No Assignable Role(s).";
            else
                AssignableRoles = $"{GConfig.AssignableRoles.Count} assignable AssignableRoles.";

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

            var embed = Vmbed.Embed(VmbedColors.Gold, Title: $"SETTINGS | {Context.Guild}");

            embed.AddInlineField("Prefix", GConfig.Prefix);
            embed.AddInlineField("Mute Role", GConfig.MuteRoleID);
            embed.AddInlineField("Mod Cases", GConfig.ModCases);
            embed.AddInlineField("AntiAdvertisement", GConfig.AntiAdvertisement ? "Enabled" : "Disabled");
            embed.AddInlineField("Welcome Messages", WelcomeMessages);
            embed.AddInlineField("Leave Messages", LeaveMessages);
            embed.AddInlineField("Assignable Roles", AssignableRoles);
            embed.AddInlineField("AFK List", AFKList);
            embed.AddInlineField("Tags List", TagList);

            var Eridium = GConfig.EridiumHandler;
            var EEnabled = Eridium.IsEridiumEnabled ? "Enabled" : "Disabled";
            string BRoles = null;
            if (!Eridium.BlacklistRoles.Any())
                BRoles = "No Blacklisted Roles";
            else
                BRoles = string.Join(", ", Eridium.BlacklistRoles.Select(x => Context.Guild.GetRole(UInt64.Parse(x)).Name));
            string LRoles = null;
            if (!Eridium.LevelUpRoles.Any())
                LRoles = "No LevelUp Roles";
            else
                LRoles = string.Join(", ", Eridium.LevelUpRoles.Select(x => Context.Guild.GetRole(x.Key).Name));
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Eridium Stats";
                x.Value = $"**Enabled?** {EEnabled}\n**Total Users:** {Eridium.UsersList.Count}\n**Max Level:** {Eridium.MaxRoleLevel}" +
                $"\n**Blacklisted Roles:** {BRoles}\n**LevelUp Roles:** {LRoles}";
            });

            var JEnabled = GConfig.JoinEvent.IsEnabled ? "Enabled" : "Disabled";
            var LEnabled = GConfig.LeaveEvent.IsEnabled ? "Enabled" : "Disabled";
            var CEnabled = GConfig.Chatterbot.IsEnabled ? "Enabled" : "Disabled";
            var SEnabled = GConfig.Starboard.IsEnabled ? "Enabled" : "Disabled";
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Events";
                x.Value = $"**Join:** {JEnabled} ({JoinChannel})\n**Leave:** {LEnabled} ({LeaveChannel})\n" +
                $"**Starboard:** {SEnabled} ({SBChannel})\n**Chatter Bot:** {CEnabled} ({ChatterBotChannel})";
            });
            await ReplyAsync("", embed: embed);
        }

        [Command("EridiumBlacklist"), Summary("Adds/removes a role to/from blacklisted roles"), Alias("EB")]
        public async Task BlacklistRoleAsync(Actions Action, IRole Role)
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            switch (Action)
            {
                case Actions.Add:
                    if (Config.EridiumHandler.BlacklistRoles.Contains(Role.Id.ToString()))
                    {
                        await ReplyAsync($"{Role} already exists in roles blacklist."); return;
                    }
                    await ServerDB.EridiumHandlerAsync(Context.Guild.Id, ModelEnum.EridiumBLAdd, Role.Id);
                    await ReplyAsync($"{Role} has been added."); break;

                case Actions.Remove:
                    if (!Config.EridiumHandler.BlacklistRoles.Contains(Role.Id.ToString()))
                    {
                        await ReplyAsync($"{Role} doesn't exists in roles blacklist."); return;
                    }
                    await ServerDB.EridiumHandlerAsync(Context.Guild.Id, ModelEnum.EridiumBLRemove, Role.Id);
                    await ReplyAsync($"{Role} has been removed."); break;
            }
        }

        [Command("LevelAdd"), Summary("Adds a level to level up list."), Alias("LA")]
        public async Task LevelAddAsync(IRole Role, int Level)
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            if (Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                await ReplyAsync($"{Role} already exists in level up roles."); return;
            }
            await ServerDB.EridiumHandlerAsync(Context.Guild.Id, ModelEnum.EridiumRoleAdd, Role.Id, Level);
            await ReplyAsync($"{Role} has been added.");
        }

        [Command("LevelRemove"), Summary("Removes a role from level up roles"), Alias("LR")]
        public async Task EridiumLevelAsync(IRole Role)
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            if (!Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                await ReplyAsync($"{Role} doesn't exists in level up roles."); return;
            }
            await ServerDB.EridiumHandlerAsync(Context.Guild.Id, ModelEnum.EridiumRoleRemove, Role.Id);
            await ReplyAsync($"{Role} has been removed.");
        }

        [Command("SetLevel"), Summary("Sets Max level for auto roles")]
        public async Task SetLevelAsync(int MaxLevel)
        {
            if (MaxLevel < 10)
            {
                await ReplyAsync("Max level can't be lower than 10"); return;
            }
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.EridiumMaxRoleLevel, MaxLevel.ToString());
            await ReplyAsync($"Max level has been set to: {MaxLevel}");
        }
    }
}
