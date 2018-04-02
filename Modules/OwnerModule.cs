using System;
using Discord;
using System.IO;
using Valerie.Enums;
using Valerie.Addons;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Valerie.Modules
{
    [Name("Valerie's Owner Commands"), RequireOwner]
    public class OwnerModule : Base
    {
        [Command("Update"), Summary("Upd")]
        public async Task UpdateAsync(UpdateType Update, [Remainder] string Value)
        {
            switch (Update)
            {
                case UpdateType.Avatar:
                    using (var Picture = new FileStream(Value, FileMode.Open))
                        await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(Picture));
                    break;
                case UpdateType.Username:
                    await Context.Client.CurrentUser.ModifyAsync(x => x.Username = Value);
                    break;
                case UpdateType.Status:
                    var Split = Value.Split(':');
                    await (Context.Client as DiscordSocketClient).SetActivityAsync(
                        new Game(Split[1], (ActivityType)Enum.Parse(typeof(ActivityType), Split[0]))).ConfigureAwait(false);
                    break;
                case UpdateType.Prefix:
                    Context.Config.Prefix = Value;
                    break;
            }
            await ReplyAsync($"{Update} has been updated {Emotes.DWink}", Document: DocumentType.Config);
        }

        [Command("GetInvite"), Summary("Makes an invite for the  specified guild.")]
        public async Task GetInviteAsync(ulong GuildId)
        {
            var channel = await Context.Client.GetChannelAsync((await Context.Client.GetGuildAsync(GuildId)).DefaultChannelId);
            var invite = await (channel as SocketGuildChannel).CreateInviteAsync(null);
            await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync($"Here is your invite link: <{invite.Url}>");
        }
    }
}