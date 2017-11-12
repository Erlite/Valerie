using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Valerie.Attributes;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules
{
    [RequireAccess(AccessLevel.Admins)]
    public class AdminModule : ValerieBase
    {
        [Command("Update Prefix"), Summary("Updates current server's prefix.")]
        public Task PrefixAsync(string Prefix)
        {
            Context.Server.Prefix = Prefix;
            return SaveAsync();
        }

        [Command("Set Channel"), Summary("Sets channel for Join/Leave/Chatterbot. To remove channel, don't provide channel name.")]
        public Task SetChannelAsync(ModuleEnums ChannelType, ITextChannel Channel = null)
        {
            switch (ChannelType)
            {
                case ModuleEnums.Chatter: Context.Server.ChatterChannel = $"{Channel.Id}"; break;
                case ModuleEnums.Join: Context.Server.JoinChannel = $"{Channel.Id}"; break;
                case ModuleEnums.Leave: Context.Server.LeaveChannel = $"{Channel.Id}"; break;
                case ModuleEnums.Mod: Context.Server.ModLog.TextChannel = $"{Channel.Id}"; break;
                case ModuleEnums.Starboard: Context.Server.Starboard.TextChannel = $"{Channel.Id}"; break;
            }
            return SaveAsync();
        }

        [Command("Set MaxWarns"), Summary("Sets how many warning a user can get before getting kicked by automod. -1 to disable it.")]
        public Task SetMaxWarnsAsync(int MaxWarnings)
        {
            Context.Server.ModLog.MaxWarnings = MaxWarnings;
            return SaveAsync();
        }

        [Command("Set AutoRole"), Summary("Sets auto role for this server.")]
        public Task SetAutoRoleAsync(IRole Role)
        {
            Context.Server.ModLog.AutoAssignRole = $"{Role.Id}";
            return SaveAsync();
        }

        [Command("Set LvlUpMsg"), Summary("Sets level up message for when a user level ups. To remove message,  don't provide any level up message.")]
        public Task SetLevelUpMessageAsync([Remainder]string Message = null)
        {
            Context.Server.ChatXP.LevelMessage = Message;
            return SaveAsync();
        }

        [Command("SelfRole"), Alias("SlfR"), Summary("Adds/Removes role to/from self assingable roles. Action: Add, Remove")]
        public Task SelfRoleAsync(ModuleEnums Action, IRole Role)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.AssignableRoles.Contains($"{Role.Id}")) return ReplyAsync($"{Role.Name} already exists in assignable roles.");
                    Context.Server.AssignableRoles.Add($"{Role.Id}");
                    return SaveAsync();
                case ModuleEnums.Remove:
                    if (!Context.Server.AssignableRoles.Contains($"{Role.Id}")) return ReplyAsync($"{Role.Name} doesn't exists in assignable roles.");
                    Context.Server.AssignableRoles.Remove($"{Role.Id}");
                    return SaveAsync();
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
                    if (Context.Server.JoinMessages.Count == 5) return ReplyAsync("Can't have more than 5 join messages.");
                    else if (Context.Server.JoinMessages.Contains(Message)) return ReplyAsync("Join message already exists. Try something different?");
                    Context.Server.JoinMessages.Add(Message);
                    return SaveAsync();
                case ModuleEnums.Remove:
                    if (!Context.Server.JoinMessages.Contains(Message)) return ReplyAsync("I couldn't find the specified join message.");
                    Context.Server.JoinMessages.Remove(Message);
                    return SaveAsync();
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
                    if (Context.Server.LeaveMessages.Count == 5) return ReplyAsync("Can't have more than 5 leave messages.");
                    else if (Context.Server.LeaveMessages.Contains(Message)) return ReplyAsync("Leave message already exists. Try something different?");
                    Context.Server.LeaveMessages.Add(Message);
                    return SaveAsync();
                case ModuleEnums.Remove:
                    if (!Context.Server.LeaveMessages.Contains(Message)) return ReplyAsync("I couldn't find the specified leave message.");
                    Context.Server.LeaveMessages.Remove(Message);
                    return SaveAsync();
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
                    return SaveAsync($"{Action} has been {XpState}");
                case ModuleEnums.AutoMod:
                    Context.Server.ModLog.IsAutoModEnabled = !Context.Server.ModLog.IsAutoModEnabled;
                    string ModState = Context.Server.ChatXP.IsEnabled ? "Enabled" : "Disabled";
                    return SaveAsync($"{Action} has been {ModState}");
            }
            return Task.CompletedTask;
        }

        [Command("Admins"), Summary("Adds/Removes users from server's admins. Admins are able to change bot's server configuration.")]
        public Task AdminsAsync(ModuleEnums Action, IGuildUser User)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.Admins.Contains(User.Id)) return ReplyAsync($"{User} is already an Admin.");
                    else if (Context.Server.Admins.Count >= 10) return ReplyAsync($"Can't have more than 10 Admins.");
                    Context.Server.Admins.Add(User.Id);
                    return SaveAsync();
                case ModuleEnums.Remove:
                    if (!Context.Server.Admins.Contains(User.Id)) return ReplyAsync($"{User} isn't an Admin.");
                    Context.Server.Admins.Remove(User.Id);
                    return SaveAsync();
            }
            return Task.CompletedTask;
        }

        [Command("XpForbid"), Summary("Forbids a role from gaining XP.")]
        public Task ForbidAsync(ModuleEnums Action, IRole Role)
        {
            return Task.CompletedTask;
        }

    }
}