using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Discord.Addons.InteractiveCommands;
using Rick.Models;
using Rick.Handlers;
using Rick.Attributes;

namespace Rick.Modules
{
    [RequireUserPermission(GuildPermission.Administrator), CheckBlacklist]
    public class AdminModule : ModuleBase
    {
        private InteractiveService inter;

        public AdminModule(InteractiveService inte)
        {
            inter = inte;
        }

        [Command("Kick"), Summary("Kick @Username This is a reason"), Remarks("Kicks a user from the guild")]
        public async Task KickAsync(SocketGuildUser user, [Remainder] string reason = "No reason provided by the moderator!")
        {
            if (user == null)
                throw new NullReferenceException("Please mention the user you would like to kick!");

            var gldConfig = GuildModel.GuildConfigs[user.Guild.Id];
            gldConfig.CaseNumber += 1;
            if (gldConfig.UserBannedLogged)
            {
                var embed = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x.Name = Context.User.Username;
                        x.IconUrl = Context.User.GetAvatarUrl();
                    })
                .WithDescription($"**Username: **{user.Username}#{user.Discriminator}\n**Responsilble Mod: **{Context.User}\n**Reason: **{reason}\n**Case Number:** {gldConfig.CaseNumber}")
                .WithColor(new Color(232, 226, 53))
                .WithFooter(x =>
                {
                    x.Text = $"Kicked by {Context.User}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                });
                await ReplyAsync("", embed: embed);
            }
            else
                await ReplyAsync($"***{ user.Username + '#' + user.Discriminator} GOT KICKED*** :ok_hand: ");

            GuildModel.GuildConfigs[user.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
            await user.KickAsync();
        }

        [Command("Ban"), Summary("Ban @Username This is a reason"), Remarks("Bans a user from the guild")]
        public async Task BanAsync(SocketGuildUser user, [Remainder] string reason = "No reason provided by the moderator!")
        {
            if (user == null)
                throw new NullReferenceException("Please mention the user you would like to kick!");

            var gldConfig = GuildModel.GuildConfigs[user.Guild.Id];
            gldConfig.CaseNumber += 1;
            if (gldConfig.UserBannedLogged)
            {
                var embed = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x.Name = $"{user.Username} Banned from {user.Guild.Name}";
                        x.IconUrl = user.GetAvatarUrl();
                    })
                    .WithDescription($"**Username: **{user.Username}#{user.Discriminator}\n**Responsilble Mod: **{Context.User}\n**Reason: **{reason}\n**Case Number:** {gldConfig.CaseNumber}")
                    .WithColor(new Color(206, 47, 47))
                    .WithFooter(x =>
                    {
                        x.Text = $"Banned by {Context.User}";
                        x.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithImageUrl("https://i.redd.it/psv0ndgiqrny.gif");
                var ModChannel = user.Guild.GetChannel(gldConfig.ModChannelID) as ITextChannel;
                await ModChannel.SendMessageAsync("", embed: embed);
            }
            else
                await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT BENT*** :hammer: ");

            GuildModel.GuildConfigs[user.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
            await user.Guild.AddBanAsync(user);
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

        [Command("Mute"), Summary("Mute @User This is a reason"), Remarks("Mutes a user")]
        public async Task MuteAsync(SocketGuildUser user, [Remainder] string reason = "No reason provided by the moderator!")
        {
            if (user == null)
                throw new NullReferenceException("You must mention a user you want to mute!");
            
            var gldConfig = GuildModel.GuildConfigs[user.Guild.Id];
            var GetMuteRole = user.Guild.GetRole(gldConfig.MuteRoleId);
            gldConfig.CaseNumber += 1;

            if (GetMuteRole == null)
                throw new NullReferenceException("Mute Role ID is null! Add Mute Role ID in guild Config!");

            await user.AddRoleAsync(GetMuteRole);
            GuildModel.GuildConfigs[user.Guild.Id] = gldConfig;
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs);
            await ReplyAsync("User has been added to Mute Role!");
        }

    }
}
