﻿using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Discord.Addons.InteractiveCommands;
using Rick.Handlers;
using Rick.Attributes;
using Rick.Services;
using Rick.Classes;

namespace Rick.Modules
{
    [ 
        CheckBlacklist, 
        PermissionCheck
        (
        GuildPermission.Administrator, 
        GuildPermission.BanMembers, 
        GuildPermission.KickMembers, 
        GuildPermission.ManageMessages, 
        GuildPermission.ManageRoles
        )]

    public class AdminModule : ModuleBase
    {
        private InteractiveService inter;

        public AdminModule(InteractiveService inte)
        {
            inter = inte;
        }

        [Command("Kick"), Summary("Kick @Username This is a reason"), Remarks("Kicks a user from the guild")]
        public async Task KickAsync(SocketGuildUser user, [Remainder] string Reason = "No reason provided by the moderator!")
        {
            var gldConfig = GuildHandler.GuildConfigs[user.Guild.Id];
            if (gldConfig.UserBannedLogged)
            {
                string description = $"**Username: **{user.Username}#{user.Discriminator}\n**Responsilble Mod: **{Context.User.Username}\n**Reason: **{Reason}\n**Case Number:** {gldConfig.CaseNumber}";
                var embed = EmbedService.Embed(EmbedColors.Red, Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl(), Description: description, FooterText: $"Kick Date: { DateTime.Now.ToString()}", ImageUrl: "https://media.tenor.co/images/6c5fc36400b6adcf3d2bcc7bb68677eb/tenor.gif");
                var ModChannel = user.Guild.GetChannel(gldConfig.ModChannelID) as ITextChannel;
                await ModChannel.SendMessageAsync("", embed: embed);
            }
            else
                await ReplyAsync($"***{ user.Username + '#' + user.Discriminator} GOT KICKED*** :ok_hand: ");

            await user.KickAsync();
            gldConfig.CaseNumber += 1;
            GuildHandler.GuildConfigs[user.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("Ban"), Summary("Ban @Username This is a reason"), Remarks("Bans a user from the guild")]
        public async Task BanAsync(SocketGuildUser user, [Remainder] string reason = "No reason provided by the moderator!")
        {
            var gldConfig = GuildHandler.GuildConfigs[user.Guild.Id];            
            if (gldConfig.UserBannedLogged)
            {
                string description = $"**Username: **{user.Username}#{user.Discriminator}\n**Responsilble Mod: **{Context.User.Username}\n**Reason: **{reason}\n**Case Number:** {gldConfig.CaseNumber}";
                var embed = EmbedService.Embed(EmbedColors.Red, Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl(), Description: description, FooterText: $"Ban Date: { DateTime.Now.ToString()}", ImageUrl: "https://i.redd.it/psv0ndgiqrny.gif");
                var ModChannel = user.Guild.GetChannel(gldConfig.ModChannelID) as ITextChannel;
                await ModChannel.SendMessageAsync("", embed: embed);
            }
            else
                await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT BENT*** :hammer: ");

            await user.Guild.AddBanAsync(user);
            gldConfig.CaseNumber += 1;
            GuildHandler.GuildConfigs[user.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
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
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await ReplyAsync("User has been added to Mute Role!");
        }

        [Command("Delete"), Summary("Delete 10"), Remarks("Deletes X amount of messages"), Alias("Del")]
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

        [Command("Addrole"), Summary("Addrole @Username @RoleName"), Remarks("Adds role to a user"), Alias("Arole")]
        public async Task AroleAsync(SocketGuildUser User, SocketRole Role)
        {
            await User.AddRoleAsync(Role);
            string Description = $"{User.Username} has been added to {Role.Name}!";
            var embed = EmbedService.Embed(EmbedColors.Dark, User.Username, User.GetAvatarUrl(), Description: Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("Removerole"), Summary("RemoveRole @Username @RoleName"), Remarks("Removes role from a user"), Alias("Rrole")]
        public async Task RemoveRoleAsync(SocketGuildUser User, SocketRole Role)
        {
            await User.RemoveRoleAsync(Role);
            string Description = $"{User.Username} has been removed from {Role.Name}!";
            var embed = EmbedService.Embed(EmbedColors.Dark, User.Username, User.GetAvatarUrl(), Description: Description);
            await ReplyAsync("", embed: embed);
        }
    }
}
