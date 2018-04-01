using Discord;
using System.Linq;
using Valerie.Models;
using Valerie.Handlers;
using Discord.WebSocket;

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

        public IMessageChannel DefaultChannel(ulong Id)
        {
            var Guild = Client.GetGuild(Id);
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
    }
}