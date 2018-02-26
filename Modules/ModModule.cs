using System;
using Discord;
using System.Linq;
using Discord.Commands;
using Valerie.Attributes;
using Valerie.JsonModels;
using Discord.WebSocket;
using Valerie.Modules.Addons;
using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules
{
    [Name("Admin & Moderater Commands"), RequireAccess(AccessLevel.AdminsNMods), RequireBotPermission(ChannelPermission.SendMessages)]
    public class ModModule : ValerieBase
    {
        [Command("Kick"), Summary("Kicks a user out of the server."), RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(IGuildUser User, [Remainder]string Reason = null)
        {
            await User.KickAsync(Reason);
            await ReplyAsync($"***{User} got kicked.*** :ok_hand:");
            await LogAsync(User, CaseType.Kick, Reason);
        }

        [Command("MassKick"), Summary("Kicks multiple users at once."), RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(params IGuildUser[] Users)
        {
            foreach (var User in Users)
            {
                await User.KickAsync("Mass Kick");
                await LogAsync(User, CaseType.Kick, "Mass Kick");
            }
            await ReplyAsync($"{string.Join(", ", Users.Select(x => $"*{x.Username}*"))} got kicked. :ok_hand:");
        }

        [Command("Ban"), Summary("Bans a user from the server."), RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(IGuildUser User, [Remainder]string Reason = null)
        {
            await Context.Guild.AddBanAsync(User, 7, Reason);
            await ReplyAsync($"***{User} got bent.*** :ok_hand:");
            await LogAsync(User, CaseType.Ban, Reason);
        }

        [Command("MassBan"), Summary("Bans multiple users at once."), RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(params IGuildUser[] Users)
        {
            foreach (var User in Users)
            {
                await Context.Guild.AddBanAsync(User, 7, "Mass Ban.");
                await LogAsync(User, CaseType.Ban, "Mass Ban");
            }
            await ReplyAsync($"{string.Join(", ", Users.Select(x => $"*{x.Username}*"))} got bent. :ok_hand:");
        }

        [Command("Ban"), Summary("Bans a user form the server even if they are not in the server."), RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(ulong UserId, [Remainder] string Reason = null)
        {
            await Context.Guild.AddBanAsync(UserId, 7, Reason ?? "Secert Ban.");
            await ReplyAsync($"***{UserId} got bent.*** :ok_hand:");
        }

        [Command("Reason"), Summary("Specifies reason for a user case.")]
        public async Task ReasonAsync(int Case, [Remainder] string Reason)
        {
            var GetCase = Context.Server.ModLog.Cases.FirstOrDefault(x => x.CaseNumber == Case);
            if (GetCase == null)
            {
                await ReplyAsync("Invalid case number.");
                return;
            }
            Context.Server.ModLog.Cases.FirstOrDefault(x => x.CaseNumber == Case).Reason = Reason;
            var Channel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Server.ModLog.TextChannel));
            if (await Channel.GetMessageAsync(Convert.ToUInt64(GetCase.MessageId)) is IUserMessage Message)
                await Message.ModifyAsync(x =>
                {
                    x.Content = $"**{GetCase.CaseType}** | Case {GetCase.CaseNumber}\n**User:** {GetCase.UserInfo}\n**Reason:** {Reason}\n" +
                        $"**Responsible Moderator:** {GetCase.ResponsibleMod}";
                });
            GetCase.Reason = Reason;
            await SaveAsync(ModuleEnums.Server);
        }

        [Command("Purge Channel"), Summary("Purges 100 messages from a channel."), Alias("PC"), RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeChannelAsync(ITextChannel Channel = null)
        {
            Channel = Channel ?? Context.Channel as ITextChannel;
            var Messages = await Channel.GetMessagesAsync(100).Flatten();
            await Channel.DeleteMessagesAsync(Messages);
        }

        [Command("Purge User"), Alias("PU"),
            Summary("Purges specified user messages with specified limit. If no user is provided, default user will be bot and default limit will be 10."),
            RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeUserAsync(IGuildUser User = null, int Amount = 10)
        {
            User = Context.Client.CurrentUser as IGuildUser ?? User;
            var GetMessages = (await Context.Channel.GetMessagesAsync(Amount).Flatten()).Where(x => x.Author.Id == User.Id);
            if (Amount <= 100) await (Context.Channel as ITextChannel).DeleteMessagesAsync(GetMessages);
            else if (Amount > 100) foreach (var msg in GetMessages) await msg.DeleteAsync().ConfigureAwait(false);
        }

        [Command("Purge"), Alias("Delete", "Del"), Summary("Deletes all messages from a channel."), RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task Purge(int Amount = 10)
        {
            var GetMessages = await Context.Channel.GetMessagesAsync(Amount).Flatten();
            if (Amount <= 100) await (Context.Channel as ITextChannel).DeleteMessagesAsync(GetMessages);
            else if (Amount > 100) foreach (var msg in GetMessages) await msg.DeleteAsync().ConfigureAwait(false);
        }

        [Command("Role"), Summary("Adds/Removes a role from a user."), RequireBotPermission(GuildPermission.ManageRoles)]
        public Task ManageRoleAsync(ModuleEnums Action, IGuildUser User, IRole Role)
        {
            switch (Action)
            {
                case ModuleEnums.Add: User.AddRoleAsync(Role); break;
                case ModuleEnums.Remove: User.RemoveRoleAsync(Role); break;
            }
            return ReplyAsync($"Done.");
        }

        [Command("Mute"), Summary("Mutes a user."), RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task MuteAsync(IGuildUser User)
        {
            if (User.RoleIds.Contains(Convert.ToUInt64(Context.Server.ModLog.MuteRole)))
            {
                await ReplyAsync($"{User} is already muted.");
                return;
            }
            if (Context.Guild.Roles.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted")))
            {
                Context.Server.ModLog.MuteRole = $"{ Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted").Id}";
                await User.AddRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted"));
                await SaveAsync(ModuleEnums.Server, $"**{User} has been muted** :zipper_mouth:");
                return;
            }
            OverwritePermissions Permissions = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny, useExternalEmojis: PermValue.Deny);
            if (Context.Guild.GetRole(Convert.ToUInt64(Context.Server.ModLog.MuteRole)) == null)
            {
                var Role = await Context.Guild.CreateRoleAsync("Muted", GuildPermissions.None, Color.Default);
                foreach (var Channel in (Context.Guild as SocketGuild).TextChannels)
                    if (!Channel.PermissionOverwrites.Select(x => x.Permissions).Contains(Permissions))
                        await Channel.AddPermissionOverwriteAsync(Role, Permissions).ConfigureAwait(false);

                Context.Server.ModLog.MuteRole = $"{Role.Id}";
                await User.AddRoleAsync(Role);
                await SaveAsync(ModuleEnums.Server, $"**{User} has been muted** :zipper_mouth:");
                return;
            }

            await User.AddRoleAsync(Context.Guild.GetRole(Convert.ToUInt64(Context.Server.ModLog.MuteRole)));
            await ReplyAsync($"**{User} has been muted** :zipper_mouth:");
        }

        [Command("Unmute"), Summary("Umutes a user."), RequireUserPermission(GuildPermission.MuteMembers)]
        public Task UnMuteAsync(SocketGuildUser User)
        {
            IRole Role = Context.Guild.GetRole(Convert.ToUInt64(Context.Server.ModLog.MuteRole));
            if (Role == null) Role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted");
            if (!User.Roles.Contains(Role)) return ReplyAsync($"{User} doesn't have the mute role.");
            User.RemoveRoleAsync(Role);
            return ReplyAsync($"**{User} has been unmuted.** :v:");
        }

        [Command("Warn"), Summary("Warns a user with a specified reason."), RequireBotPermission(GuildPermission.KickMembers)]
        public async Task WarnAysnc(IGuildUser User, [Remainder]string Reason)
        {
            string WarnMessage = $"**[Warned in {Context.Guild}]** {Reason}";
            if (!Context.Server.Profiles.ContainsKey(User.Id))
            {
                Context.Server.Profiles.Add(User.Id, new UserProfile { Warnings = 1 });
                await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync(WarnMessage);
                await SaveAsync(ModuleEnums.Server, $"**{User} has been warned** :ok_hand:");
                return;
            }

            Context.Server.Profiles[User.Id].Warnings++;
            if (Context.Server.Profiles[User.Id].Warnings == 3)
            {
                await User.KickAsync($"{User} was Kicked due to reaching Max number of warnings.");
                await ReplyAsync($"{User} was Kicked due to reaching Max number of warnings.");
                await LogAsync(User, CaseType.Kick, "Kicked due to reaching max number of warnings.");
                return;
            }

            await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync(WarnMessage);
            await ReplyAsync($"**{User} has been warned** :ok_hand:");
        }

        [Command("ResetWarns"), Summary("Resets users warnings.")]
        public Task ResetWarnsAsync(IGuildUser User)
        {
            if (!Context.Server.Profiles.ContainsKey(User.Id)) return ReplyAsync($"{User} has no warnings.");
            Context.Server.Profiles[User.Id].Warnings = 0;
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Blacklist"), Summary("Add's a user to server's blacklist. This prevents user from running Valerie's commands.")]
        public Task BlacklistAsync(ModuleEnums Action, IGuildUser User)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.BlacklistedUsers.Contains(User.Id)) return ReplyAsync($"{User} is already blacklisted.");
                    else if (Context.Server.BlacklistedUsers.Count == Context.Server.BlacklistedUsers.Capacity)
                        return ReplyAsync("Blacklist can't have more than 50 users.");
                    Context.Server.BlacklistedUsers.Add(User.Id);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (Context.Server.BlacklistedUsers.Contains(User.Id)) return ReplyAsync($"{User} isn't  blacklisted.");
                    Context.Server.BlacklistedUsers.Remove(User.Id);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }
    }
}