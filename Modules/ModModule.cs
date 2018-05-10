using System;
using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Helpers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Valerie.Addons.Preconditions;

namespace Valerie.Modules
{
    [Name("Moderator Commands"), RequirePermission(AccessLevel.MODERATOR), RequireBotPermission(ChannelPermission.SendMessages)]
    public class ModModule : Base
    {
        [Command("Kick"), Summary("Kicks a user out of the server."), RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(IGuildUser User, [Remainder]string Reason = null)
        {
            if (Context.GuildHelper.HierarchyCheck(Context.Guild, User))
            { await ReplyAsync($"Can't kick someone whose highest role is higher than valerie's roles. ");  return;}
            await User.KickAsync(Reason).ConfigureAwait(false);
            await Context.GuildHelper.LogAsync(Context, User, CaseType.Kick, Reason).ConfigureAwait(false);
            await ReplyAsync($"***{User} got kicked.*** {Emotes.Hammer}", Document: DocumentType.Server).ConfigureAwait(false);
        }

        [Command("MassKick"), Summary("Kicks multiple users at once."), RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(params IGuildUser[] Users)
        {
            if (!Users.Any()) return;
            foreach (var User in Users)
            {
                if (Context.GuildHelper.HierarchyCheck(Context.Guild, User)) continue;
                await User.KickAsync("Multiple kicks.").ConfigureAwait(false);
                await Context.GuildHelper.LogAsync(Context, User, CaseType.Kick, "Multiple kicks.").ConfigureAwait(false);
            }
            await ReplyAsync($"{string.Join(", ", Users.Select(x => $"*{x.Username}*"))} got kicked. {Emotes.Hammer}", Document: DocumentType.Server).ConfigureAwait(false);
        }

        [Command("Ban"), Summary("Bans a user from the server."), RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(IGuildUser User, [Remainder]string Reason = null)
        {
            if (Context.GuildHelper.HierarchyCheck(Context.Guild, User))
            { await ReplyAsync($"Can't ban someone whose highest role is higher than valerie's roles. ");  return;}
            await Context.Guild.AddBanAsync(User, 7, Reason).ConfigureAwait(false);
            await Context.GuildHelper.LogAsync(Context, User, CaseType.Ban, Reason).ConfigureAwait(false);
            await ReplyAsync($"***{User} got banned.*** {Emotes.Hammer}", Document: DocumentType.Server).ConfigureAwait(false);
        }

        [Command("MassBan"), Summary("Bans multiple users at once."), RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(params IGuildUser[] Users)
        {
            if (!Users.Any()) return;
            foreach (var User in Users)
            {
                if (Context.GuildHelper.HierarchyCheck(Context.Guild, User)) continue;
                await Context.Guild.AddBanAsync(User, 7, "Mass Ban.");
                await Context.GuildHelper.LogAsync(Context, User, CaseType.Ban, "Multiple bans.");
            }
            await ReplyAsync($"{string.Join(", ", Users.Select(x => $"*{x.Username}*"))} got bent {Emotes.Hammer}", Document: DocumentType.Server);
        }

        [Command("Ban"), Summary("Bans a user from the server."), RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(ulong UserId, [Remainder] string Reason = null)
        {
            await Context.Guild.AddBanAsync(UserId, 7, Reason ?? "Secert Ban.");
            await ReplyAsync($"***{UserId} got bent.*** {Emotes.Hammer}");
        }

        [Command("Reason"), Summary("Specifies reason for a users case.")]
        public async Task ReasonAsync(int CaseNum, [Remainder] string Reason)
        {
            var Case = Context.Server.Mod.Cases.FirstOrDefault(x => x.CaseNumber == CaseNum);
            if (Case == null)
            {
                await ReplyAsync("Invalid case number.");
                return;
            }
            Case.Reason = Reason;
            var Channel = await Context.Guild.GetTextChannelAsync(Context.Server.Mod.TextChannel);
            if (await Channel.GetMessageAsync(Case.MessageId) is IUserMessage Message)
                await Message.ModifyAsync(x =>
                {
                    x.Content = $"**{Case.CaseType}** | Case {Case.CaseNumber}\n**User:** {StringHelper.CheckUser(Context.Client, Case.UserId)} ({Case.UserId})" +
                    $"\n**Reason:** {Reason}\n**Responsible Moderator:** {StringHelper.CheckUser(Context.Client, Case.ModId)}";
                });
            await ReplyAsync($"Case #{CaseNum} has been updated.", Document: DocumentType.Server);
        }

        [Command("Purge"), Priority(0), Summary("Deletes all messages from a channel."), RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeAsync(int Amount = 10)
        {
            var GetMessages = await Context.Channel.GetMessagesAsync(Amount).FlattenAsync();
            await Context.GuildHelper.PurgeAync(GetMessages.Cast<IUserMessage>(), Context.Channel as ITextChannel, Amount).ConfigureAwait(false);
        }

        [Command("Purge"), Priority(10),
            Summary("Purges specified user messages with specified limit. If no user is provided, default user will be bot and default limit will be 10."),
            RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeUserAsync(SocketGuildUser User = null, int Amount = 10)
        {
            User = User ?? await Context.Guild.GetCurrentUserAsync() as SocketGuildUser;
            var GetMessages = await Context.Channel.GetMessagesAsync(Amount).FlattenAsync();
            await Context.GuildHelper.PurgeAync(GetMessages.Where(x => x.Author.Id == User.Id).Cast<IUserMessage>(), Context.Channel as ITextChannel, Amount);
        }

        [Command("Mute"), Summary("Mutes a user."), RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(IGuildUser User)
        {
            if (User.RoleIds.Contains(Context.Server.Mod.MuteRole))
            {
                await ReplyAsync($"{User} is already muted.");
                return;
            }
            if (Context.GuildHelper.HierarchyCheck(Context.Guild, User))
            { await ReplyAsync($"Can't mute someone whose highest role is higher than valerie's roles. "); return;}
            if (Context.Guild.Roles.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted")))
            {
                Context.Server.Mod.MuteRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted").Id;
                await User.AddRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted"));
                await ReplyAsync($"{User} has been muted {Emotes.ThumbDown}", Document: DocumentType.Server);
                return;
            }
            var Permissions = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);
            if (Context.Guild.GetRole(Context.Server.Mod.MuteRole) == null)
            {
                var Role = await Context.Guild.CreateRoleAsync("Muted", GuildPermissions.None, Color.DarkerGrey);
                foreach (var Channel in (Context.Guild as SocketGuild).TextChannels)
                    if (!Channel.PermissionOverwrites.Select(x => x.Permissions).Contains(Permissions))
                        await Channel.AddPermissionOverwriteAsync(Role, Permissions).ConfigureAwait(false);
                Context.Server.Mod.MuteRole = Role.Id;
                await User.AddRoleAsync(Role);
                await ReplyAsync($"{User} has been muted {Emotes.ThumbDown}", Document: DocumentType.Server);
                return;
            }

            await User.AddRoleAsync(Context.Guild.GetRole(Context.Server.Mod.MuteRole));
            await ReplyAsync($"{User} has been muted {Emotes.ThumbDown}");
        }

        [Command("Unmute"), Summary("Umutes a user."), RequireBotPermission(GuildPermission.ManageRoles)]
        public Task UnMuteAsync(SocketGuildUser User)
        {
            var Role = Context.Guild.GetRole(Context.Server.Mod.MuteRole) ?? Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted");
            if (!User.Roles.Contains(Role)) return ReplyAsync($"{User} doesn't have the mute role.");
            User.RemoveRoleAsync(Role);
            return ReplyAsync($"{User} has been unmuted {Emotes.ThumbUp}");
        }

        [Command("Warn"), Summary("Warns a user with a specified reason."), RequireBotPermission(GuildPermission.KickMembers)]
        public async Task WarnAysnc(IGuildUser User, [Remainder]string Reason)
        {
            string WarnMessage = $"**[Warned in {Context.Guild}]** {Reason}";
            await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync(WarnMessage);
            var Profile = Context.GuildHelper.GetProfile(Context.Guild.Id, User.Id);
            Profile.Warnings++;
            if (Profile.Warnings >= 3)
            {
                await User.KickAsync($"{User} was Kicked due to reaching Max number of warnings.");
                await Context.GuildHelper.LogAsync(Context, User, CaseType.Kick, Reason);
            }
            Context.GuildHelper.SaveProfile(Context.Guild.Id, User.Id, Profile);
            await ReplyAsync($"{User} has been warned {Emotes.Shout}");
        }

        [Command("ResetWarns"), Summary("Resets users warnings.")]
        public Task ResetWarnsAsync(IGuildUser User)
        {
            var Profile = Context.GuildHelper.GetProfile(Context.Guild.Id, User.Id);
            Profile.Warnings = 0;
            Context.GuildHelper.SaveProfile(Context.Guild.Id, User.Id, Profile);
            return ReplyAsync($"{User.Username}'s warnings has been reset {Emotes.Squint}");
        }

        [Command("Blacklist"), Summary("Blacklists a user preventing them from using any Valerie's features .")]
        public Task BlacklistAsync(char Action, IGuildUser User)
        {
            var Profile = Context.GuildHelper.GetProfile(Context.Guild.Id, User.Id);
            switch (Action)
            {
                case 'a':
                    if (Profile.IsBlacklisted) return ReplyAsync($"{User} is already blacklisted.");
                    Profile.IsBlacklisted = true;
                    Context.GuildHelper.SaveProfile(Context.Guild.Id, User.Id, Profile);
                    return ReplyAsync($"{User} has been blacklisted {Emotes.ThumbDown}");
                case 'r':
                    if (!Profile.IsBlacklisted) return ReplyAsync($"{User} isn't blacklisted.");
                    Profile.IsBlacklisted = false;
                    Context.GuildHelper.SaveProfile(Context.Guild.Id, User.Id, Profile);
                    return ReplyAsync($"{User} has been whitelisted {Emotes.ThumbUp}");
            }
            return Task.CompletedTask;
        }

        [Command("Mute"), Summary("Mutes a user for x minutes.")]
        public async Task TimeoutAsync(SocketGuildUser User, int Minutes)
        {
            if (Context.GuildHelper.HierarchyCheck(Context.Guild, User))
            { await ReplyAsync($"Can't mute someone whose highest role is higher than valerie's roles. "); return;}
            var Roles = User.Roles.Where(x => x.IsEveryone == false);
            await User.RemoveRolesAsync(Roles).ContinueWith(async _ => await MuteAsync(User));
            _ = Task.Delay(TimeSpan.FromMinutes(Minutes)).ContinueWith(async y =>
             {
                 await UnMuteAsync(User);
                 await User.AddRolesAsync(Roles);
             });
        }
    }
}