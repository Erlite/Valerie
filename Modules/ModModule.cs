using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Handlers;
using Valerie.Extensions;

namespace Valerie.Modules
{
    [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.ManageMessages)]
    public class ModModule : ValerieBase<ValerieContext>
    { 
        [Command("Kick"), Summary("Kicks user from the guild."),
            RequireBotPermission(GuildPermission.KickMembers),
            RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(IGuildUser User, [Remainder]string Reason = "No reason provided by moderator.")
        {
            await User.KickAsync(Reason);
            Context.Config.ModLog.Cases += 1;
            ITextChannel Channel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.ModLog.TextChannel));
            if (Channel != null)
            {
                var embed = ValerieEmbed.Embed(EmbedColor.Red, ThumbUrl: User.GetAvatarUrl(), FooterText: $"Kick Date: {DateTime.Now}");
                embed.AddField("User", $"{User.Username}#{User.Discriminator}\n{User.Id}", true);
                embed.AddField("Responsible Moderator", Context.User.Username, true);
                embed.AddField("Case No.", Context.Config.ModLog.Cases, true);
                embed.AddField("Case Type", "Kick", true);
                embed.AddField("Reason", Reason, true);
                await Channel.SendMessageAsync("", embed: embed.Build());
            }
            else
                await ReplyAsync($"***{User.Username} got kicked*** :ok_hand:");
        }

        [Command("Ban"), Summary("Bans user from the guild."),
            RequireBotPermission(GuildPermission.BanMembers),
            RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(IGuildUser User, [Remainder] string Reason = "No reason provided by moderator.")
        {
            await Context.Guild.AddBanAsync(User, 7, Reason);
            Context.Config.ModLog.Cases += 1;
            ITextChannel Channel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.ModLog.TextChannel));
            if (Channel != null)
            {
                var embed = ValerieEmbed.Embed(EmbedColor.Red, ThumbUrl: User.GetAvatarUrl(), FooterText: $"Ban Date: {DateTime.Now}");
                embed.AddField("User", $"{User.Username}#{User.Discriminator}\n{User.Id}", true);
                embed.AddField("Responsible Moderator", Context.User.Username, true);
                embed.AddField("Case No.", Context.Config.ModLog.Cases, true);
                embed.AddField("Case Type", "Ban", true);
                embed.AddField("Reason", Reason, true);
                await Channel.SendMessageAsync("", embed: embed.Build());
            }
            else
                await ReplyAsync($"***{User.Username} got kicked*** :ok_hand:");
        }

        [Command("Unban"), Summary("Unbans user from the guild"),
            RequireBotPermission(GuildPermission.BanMembers),
            RequireUserPermission(GuildPermission.BanMembers)]
        public async Task UnBanAsync(IGuildUser User)
        {
            await Context.Guild.RemoveBanAsync(User);
            await ReplyAsync($"**{User} got unbent.** :v:");
        }

        [Command("PurgeChannel"), Summary("Purges 500 messages from a channel."), Alias("PC"),
            RequireBotPermission(ChannelPermission.ManageMessages),
            RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeChannelAsync(ITextChannel Channel)
        {
            var Messages = await Channel.GetMessagesAsync(500).Flatten();
            await Channel.DeleteMessagesAsync(Messages);
        }

        [Command("PurgeUser"), Summary("Purges User messages from current channel."), Alias("PU"),
            RequireBotPermission(ChannelPermission.ManageMessages),
            RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeUserAsync(int Amount, IGuildUser User)
        {
            var GetMessages = (await Context.Channel.GetMessagesAsync(Amount).Flatten()).Where(x => x.Author.Id == User.Id);
            if (Amount <= 100)
                await Context.Channel.DeleteMessagesAsync(GetMessages);
            else if (Amount > 100)
                foreach (var msg in GetMessages)
                    await msg.DeleteAsync();
        }

        [Command("Purge"), Summary("Deletes all messages from a channel."), Alias("Del"),
            RequireBotPermission(ChannelPermission.ManageMessages),
            RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task Purge(int Amount)
        {
            var GetMessages = await Context.Channel.GetMessagesAsync(Amount).Flatten();
            if (Amount <= 100)
                await Context.Channel.DeleteMessagesAsync(GetMessages);
            else if (Amount > 100)
                foreach (var msg in GetMessages)
                    await msg.DeleteAsync();
        }

        [Command("Addrole"), Alias("Arole"), Summary("Adds user to the specified role."),
            RequireBotPermission(GuildPermission.ManageRoles),
            RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddroleAsync(IGuildUser User, IRole Role)
        {
            await User.AddRoleAsync(Role);
            await ReplyAsync($"**{User} has been added to {Role.Name}.** :v:");
        }

        [Command("Removerole"), Alias("Rrole"), Summary("Removes user from the specified role."),
            RequireBotPermission(GuildPermission.ManageRoles),
            RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRoleAsync(IGuildUser User, IRole Role)
        {
            await User.RemoveRoleAsync(Role);
            await ReplyAsync($"**{User} has been removed from {Role.Name}.** :v:");
        }

        [Command("Mute"), Summary("Mutes a user."),
            RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task MuteAsync(IGuildUser User)
        {
            if (User.RoleIds.Contains(Convert.ToUInt64(Context.Config.ModLog.MuteRole)))
            {
                await ReplyAsync($"{User} is already muted.");
                return;
            }
            if (Context.Guild.Roles.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted")))
            {
                Context.Config.ModLog.MuteRole = $"{ Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted").Id}";
                await ReplyAsync($"**{User} has been muted** :zipper_mouth:");
                await User.AddRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted"));
                return;
            }
            OverwritePermissions Permissions = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny, useExternalEmojis: PermValue.Deny);
            if (Context.Guild.GetRole(Convert.ToUInt64(Context.Config.ModLog.MuteRole)) == null)
            {
                var Role = await Context.Guild.CreateRoleAsync("Muted", GuildPermissions.None, Color.Default);
                foreach (var Channel in (Context.Guild as SocketGuild).TextChannels)
                {
                    if (!Channel.PermissionOverwrites.Select(x => x.Permissions).Contains(Permissions))
                    {
                        await Channel.AddPermissionOverwriteAsync(Role, Permissions).ConfigureAwait(false);
                    }
                }
                Context.Config.ModLog.MuteRole = $"{Role.Id}";
                await User.AddRoleAsync(Role);
                Context.Config.ModLog.Cases += 1;
                await ReplyAsync($"**{User} has been muted** :zipper_mouth:");
                return;
            }
            else
            {
                await User.AddRoleAsync(Context.Guild.GetRole(Convert.ToUInt64(Context.Config.ModLog.MuteRole)));
                Context.Config.ModLog.Cases += 1;
                await ReplyAsync($"**{User} has been muted** :zipper_mouth:");
                return;
            }
        }

        [Command("Unmute"), Summary("Umutes a user."),
            RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task UnMuteAsync(SocketGuildUser User)
        {
            IRole Role = Context.Guild.GetRole(Convert.ToUInt64(Context.Config.ModLog.MuteRole));
            if (Role == null)
            {
                await ReplyAsync("Woopsie, Mute role is empty.");
                return;
            }
            if (!User.Roles.Contains(Role))
            {
                await ReplyAsync($"**{User} isn't muted.** :eyes:");
                return;
            }
            await User.RemoveRoleAsync(Role);
            await ReplyAsync($"**{User} has been unmuted.** :v:");
        }

        [Command("Warn"), Summary("Warns a user with a specified reason."),
            RequireBotPermission(GuildPermission.KickMembers),
            RequireUserPermission(GuildPermission.KickMembers)]
        public async Task WarnAysnc(IGuildUser User, [Remainder]string Reason)
        {
            string WarnMessage = $"**[Warned in {Context.Guild}]** {Reason}";
            if (!Context.Config.ModLog.Warnings.ContainsKey(User.Id))
            {
                Context.Config.ModLog.Warnings.TryAdd(User.Id, 1);
                Context.Config.ModLog.Cases += 1;
                await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync(WarnMessage);
                await ReplyAsync($"**{User} has been warned** :ok_hand:");
                return;
            }

            Context.Config.ModLog.Cases += 1;
            Context.Config.ModLog.Warnings.TryGetValue(User.Id, out int OldValue);
            Context.Config.ModLog.Warnings.TryUpdate(User.Id, OldValue += 1, OldValue);

            if (Context.Config.ModLog.Warnings[User.Id] == 3)
            {
                await User.KickAsync($"**[Kicked from {Context.Guild}]** {Reason}");
                return;
            }

            await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync(WarnMessage);
            await ReplyAsync($"**{User} has been warned** :ok_hand:");
        }
    }
}