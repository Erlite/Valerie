using System;
using Discord;
using System.Linq;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Handlers;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Valerie.Helpers
{
    public class GuildHelper
    {
        GuildHandler GuildHandler { get; }
        DiscordSocketClient Client { get; }
        public GuildHelper(GuildHandler guildHandler, DiscordSocketClient client)
        {
            Client = client;
            GuildHandler = guildHandler;
        }

        public IMessageChannel DefaultChannel(ulong GuildId)
        {
            var Guild = Client.GetGuild(GuildId);
            return Guild.TextChannels.FirstOrDefault(x => x.Name.Contains("general") || x.Name.Contains("lobby") || x.Id == Guild.Id) ?? Guild.DefaultChannel;
        }

        public UserProfile GetProfile(ulong GuildId, ulong UserId)
        {
            var Guild = GuildHandler.GetGuild(Client.GetGuild(GuildId).Id);
            if (!Guild.Profiles.ContainsKey(UserId))
            {
                Guild.Profiles.Add(UserId, new UserProfile());
                GuildHandler.Save(Guild);
                return Guild.Profiles[UserId];
            }
            return Guild.Profiles[UserId];
        }

        public void SaveProfile(ulong GuildId, ulong UserId, UserProfile Profile)
        {
            var Config = GuildHandler.GetGuild(GuildId);
            Config.Profiles[UserId] = Profile;
            GuildHandler.Save(Config);
        }

        public async Task LogAsync(IContext Context, IUser User, CaseType CaseType, string Reason)
        {
            var ModChannel = await Context.Guild.GetTextChannelAsync(Context.Server.Mod.TextChannel);
            if (ModChannel == null) return;
            Reason = Reason ?? $"*Responsible moderator, please type `{Context.Config.Prefix}Reason {Context.Server.Mod.Cases.Count + 1} <Reason>`*";
            var Message = await ModChannel.SendMessageAsync($"**{CaseType}** | Case {Context.Server.Mod.Cases.Count + 1}\n**User:** {User} ({User.Id})\n**Reason:** {Reason}\n" +
                    $"**Responsible Moderator:** {Context.User}");
            Context.Server.Mod.Cases.Add(new CaseWrapper()
            {
                Reason = Reason,
                UserId = User.Id,
                CaseType = CaseType,
                MessageId = Message.Id,
                ModId = Context.User.Id,
                CaseNumber = Context.Server.Mod.Cases.Count + 1
            });
        }

        public async Task PurgeAync(IEnumerable<IUserMessage> Messages, ITextChannel Channel, int Amount)
        {
            if (Amount <= 100) await Channel.DeleteMessagesAsync(Messages).ConfigureAwait(false);
            else foreach (var Message in Messages) await Message.DeleteAsync().ConfigureAwait(false);
        }

        public (bool, ulong) GetChannelId(SocketGuild Guild, string Channel)
        {
            UInt64.TryParse(Channel.Replace('<', ' ').Replace('>', ' ').Replace('#', ' ').Replace(" ", ""), out ulong Id);
            return Guild.GetTextChannel(Id) == null ? (false, 0) : (true, Id);
        }

        public (bool, ulong) GetRoleId(SocketGuild Guild, string Role)
        {
            UInt64.TryParse(Role.Replace('<', ' ').Replace('>', ' ').Replace('@', ' ').Replace('&', ' ').Replace(" ", ""), out ulong Id);
            return Guild.GetRole(Id) == null ? (false, 0) : (true, Id);
        }
    }
}