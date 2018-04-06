using System;
using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Addons;
using Valerie.Models;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Preconditions;
using System.Threading.Tasks;

namespace Valerie.Modules
{
    [Name("Moderator Commands"), RequirePermission(AccessLevel.MODERATOR), RequireBotPermission(ChannelPermission.SendMessages)]
    public class ModModule : Base
    {
        [Command("Ban"), Summary("Bans a user form the server even if they are not in the server."), RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(ulong UserId, [Remainder] string Reason = null)
        {
            await Context.Guild.AddBanAsync(UserId, 7, Reason ?? "Secert Ban.");
            await ReplyAsync($"***{UserId} got bent.*** :ok_hand:");
        }
    }
}