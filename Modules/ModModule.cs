using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Valerie.Attributes;
using Valerie.Handlers.ModuleHandler;
using Models;

namespace Valerie.Modules
{
    [RequireAccess(AccessLevel.AdminsNMods)]
    public class ModModule : ValerieBase
    {
        [Command("Kick"), Summary("Kick's a user out of the server."), RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(IGuildUser User, [Remainder]string Reason)
        {
            await User.KickAsync(Reason);
            ITextChannel Channel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Server.ModLog.TextChannel));
            IUserMessage Message = null;
            Reason = Reason ?? $"*Responsible moderator, please type `{Context.Config.Prefix}Reason {Context.Server.ModLog.ModCases.Count + 1} <Reason>`*";

            if (Channel != null)
                Message = await Channel.SendMessageAsync($"**Kick** | Case {Context.Server.ModLog.ModCases.Count + 1}\n**User:** {User} ({User.Id})\n**Reason:** {Reason}\n" +
                    $"**Responsible Moderator:** {Context.User}");

            Context.Server.ModLog.ModCases.Add(new CaseWrapper()
            {
                UserInfo = $"{User.Username} ({User.Id})",
                Reason = Reason,
                CaseType = CaseType.Kick,
                MessageId = $"{Message.Id}",
                ResponsibleMod = $"{Context.User}",
                CaseNumber = Context.Server.ModLog.ModCases.Count + 1
            });

            await ReplyAsync($"***{User} got kicked.*** :ok_hand:");
        }

        [Command("MassKick"), Summary("Kick's multiple users at once."), RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(params IGuildUser[] Users)
        {
            IUserMessage Message = null;
            ITextChannel Channel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Server.ModLog.TextChannel));
            foreach (var User in Users)
            {
                ulong Id = 0;
                string Reason = $"*Responsible moderator, please type `{Context.Config.Prefix}Reason {Context.Server.ModLog.ModCases.Count + 1} <Reason>`*";
                if (Channel != null)
                    Message = await Channel.SendMessageAsync($"**Kick** | Case {Context.Server.ModLog.ModCases.Count + 1}\n**User:** {User} ({User.Id})\n**Reason:** {Reason}\n" +
                         $"**Responsible Moderator:** {Context.User}");
                if (Message != null) Id = Message.Id;
                Context.Server.ModLog.ModCases.Add(new CaseWrapper()
                {
                    UserInfo = $"{User.Username} ({User.Id})",
                    Reason = Reason,
                    MessageId = $"{Id}",
                    CaseType = CaseType.Kick,
                    ResponsibleMod = $"{Context.User}",
                    CaseNumber = Context.Server.ModLog.ModCases.Count + 1
                });
                await User.KickAsync("Mass Kick");
            }
            await ReplyAsync($"{string.Join(", ", Users.Select(x => $"*{x.Username}*"))} got kicked. :ok_hand:");
        }

        [Command("Ban"), Summary("Ban's a user from the server."), RequireBotPermission(GuildPermission.KickMembers)]
        public async Task BanAsync(IGuildUser User, [Remainder]string Reason)
        {
            await Context.Guild.AddBanAsync(User, 7, Reason);
            ITextChannel Channel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Server.ModLog.TextChannel));
            IUserMessage Message = null;
            Reason = Reason ?? $"*Responsible moderator, please type `{Context.Config.Prefix}Reason {Context.Server.ModLog.ModCases.Count + 1} <Reason>`*";

            if (Channel != null)
                Message = await Channel.SendMessageAsync($"**Kick** | Case {Context.Server.ModLog.ModCases.Count + 1}\n**User:** {User} ({User.Id})\n**Reason:** {Reason}\n" +
                    $"**Responsible Moderator:** {Context.User}");

            Context.Server.ModLog.ModCases.Add(new CaseWrapper()
            {
                UserInfo = $"{User.Username} ({User.Id})",
                Reason = Reason,
                CaseType = CaseType.Ban,
                MessageId = $"{Message.Id}",
                ResponsibleMod = $"{Context.User}",
                CaseNumber = Context.Server.ModLog.ModCases.Count + 1
            });

            await ReplyAsync($"***{User} got bent.*** :ok_hand:");
        }

        [Command("MassBan"), Summary("Ban's multiple users at once."), RequireBotPermission(GuildPermission.KickMembers)]
        public async Task BanAsync(params IGuildUser[] Users)
        {
            IUserMessage Message = null;
            ITextChannel Channel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Server.ModLog.TextChannel));
            foreach (var User in Users)
            {
                string Reason = $"*Responsible moderator, please type `{Context.Config.Prefix}Reason {Context.Server.ModLog.ModCases.Count + 1} <Reason>`*";
                if (Channel != null)
                    Message = await Channel.SendMessageAsync($"**Kick** | Case {Context.Server.ModLog.ModCases.Count + 1}\n**User:** {User} ({User.Id})\n**Reason:** {Reason}\n" +
                        $"**Responsible Moderator:** {Context.User}");

                Context.Server.ModLog.ModCases.Add(new CaseWrapper()
                {
                    UserInfo = $"{User.Username} ({User.Id})",
                    Reason = Reason,
                    CaseType = CaseType.Ban,
                    MessageId = $"{Message.Id}",
                    ResponsibleMod = $"{Context.User}",
                    CaseNumber = Context.Server.ModLog.ModCases.Count + 1
                });
                await Context.Guild.AddBanAsync(User, 7, "Mass Ban");
            }
            await ReplyAsync($"{string.Join(", ", Users.Select(x => $"*{x.Username}*"))} got bent. :ok_hand:");
        }

        [Command("Reason"), Summary("Specifies reason for a user case.")]
        public async Task ReasonAsync(int Case, [Remainder] string Reason)
        {
            var GetCase = Context.Server.ModLog.ModCases.FirstOrDefault(x => x.CaseNumber == Case);
            if (GetCase == null)
            {
                await ReplyAsync("Invalid case number.");
                return;
            }
            Context.Server.ModLog.ModCases.FirstOrDefault(x => x.CaseNumber == Case).Reason = Reason;
            ITextChannel Channel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Server.ModLog.TextChannel));
            var Message = await Channel.GetMessageAsync(Convert.ToUInt64(GetCase.MessageId)) as IUserMessage;
            if (Message != null)
                await Message.ModifyAsync(x =>
                {
                    x.Content = $"**{GetCase.CaseType}** | Case {GetCase.CaseNumber}\n**User:** {GetCase.UserInfo}\n**Reason:** {Reason}\n" +
                        $"**Responsible Moderator:** {GetCase.ResponsibleMod}";
                });
            await SaveAsync();
        }

        [Command("Purge Channel"), Summary("Purges 500 messages from a channel."), Alias("PC"), RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeChannelAsync(ITextChannel Channel)
        {
            var Messages = await Channel.GetMessagesAsync(500).Flatten();
            await Channel.DeleteMessagesAsync(Messages);
        }

        [Command("Purge User"), Alias("PU"), Summary("Purges User messages from current channel."), RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeUserAsync(int Amount, IGuildUser User)
        {
            var GetMessages = (await Context.Channel.GetMessagesAsync(Amount).Flatten()).Where(x => x.Author.Id == User.Id);
            if (Amount <= 100) await (Context.Channel as ITextChannel).DeleteMessagesAsync(GetMessages);
            else if (Amount > 100) foreach (var msg in GetMessages) await msg.DeleteAsync().ConfigureAwait(false);
        }

        [Command("Purge"), Alias("Delete", "Del"), Summary("Deletes all messages from a channel."), RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task Purge(int Amount)
        {
            var GetMessages = await Context.Channel.GetMessagesAsync(Amount).Flatten();
            if (Amount <= 100) await (Context.Channel as ITextChannel).DeleteMessagesAsync(GetMessages);
            else if (Amount > 100) foreach (var msg in GetMessages) await msg.DeleteAsync().ConfigureAwait(false);
        }


    }
}