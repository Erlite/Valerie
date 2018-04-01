using Discord;
using System.IO;
using Valerie.Addons;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Valerie.Modules
{
    [Name("Valerie's Owner Commands"), RequireOwner]
    public class OwnerModule : Base
    {
        [Command("Update"), Summary("Updates ")]
        public async Task UpdateAsync(Update Update, [Remainder] string Value)
        {
            switch (Update)
            {
                case Update.Avatar:
                    using (var Picture = new FileStream(Value, FileMode.Open))
                        await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(Picture));
                    break;
                case Update.Username:
                    await Context.Client.CurrentUser.ModifyAsync(x => x.Username = Value);
                    break;
            }
            await ReplyAsync($"{Update} has been updated {Emotes.DWink}");
        }

        [Command("GetInvite"), Summary("Makes an invite to the specified guild.")]
        public async Task GetInviteAsync(ulong GuildId)
        {
            var channel = await Context.Client.GetChannelAsync((await Context.Client.GetGuildAsync(GuildId)).DefaultChannelId);
            var invite = await (channel as SocketGuildChannel).CreateInviteAsync(null);
            await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync($"Here is your invite link: <{invite.Url}>");
        }

        public enum Update
        {
            Avatar,
            Username
        }
    }
}