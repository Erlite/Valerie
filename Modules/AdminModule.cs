using System;
using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Rick.Handlers;
using Rick.Attributes;
using Rick.Enums;
using Rick.Extensions;

namespace Rick.Modules
{
    [ CheckBlacklist, RequireBotPermission(GuildPermission.Administrator |
        GuildPermission.KickMembers |
        GuildPermission.BanMembers |
        GuildPermission.ManageMessages),
        RequireUserPermission(GuildPermission.KickMembers |
        GuildPermission.BanMembers |
        GuildPermission.ManageMessages)]

    public class AdminModule : ModuleBase
    {
        [Command("Kick"), Summary("Kick @Username This is a reason"), Remarks("Kicks a user from the guild")]
        public async Task KickAsync(SocketGuildUser user, [Remainder] string Reason = "No reason provided by the moderator!")
        {
            await user.KickAsync();            
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var BanChannel = user.Guild.GetChannel(gldConfig.JoinEvent.TextChannel) as ITextChannel;
            gldConfig.CaseNumber += 1;
            if (gldConfig.UserBanned.IsEnabled && gldConfig.UserBanned.TextChannel != 0)
            {
                string description = $"**Username: **{user.Username}#{user.Discriminator}\n**Responsilble Mod: **{Context.User.Username}\n**Reason: **{Reason}\n**Case Number:** {gldConfig.CaseNumber}";
                var embed = EmbedExtension.Embed(EmbedColors.Red, Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl(), Description: description, FooterText: $"Kick Date: { DateTime.Now.ToString()}", ImageUrl: "https://media.tenor.co/images/6c5fc36400b6adcf3d2bcc7bb68677eb/tenor.gif");
                await BanChannel.SendMessageAsync("", embed: embed);
            }
            await ReplyAsync($"***{ user.Username + '#' + user.Discriminator} GOT KICKED*** :ok_hand: ");
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("Ban"), Summary("Ban @Username This is a reason"), Remarks("Bans a user from the guild")]
        public async Task BanAsync(SocketGuildUser user, [Remainder] string reason = "No reason provided by the moderator!")
        {
            var gldConfig = GuildHandler.GuildConfigs[user.Guild.Id];
            await user.Guild.AddBanAsync(user);
            gldConfig.CaseNumber += 1;
            var BanChannel = user.Guild.GetChannel(gldConfig.JoinEvent.TextChannel) as ITextChannel;
            if (gldConfig.UserBanned.IsEnabled && gldConfig.UserBanned.TextChannel != 0)
            {
                string description = $"**Username: **{user.Username}#{user.Discriminator}\n**Responsilble Mod: **{Context.User.Username}\n**Reason: **{reason}\n**Case Number:** {gldConfig.CaseNumber}";
                var embed = EmbedExtension.Embed(EmbedColors.Red, Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl(), Description: description, FooterText: $"Ban Date: { DateTime.Now.ToString()}", ImageUrl: "https://i.redd.it/psv0ndgiqrny.gif");
                await BanChannel.SendMessageAsync("", embed: embed);
            }
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT BENT*** :hammer: ");

            GuildHandler.GuildConfigs[user.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("Mute"), Summary("Mute @User This is a reason"), Remarks("Mutes a user")]
        public async Task MuteAsync(SocketGuildUser user, [Remainder] string reason = "No reason provided by the moderator!")
        {
            var gldConfig = GuildHandler.GuildConfigs[user.Guild.Id];
            var GetMuteRole = user.Guild.GetRole(gldConfig.MuteRoleId);
            if (GetMuteRole == null)
                throw new NullReferenceException("Mute Role ID is null! Add Mute Role ID in guild Config!");
            await user.AddRoleAsync(GetMuteRole);
            gldConfig.CaseNumber += 1;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync("User has been added to Mute Role!");
        }

        [Command("Delete"), Alias("Del"), Summary("Deletes X amount of messages.")]
        public async Task DeleteAsync(int MessageAmount)
        {
            if (MessageAmount <= 0)
            {
                await ReplyAsync("The amount cannot be lower than or equal to 0!");
                return;
            }
            if (MessageAmount > 100)
            {
                await ReplyAsync("Amount can't be higher than 100!");
                return;
            }
            var messageList = await Context.Channel.GetMessagesAsync(MessageAmount).Flatten();
            await Context.Channel.DeleteMessagesAsync(messageList);
        }

        [Command("Addrole"), Summary("Addrole @Username @RoleName"), Remarks("Adds role to a user"), Alias("Arole")]
        public async Task AroleAsync(SocketGuildUser User, SocketRole Role)
        {
            await User.AddRoleAsync(Role);
            string Description = $"{User.Username} has been added to {Role.Name}!";
            var embed = EmbedExtension.Embed(EmbedColors.Dark, User.Username, User.GetAvatarUrl(), Description: Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("Removerole"), Summary("RemoveRole @Username @RoleName"), Remarks("Removes role from a user"), Alias("Rrole")]
        public async Task RemoveRoleAsync(SocketGuildUser User, SocketRole Role)
        {
            await User.RemoveRoleAsync(Role);
            string Description = $"{User.Username} has been removed from {Role.Name}!";
            var embed = EmbedExtension.Embed(EmbedColors.Dark, User.Username, User.GetAvatarUrl(), Description: Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("Antiraid"), Summary("Antiraid"), Remarks("Mutes everyone in the guild")]
        public async Task AntiRadeAsync()
        {
            var GuildConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            IRole MuteRole = Context.Guild.GetRole(GuildConfig.MuteRoleId);
            var GetUsers = await Context.Guild.GetUsersAsync();

            if (GuildConfig.MuteRoleId == 0 || Context.Guild.GetRole(GuildConfig.MuteRoleId) == null)
            {
                var CreateNew = await Context.Guild.CreateRoleAsync("Mute Role", GuildPermissions.None, Color.Default);
                GuildConfig.MuteRoleId = CreateNew.Id;
                await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
                foreach (var user in GetUsers)
                {
                    await user.AddRoleAsync(MuteRole);
                }
                await ReplyAsync("Created mute role and muted everyone!");
            }
            else
            {
                foreach (var user in GetUsers)
                {
                    await user.AddRoleAsync(MuteRole);
                }
                await ReplyAsync("Muted everyone. This guild is protected from raid!");
            }
        }

        [Command("MoneyShot"), Summary("Normal Command"), Remarks("Gives everyone random karma")]
        public async Task MoneyShotAsync()
        {
            var Random = new Random();
            int Karma = Random.Next(100, 500);
            var GldCfg = GuildHandler.GuildConfigs[Context.Guild.Id];
            foreach(var Key in GldCfg.Karma.Keys.ToList())
            {
                GldCfg.Karma[Key] += Karma;
            }
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            if (Karma > 300)
                await ReplyAsync("That was a massive money shot boiiii! :money_mouth: ");
            else
                await ReplyAsync("It was a decent money shot :point_up: ");
        }

        [Command("Clear"), Summary("Normal Command"), Remarks("Clears Karma list")]
        public async Task ClearAsync()
        {
            var GuildConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            GuildConfig.Karma.Clear();
            GuildHandler.GuildConfigs[Context.Guild.Id] = GuildConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync("Karma Leaderboard has been cleared!");
        }
    }
}
