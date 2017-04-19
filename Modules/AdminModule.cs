using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Discord.Addons.InteractiveCommands;

namespace Rick.Modules
{
    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : ModuleBase
    {
        private InteractiveService inter;

        public AdminModule(InteractiveService inte)
        {
            inter = inte;
        }

        [Command("Kick"), Summary("Kick @Username This is a reason"), Remarks("Kicks a user from the guild")]
        public async Task KickAsync(SocketGuildUser user = null, [Remainder] string reason = "No reason provided by the moderator!")
        {
            if (user == null)
                throw new NullReferenceException("Please mention the user you would like to kick!");

            var embed = new EmbedBuilder()
                .WithTitle("===== Kicked User =====")
                .WithDescription($"**Username: **{user.Username}#{user.Discriminator}\n**Responsilble Mod: **{Context.User}\n**Reason: **{reason}")
                .WithAuthor(x =>
                {
                    x.Name = Context.User.Username;
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithColor(new Color(232, 226, 53))
                .WithFooter(x =>
                {
                    x.Text = $"Kicked by {Context.User}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                });
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT KICKED*** :ok_hand: ", embed: embed);
            await user.KickAsync();
        }

        [Command("Ban"), Summary("Ban @Username This is a reason"), Remarks("Bans a user from the guild")]
        public async Task BanAsync(SocketGuildUser user = null, [Remainder] string reason = "No reason provided by the moderator!")
        {
            if (user == null)
                throw new NullReferenceException("Please mention the user you would like to kick!");

            var gld = Context.Guild as SocketGuild;
            var embed = new EmbedBuilder()
                .WithTitle("===== Banned User =====")
                .WithDescription($"**Username: **{user.Username}#{user.Discriminator}\n**Responsilble Mod: **{Context.User}\n**Reason: **{reason}")
                .WithAuthor(x =>
                {
                    x.Name = Context.User.Username;
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithColor(new Color(206, 47, 47))
                .WithFooter(x =>
                {
                    x.Text = $"Banned by {Context.User}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithImageUrl("https://i.redd.it/psv0ndgiqrny.gif");
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT BENT*** :hammer: ");
            await gld.AddBanAsync(user);
        }

        [Command("Delete"), Summary("Delete 10"), Remarks("Deletes X amount of messages"), Alias("Del")]
        [RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
        public async Task DeleteAsync(int range = 0)
        {
            if (range <= 0)
                throw new ArgumentException("The amount cannot be lower than or equal to 0!");
            var messageList = await Context.Channel.GetMessagesAsync(range).Flatten();
            await Context.Channel.DeleteMessagesAsync(messageList);
            var msg = await ReplyAsync($"I've deleted {range} messages :ok_hand:");
            await Task.Delay(5000);
            await msg.DeleteAsync();
        }

        //[Command("Gift")]
        //public async Task Gift(double points)
        //{
        //    var guild = Context.Guild;
        //    var configs = await GiftsHandler.GetAll();
        //    uint givePoints = points > uint.MaxValue ? uint.MaxValue : (uint)points;
        //    foreach (var config in configs)
        //    {
        //        config.GivePoints(Context.Guild.Id, givePoints);
        //    }
        //    await ReplyAsync($"Gifted {points} XP to {configs.Count} users.");
        //}
    }
}
