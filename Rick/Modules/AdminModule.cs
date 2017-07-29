using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rick.Handlers.GuildHandler;
using Rick.Handlers.GuildHandler.Enum;
using Rick.Extensions;

namespace Rick.Modules
{
    [RequireBotPermission(GuildPermission.KickMembers | GuildPermission.BanMembers | GuildPermission.SendMessages),
        RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : ModuleBase
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
            if (ServerDB.GuildConfig(Context.Guild.Id).AssignableRoles.Contains(Role.Id))
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
            if (!ServerDB.GuildConfig(Context.Guild.Id).AssignableRoles.Contains(Role.Id))
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
                        await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.JoinEnabled, "true");
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
            if (Config.ModLog.IsEnabled && Config.ModLog.TextChannel != 0)
            {
                var embed = Vmbed.Embed(VmbedColors.Red, ThumbUrl: User.GetAvatarUrl(), FooterText: $"Kick Date: {DateTime.Now}");
                embed.AddInlineField("User", $"{User.Username}#{User.Discriminator}\n{User.Id}");
                embed.AddInlineField("Responsible Moderator", Context.User.Username);
                embed.AddInlineField("Case No.", Config.ModCases);
                embed.AddInlineField("Case Type", "Kick");
                embed.AddInlineField("Reason", Reason);
                var msg = await (await Context.Guild.GetTextChannelAsync(Config.ModLog.TextChannel)).SendMessageAsync("", embed: embed);
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
            if (Config.ModLog.IsEnabled && Config.ModLog.TextChannel != 0)
            {
                var embed = Vmbed.Embed(VmbedColors.Red, ThumbUrl: User.GetAvatarUrl(), FooterText: $"Ban Date: {DateTime.Now}");
                embed.AddInlineField("User", $"{User.Username}#{User.Discriminator}\n{User.Id}");
                embed.AddInlineField("Responsible Moderator", Context.User.Username);
                embed.AddInlineField("Case No.", Config.ModCases);
                embed.AddInlineField("Case Type", "Ban");
                embed.AddInlineField("Reason", Reason);
                var msg = await (await Context.Guild.GetTextChannelAsync(Config.ModLog.TextChannel)).SendMessageAsync("", embed: embed);
            }
            else
                await ReplyAsync($"***{User.Username} got kicked*** :ok_hand:");
        }

        [Command("Delete"), Alias("Del"), Summary("Deletes X amount of messages. Messages can't be old than 2 weeks.")]
        public async Task DeleteAsync(int MessageAmount)
        {
            if (MessageAmount <= 0)
            {
                await ReplyAsync("The amount cannot be lower than or equal to 0!");
                return;
            }
            if (MessageAmount > 100)
            {
                await ReplyAsync("Amount can't be higher than 100!");
                return;
            }
            var messageList = await Context.Channel.GetMessagesAsync(MessageAmount).Flatten();
            await Context.Channel.DeleteMessagesAsync(messageList);
        }

        [Command("PurgeUser"), Summary("Purges 500 messages by the specified user."), Remarks("PurgeUser @Username"), Alias("PUser", "PurgeU")]
        public async Task PurgeUserAsync(IGuildUser User)
        {
            try
            {
                var Guild = Context.Guild as SocketGuild;
                foreach (var Channel in Guild.TextChannels)
                {
                    var Chn = Channel as ITextChannel;
                    var Messages = (await Chn.GetMessagesAsync(200).Flatten()).Where(x => x.Author.Id == User.Id);
                    await Chn.DeleteMessagesAsync(Messages);
                }
                await ReplyAsync($"Cleaned up {User.Username} messages.");
            }
            catch (NullReferenceException Ex)
            {
                await ReplyAsync(Ex.StackTrace);
            }
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
    }
}
