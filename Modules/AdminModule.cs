using Discord;
using System.Linq;
using Discord.Commands;
using Valerie.Attributes;
using Valerie.Extensions;
using Valerie.Modules.Addons;
using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules
{
    [Name("Admin/Server Owner Commands"), RequireAccess(AccessLevel.Admins)]
    public class AdminModule : ValerieBase
    {
        [Command("Update Prefix"), Summary("Updates current server's prefix.")]
        public Task PrefixAsync(string Prefix)
        {
            Context.Server.Prefix = Prefix;
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Set Channel"), Summary("Sets channel for Join/Leave/Chatterbot. To remove channel, don't provide channel name.")]
        public Task SetChannelAsync(ModuleEnums ChannelType, ITextChannel Channel = null)
        {
            string ChannelId = $"{Channel?.Id}" ?? null;
            switch (ChannelType)
            {
                case ModuleEnums.Chatter: Context.Server.ChatterChannel = ChannelId; break;
                case ModuleEnums.Join: Context.Server.JoinChannel = ChannelId; break;
                case ModuleEnums.Leave: Context.Server.LeaveChannel = ChannelId; break;
                case ModuleEnums.Mod: Context.Server.ModLog.TextChannel = ChannelId; break;
                case ModuleEnums.Starboard: Context.Server.Starboard.TextChannel = ChannelId; break;
            }
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Set MaxWarns"), Summary("Sets how many warning a user can get before getting kicked by automod. -1 to disable it.")]
        public Task SetMaxWarnsAsync(int MaxWarnings)
        {
            Context.Server.ModLog.MaxWarnings = MaxWarnings;
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Set AutoRole"), Summary("Sets auto role for this server.")]
        public Task SetAutoRoleAsync(IRole Role)
        {
            Context.Server.ModLog.AutoAssignRole = $"{Role.Id}";
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Set LvlUpMsg"), Summary("Sets level up message for when a user level ups. To remove message,  don't provide any level up message.")]
        public Task SetLevelUpMessageAsync([Remainder]string Message = null)
        {
            Context.Server.ChatXP.LevelMessage = Message;
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("SelfRoles"), Alias("SlfR"), Summary("Adds/Removes role to/from self assingable roles. Action: Add, Remove")]
        public Task SelfRoleAsync(ModuleEnums Action, IRole Role)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.AssignableRoles.Contains(Role.Id)) return ReplyAsync($"{Role.Name} already exists in assignable roles.");
                    else if (Context.Server.AssignableRoles.Count == Context.Server.AssignableRoles.Capacity)
                        return ReplyAsync($"It seems you have reached max number of assignable roles.");
                    Context.Server.AssignableRoles.Add(Role.Id);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.AssignableRoles.Contains(Role.Id)) return ReplyAsync($"I couldn't find  {Role.Name} role.");
                    Context.Server.AssignableRoles.Remove(Role.Id);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("JoinMsg"), Alias("JMsg"),
            Summary("Add/Removes join message. Action: Add, Remove. {user} to mention user. {guild} to print server name.")]
        public Task JoinAsync(ModuleEnums Action, [Remainder] string Message)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.JoinMessages.Count == Context.Server.JoinMessages.Capacity) return ReplyAsync("You have reached max number of join messages.");
                    else if (Context.Server.JoinMessages.Contains(Message)) return ReplyAsync("Join message already exists. Try something different?");
                    Context.Server.JoinMessages.Add(Message);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.JoinMessages.Contains(Message)) return ReplyAsync("I couldn't find the specified join message.");
                    Context.Server.JoinMessages.Remove(Message);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("LeaveMsg"), Alias("LMsg"),
            Summary("Add/Removes leave message. Action: Add, Remove. {user} to mention user. {guild} to print server name.")]
        public Task LeaveAsync(ModuleEnums Action, [Remainder] string Message)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.LeaveMessages.Count == Context.Server.LeaveMessages.Capacity) return ReplyAsync("You have reached max number of leave messages.");
                    else if (Context.Server.LeaveMessages.Contains(Message)) return ReplyAsync("Leave message already exists. Try something different?");
                    Context.Server.LeaveMessages.Add(Message);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.LeaveMessages.Contains(Message)) return ReplyAsync("I couldn't find the specified leave message.");
                    Context.Server.LeaveMessages.Remove(Message);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Toggle"), Summary("Enables/Disables various guild's actions. Action: XP, AutoMod")]
        public Task ToggleAsync(ModuleEnums Action)
        {
            switch (Action)
            {
                case ModuleEnums.XP:
                    Context.Server.ChatXP.IsEnabled = !Context.Server.ChatXP.IsEnabled;
                    string XpState = Context.Server.ChatXP.IsEnabled ? "Enabled" : "Disabled";
                    return SaveAsync(ModuleEnums.Server, $"{Action} has been {XpState}.");
                case ModuleEnums.AutoMod:
                    Context.Server.ModLog.IsAutoModEnabled = !Context.Server.ModLog.IsAutoModEnabled;
                    string ModState = Context.Server.ChatXP.IsEnabled ? "Enabled" : "Disabled.";
                    return SaveAsync(ModuleEnums.Server, $"{Action} has been {ModState}");
            }
            return Task.CompletedTask;
        }

        [Command("Admins"), Summary("Adds/Removes users from server's admins. Action: Add, Remove. Admins are able to change bot's server configuration.")]
        public Task AdminsAsync(ModuleEnums Action, IGuildUser User)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.Admins.Contains(User.Id)) return ReplyAsync($"{User} is already an Admin.");
                    else if (Context.Server.Admins.Count == Context.Server.Admins.Capacity) return ReplyAsync($"It seems you have reached max number of admins.");
                    Context.Server.Admins.Add(User.Id);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.Admins.Contains(User.Id)) return ReplyAsync($"{User} isn't an Admin.");
                    Context.Server.Admins.Remove(User.Id);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("XpForbid"), Summary("Forbids a role from gaining XP. Action: Add, Remove")]
        public Task ForbidAsync(ModuleEnums Action, IRole Role)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.ChatXP.ForbiddenRoles.Contains(Role.Id)) return ReplyAsync($"{Role} already is forbidden from gaining XP.");
                    else if (Context.Server.ChatXP.ForbiddenRoles.Count == Context.Server.ChatXP.ForbiddenRoles.Capacity)
                        return ReplyAsync("It seems you have reached max number of forbidden roles.");
                    Context.Server.ChatXP.ForbiddenRoles.Add(Role.Id);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.ChatXP.ForbiddenRoles.Contains(Role.Id)) return ReplyAsync($"{Role} isn't forbidden from gaining XP.");
                    Context.Server.ChatXP.ForbiddenRoles.Remove(Role.Id);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("XpLevel"), Summary("Adds/Removes a level up role. Action: Add, Remove, Modify")]
        public Task LevelAsync(ModuleEnums Action, IRole Role, int Level = 10)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.ChatXP.LevelRoles.Count == 15) return ReplyAsync("You have reached max number of level up roles.");
                    else if (Context.Server.ChatXP.LevelRoles.ContainsKey(Role.Id)) return ReplyAsync($"{Role} is already a level-up role.");
                    Context.Server.ChatXP.LevelRoles.Add(Role.Id, Level);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.ChatXP.LevelRoles.ContainsKey(Role.Id)) return ReplyAsync($"{Role} isn't a level-up role.");
                    Context.Server.ChatXP.LevelRoles.Remove(Role.Id);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Modify:
                    if (!Context.Server.ChatXP.LevelRoles.ContainsKey(Role.Id)) return ReplyAsync($"{Role} isn't a level-up role.");
                    Context.Server.ChatXP.LevelRoles[Role.Id] = Level;
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Badwords"), Summary("Add/remove bad words. If a user says a bad word their message will be deleted. Action: Add, Remove")]
        public Task BadWordsAsync(ModuleEnums Action, string Badword)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.ModLog.BadWords.Count == Context.Server.ModLog.BadWords.Capacity)
                        return ReplyAsync("You have reached max number of bad words.");
                    else if (Context.Server.ModLog.BadWords.Contains(Badword)) return ReplyAsync($"{Badword} already exists.");
                    Context.Server.ModLog.BadWords.Add(Badword);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.ModLog.BadWords.Contains(Badword)) return ReplyAsync($"{Badword} doesn't exists.");
                    Context.Server.ModLog.BadWords.Remove(Badword);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Blocklink"), Alias("Add/removes a link from blocked urls. Users will be warned for posting a blocked link. Action: Add, Remove")]
        public Task BlocklinkAsync(ModuleEnums Action, string Url)
        {
            var Check = BoolExt.IsValidUrl(Url);
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (!Check.Item1) return ReplyAsync($"{Url} isn't a valid Url.");
                    else if (Context.Server.ModLog.BlockedUrls.Contains(Check.Item2)) return ReplyAsync($"{Url} already is blocked.");
                    Context.Server.ModLog.BlockedUrls.Add(Check.Item2);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.ModLog.BlockedUrls.Contains(Check.Item2)) return ReplyAsync($"{Url} isn't blocked.");
                    Context.Server.ModLog.BlockedUrls.Remove(Check.Item2);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Show JoinMsgs"), Alias("Showjms"), Summary("Shows all the join messages for this server.")]
        public Task ShowJoinMsgsAsync()
        {
            if (!Context.Server.JoinMessages.Any()) return ReplyAsync($"{Context.Guild} has no join messages.");
            return ReplyAsync($"**Join Messages**\n{string.Join("\n", $"-> {Context.Server.JoinMessages}")}");
        }

        [Command("Show LeaveMsgs"), Alias("Showlms"), Summary("Shows all the join messages for this server.")]
        public Task ShowLeaveMsgsAsync()
        {
            if (!Context.Server.LeaveMessages.Any()) return ReplyAsync($"{Context.Guild} has no leave messages.");
            return ReplyAsync($"**Leave Messages**\n{string.Join("\n", $"-> {Context.Server.LeaveMessages}")}");
        }

        [Command("Show Admins"), Alias("ShowAs"), Summary("Shows all the current admins for this server.")]
        public async Task ShowAdminsAsync()
        {
            if (!Context.Server.Admins.Any())
            {
                await ReplyAsync($"{Context.Guild} has no admins.");
                return;
            }
            string Admins = null;
            foreach (var Id in Context.Server.Admins)
                Admins += $"\n-> {Id} | {await StringExt.CheckUserAsync(Context, Id)}";
            await ReplyAsync($"**{Context.Guild} Admins**\n{Admins}");
        }

        [Command("Show Forbidden"), Alias("Showfb"), Summary("Shows all the forbidden roles for this server.")]
        public Task ShowForbiddenAsync()
        {
            if (!Context.Server.ChatXP.ForbiddenRoles.Any()) return ReplyAsync($"{Context.Guild} has no forbidden roles.");
            string Roles = null;
            foreach (var Id in Context.Server.ChatXP.ForbiddenRoles)
                Roles += $"-> {Id} | {StringExt.CheckRole(Context, Id)}";
            return ReplyAsync($"**Forbidden Roles**\n{Roles}");
        }

        [Command("Show Levels"), Alias("Showlvls"), Summary("Shows all the level up roles for this server.")]
        public Task ShowLevelsAsync()
        {
            if (!Context.Server.ChatXP.LevelRoles.Any()) return ReplyAsync($"{Context.Guild} has no level-up roles.");
            string Roles = null;
            foreach (var Id in Context.Server.ChatXP.LevelRoles.Keys)
                Roles += $"-> {Id} | {StringExt.CheckRole(Context, Id)}";
            return ReplyAsync($"**Level-Up Roles**\n{Roles}");
        }

        [Command("Show Badwords"), Alias("Showbws"), Summary("Shows all the bad words for this server.")]
        public Task ShowBadwordsAsync()
        {
            if (!Context.Server.ModLog.BadWords.Any()) return ReplyAsync($"{Context.Guild} has no prohibited words.");
            return ReplyAsync($"**Bad Words**\n{string.Join(", ", Context.Server.ModLog.BadWords)}");
        }

        [Command("Show BlockedUrls"), Alias("Showbls"), Summary("Shows all the blocked links for this server.")]
        public Task ShowBlockedUrlsAsync()
        {
            if (!Context.Server.ModLog.BlockedUrls.Any()) return ReplyAsync($"{Context.Guild} has no blocked urls.");
            return ReplyAsync($"**Blocked Links**\n{string.Join(", ", Context.Server.ModLog.BlockedUrls)}");
        }
    }
}