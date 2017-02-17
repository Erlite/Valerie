using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord.Addons.Paginator;
using System.Collections.Generic;
using Nygma.Utils;
using Nygma.Handlers;

namespace Nygma.Modules
{
    public class GenModule : ModuleBase
    {
        private readonly ConfigHandler Config;
        private readonly PaginationService paginator;
        public GenModule(PaginationService Pservice, ConfigHandler con)
        {
            paginator = Pservice;
            Config = con;
        }

        [Command("Me")]
        [Summary("Shows the input as if it's an action you're performing.")]
        public async Task MeCommand([Remainder, Summary("The action you're performing.")] string input)
        {
            EmbedBuilder meEmbed = new EmbedBuilder()
                .WithColor(Misc.RandColor())
                .WithDescription($"{Format.Bold((Context.User as SocketGuildUser)?.Nickname ?? Context.User.Username)} {Format.Italics(input)}");
            await Context.Message.DeleteAsync();
            await ReplyAsync(string.Empty, embed: meEmbed);
        }

        [Command("Embed")]
        public async Task EmbedAsync([Remainder] string msg)
        {
            await Context.Message.DeleteAsync();
            var embed = new EmbedBuilder()
                .WithColor(Misc.RandColor())
                .WithDescription($"{Format.Italics(msg)}");
            await ReplyAsync("", embed: embed);
        }

        [Command("Logs")]
        public async Task LogAsync()
        {
            var chgmsg = await ReplyAsync("Getting Logs");
            var getmsgs = await Context.Channel.GetMessagesAsync().Flatten();
        }

        [Command("Meter")]
        public async Task MeterAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Butt Hurt METER ABHBAHBAHAH")
                .WithImageUrl("http://images-cdn.9gag.com/photo/aVWQx0P_700b.jpg");
            await ReplyAsync("", false, embed);
        }

        [Command("Help")]
        [RequireBotPermission(ChannelPermission.ManageMessages | ChannelPermission.AddReactions)]
        public async Task Paginate()
        {
            var prefix = Config.Prefix;
            var pages = new List<string>
            {
                $"Info:\nHello! Please use the reaction button to navigate through pages.\nThere are some group Commands and those modules will be mentioned as: Example Module (Group Module)\nThis means every commands begins with ?CommandGroup CommandName\nPrefix => {prefix}\n[Command Name] => {prefix}[Usage]",
                "Admin Module:\nKick => Kick @username For being a dick\nBan => Ban @username for being a pussy\nDel => Del 20\nPurge => Purge command ain't working.\nAddRole => Addrole Rolename @username\nRemoveRole => RemoveRole Rolename @username\nPBan => Needs to be tested",
                "Extras Module:\n(Contains Interactive Commands)",
                "General Module:\nMe => Me LOL\nLogs => Logs (Under Dev)\nMeter => Meter\nHelp => Help\nInvite => Invite",
                "Info Module:\nInfo => Info",
                "Owner Module:\nBroadcast => Broadcast [Message Goes here]",
                "Reputation Module: (Group Module) => Rep\nRep Mine\nRep @username\nRep Add @username\nRep Remove @username",
                "Search Module:\nGif => Gif Kittens\nYodify => Yodify [String That needs to be yodified]\nLmgtfy => Lmgtfy [String Goes Here]\nCatFacts => Catfacts\nWiki => Wiki Putin\nDefine => Define LOL\nImage => Image Lol\nUrban => Urban Wtf",
                "Todo Module:\nTodo => Todo\nAddtodo => Addtodo [Items]\nDeltodo => [String]/[Int]",
            };

            await paginator.SendPaginatedMessage(Context.Channel, pages, Context.User, "md");
        }

        [Command("Invite"), Summary("Invite"), Remarks("Gives invite link"), Alias("Iv")]
        public async Task Invite()
        {
            var embed = new EmbedBuilder();
            embed.Color = Misc.RandColor();
            embed.Title = "Invite link";
            embed.Description = $"https://discordapp.com/oauth2/authorize?client_id={Config.ClientID}&scope=bot&permissions={Config.Perms}";
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("GuildInfo"), Summary("GI"), Remarks("Displays information about a guild")]
        public async Task GuildInfo()
        {
            var embed = new EmbedBuilder();
            var gld = Context.Guild;
            if (!string.IsNullOrWhiteSpace(gld.IconUrl))
                embed.ThumbnailUrl = gld.IconUrl;
            var I = gld.Id;
            var O = gld.GetOwnerAsync().GetAwaiter().GetResult().Mention;
            var D = gld.GetDefaultChannelAsync().GetAwaiter().GetResult().Mention;
            var V = gld.VoiceRegionId;
            var C = gld.CreatedAt;
            var A = gld.Available;
            var N = gld.DefaultMessageNotifications;
            var E = gld.IsEmbeddable;
            var L = gld.MfaLevel;
            var R = gld.Roles;
            var VL = gld.VerificationLevel;
            embed.Color = Misc.RandColor();
            embed.Title = $"{gld.Name} Information";
            embed.Description = $"**Guild ID: **{I}\n**Guild Owner: **{O}\n**Default Channel: **{D}\n**Voice Region: **{V}\n**Created At: **{C}\n**Available? **{A}\n" +
                $"**Default Msg Notif: **{N}\n**Embeddable? **{E}\n**MFA Level: **{L}\n**Verification Level: **{VL}\n";
            await ReplyAsync("", false, embed);
        }
    }
}