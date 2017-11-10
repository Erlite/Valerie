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

        [Command("Set Channel"), Summary("Set's channel for Join/Leave/Chatterbot.")]
        public Task SetChannelAsync(ModuleEnums ChannelType, ITextChannel Channel = null)
        {
            switch (ChannelType)
            {
                case ModuleEnums.Chatter: Context.Server.ChatterChannel = $"{Channel.Id}"; return SaveAsync();
                case ModuleEnums.Join: Context.Server.JoinChannel = $"{Channel.Id}"; return SaveAsync();
                case ModuleEnums.Leave: Context.Server.LeaveChannel = $"{Channel.Id}"; return SaveAsync();
                case ModuleEnums.Mod: Context.Server.ModLog.TextChannel = $"{Channel.Id}"; return SaveAsync();
                case ModuleEnums.Starboard: Context.Server.Starboard.TextChannel = $"{Channel.Id}"; return SaveAsync();
            }
            return Task.CompletedTask;
        }

        [Command("Set MaxWarns"), Summary("Set's how many warning a user can get before getting kicked by automod. 0 to disable it.")]
        public Task SetMaxWarnsAsync(int MaxWarnings)
        {
            Context.Server.ModLog.MaxWarnings = MaxWarnings;
            return SaveAsync();
        }

        [Command("Set AutoRole"), Summary("Set's auto role for this server.")]
        public Task SetAutoRoleAsync(IRole Role)
        {
            Context.Server.ModLog.AutoAssignRole = $"{Role.Id}";
            return SaveAsync();
        }
    }
}