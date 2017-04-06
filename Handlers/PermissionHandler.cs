using Discord;
using DiscordBot.Interfaces;
using DiscordBot.Enums;
using System.Threading.Tasks;

namespace DiscordBot.Handlers
{
    public class PermissionHandler : IHandler
    {
        private MainHandler MainHandler;
        public PermissionHandler(MainHandler MainHandler)
        {
            this.MainHandler = MainHandler;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public async Task<bool> IsOwnerAsync(IUser user)
        {
            return user.Id == (await MainHandler.Client.GetApplicationInfoAsync()).Owner.Id;
        }

        public async Task<bool> IsAdminAsync(IUser user)
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> IsModAsync(IUser user)
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> IsAtLeastAsync(IUser user, AdminEnum.AdminLevel level)
        {
            if (await IsOwnerAsync(user) && AdminEnum.AdminLevel.OWNER >= level)
                return await Task.FromResult(true);
            if (await IsAdminAsync(user) && AdminEnum.AdminLevel.ADMIN >= level)
                return await Task.FromResult(true);
            if (await IsModAsync(user) && AdminEnum.AdminLevel.MODERATOR >= level)
                return await Task.FromResult(true);
            if (AdminEnum.AdminLevel.USER >= level)
                return await Task.FromResult(true);
            return false;
        }
    }
}
