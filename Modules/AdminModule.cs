using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Discord.Addons.InteractiveCommands;
using Rick.Handlers;
using Rick.Attributes;
using Rick.Services;

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

            var gldConfig = GuildHandler.GuildConfigs[user.Guild.Id];
            gldConfig.CaseNumber += 1;
            if (gldConfig.UserBannedLogged)
            {
                var emb = MethodService.AdminEmbed(user, 232, 226, 53, Context.User.Username, Context.User.GetAvatarUrl(), reason, "https://media.tenor.co/images/6c5fc36400b6adcf3d2bcc7bb68677eb/raw");
                var ModChannel = user.Guild.GetChannel(gldConfig.ModChannelID) as ITextChannel;
                await ModChannel.SendMessageAsync("", embed: emb);
            }
            else
                await ReplyAsync($"***{ user.Username + '#' + user.Discriminator} GOT KICKED*** :ok_hand: ");

            GuildHandler.GuildConfigs[user.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await user.KickAsync();
        }

        [Command("Ban"), Summary("Ban @Username This is a reason"), Remarks("Bans a user from the guild")]
        public async Task BanAsync(SocketGuildUser user, [Remainder] string reason = "No reason provided by the moderator!")
        {
            if (user == null)
                throw new NullReferenceException("Please mention the user you would like to kick!");

            var gldConfig = GuildHandler.GuildConfigs[user.Guild.Id];
            gldConfig.CaseNumber += 1;
            if (gldConfig.UserBannedLogged)
            {
                var emb = MethodService.AdminEmbed(user, 206, 46, 47, Context.User.Username, Context.User.GetAvatarUrl(), reason, "https://i.redd.it/psv0ndgiqrny.gif");
                var ModChannel = user.Guild.GetChannel(gldConfig.ModChannelID) as ITextChannel;
                await ModChannel.SendMessageAsync("", embed: emb);
            }
            else
                await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT BENT*** :hammer: ");

            GuildHandler.GuildConfigs[user.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await user.Guild.AddBanAsync(user);
        }

        [Command("Mute"), Summary("Mute @User This is a reason"), Remarks("Mutes a user")]
        public async Task MuteAsync(SocketGuildUser user, [Remainder] string reason = "No reason provided by the moderator!")
        {
            if (user == null)
                throw new NullReferenceException("You must mention a user you want to mute!");

            var gldConfig = GuildHandler.GuildConfigs[user.Guild.Id];
            var GetMuteRole = user.Guild.GetRole(gldConfig.MuteRoleId);
            gldConfig.CaseNumber += 1;

            if (GetMuteRole == null)
                throw new NullReferenceException("Mute Role ID is null! Add Mute Role ID in guild Config!");

            await user.AddRoleAsync(GetMuteRole);
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await ReplyAsync("User has been added to Mute Role!");
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
            await Task.Delay(3000);
            await msg.DeleteAsync();
        }

    }
}
