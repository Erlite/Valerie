using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using GPB.Services;
using System.IO;
using Newtonsoft.Json;
using Discord.Addons.InteractiveCommands;
using Discord.Addons.EmojiTools;
using GPB.Handlers;

namespace GPB.Modules
{
    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : ModuleBase
    {
        private LogService log;
        private InteractiveService inter;

        public AdminModule(LogService Logger, InteractiveService inte)
        {
            log = Logger;
            inter = inte;
        }

        [Command("Kick"), Summary("Kick @Username This is a reason"), Remarks("Kicks a user from the guild")]
        public async Task KickAsync(SocketGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user!");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason");

            var embed = new EmbedBuilder()
                .WithTitle("===== Kicked User =====")
                .WithDescription($"**Username: **{user.Username} || {user.Discriminator}\n**Responsilble Mod: **{Context.User}\n**Reason: **{reason}")
                .WithAuthor(x =>
                {
                    x.Name = Context.User.Mention;
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithColor(new Color(232, 226, 53))
                .WithFooter(x =>
                {
                    x.Text = $"Kicked by {Context.User}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                });
            var ModLog = await Context.Client.GetChannelAsync(log.ModLogChannelId) as ITextChannel;
            await ModLog.SendMessageAsync("", embed: embed);
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT KICKED*** :ok_hand: ");
            await user.KickAsync();
        }

        [Command("Ban"), Summary("Ban @Username This is a reason"), Remarks("Bans a user from the guild")]
        public async Task BanAsync(SocketGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user!");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason");

            var gld = Context.Guild as SocketGuild;

            var embed = new EmbedBuilder();
            embed.Color = new Color(206, 47, 47);
            embed.Title = "=== Banned User ===";
            embed.Description = $"**Username: ** {user.Username} || {user.Discriminator}\n**Responsilbe Mod: ** {Context.User}\n**Reason: **{reason}";
            embed.ImageUrl = "https://i.redd.it/psv0ndgiqrny.gif";
            var ModLog = await Context.Client.GetChannelAsync(log.ModLogChannelId) as ITextChannel;
            await ModLog.SendMessageAsync("", embed: embed);
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT BENT*** :hammer: ");
            await gld.AddBanAsync(user);
        }

        [Command("Delete"), Summary("Delete 10"), Remarks("Deletes X amount of messages"), Alias("Del")]
        [RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
        public async Task DeleteAsync(int range = 0)
        {
            if (range <= 0)
                throw new ArgumentException("Enter a valid amount");

            var messageList = await Context.Channel.GetMessagesAsync(range).Flatten();
            await Context.Channel.DeleteMessagesAsync(messageList);
            var embed = new EmbedBuilder();
            embed.Title = "Messages Deleted";
            embed.Description = $"I've deleted {range} messages.";
            embed.Color = new Color(191, 30, 60);
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("Gift")]
        public async Task Gift(double points)
        {
            var guild = Context.Guild;
            var configs = await GiftsHandler.GetAll();
            uint givePoints = points > uint.MaxValue ? uint.MaxValue : (uint)points;
            foreach (var config in configs)
            {
                config.GivePoints(Context.Guild.Id, givePoints);
            }
            await ReplyAsync($"Gifted {points} XP to {configs.Count} users.");
        }
    }
}