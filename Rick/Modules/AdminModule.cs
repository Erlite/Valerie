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
    [CheckBlacklist, Permission,
        RequireBotPermission(GuildPermission.Administrator |
        GuildPermission.KickMembers |
        GuildPermission.BanMembers |
        GuildPermission.ManageMessages)]
    public class AdminModule : ModuleBase
    {
        [Command("Kick"), Summary("Kicks user from the guild with a reason."), Remarks("Kick @Username User was spamming!")]
        public async Task KickAsync(SocketGuildUser User, [Remainder] string Reason = "No reason provided by the moderator!")
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var BanChannel = User.Guild.GetChannel(gldConfig.AdminLog.TextChannel) as ITextChannel;
            gldConfig.AdminCases += 1;
            if (gldConfig.AdminLog.IsEnabled && gldConfig.AdminLog.TextChannel != 0)
            {
                var embed = EmbedExtension.Embed(EmbedColors.Red, ThumbUrl: User.GetAvatarUrl(),
                    FooterText: $"Kick Date: { DateTime.Now.ToString()}");
                embed.AddInlineField("Username", User.Username + "#" + User.Discriminator + $" ({User.Id})");
                embed.AddInlineField("Responsible Mod", Context.User.Username);
                embed.AddInlineField("Case Number", gldConfig.AdminCases);
                embed.AddInlineField("Case Type", "Kick");
                embed.AddInlineField("Reason", Reason);
                await BanChannel.SendMessageAsync("", embed: embed);
            }
            await User.KickAsync();
            await ReplyAsync($"***{ User.Username + '#' + User.Discriminator} GOT KICKED*** :ok_hand: ");
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("Ban"), Summary("Bans a user from the guild with a reason."), Remarks("Ban @Username User was spamming!")]
        public async Task BanAsync(SocketGuildUser User, [Remainder] string Reason = "No reason provided by the moderator!")
        {
            var gldConfig = GuildHandler.GuildConfigs[User.Guild.Id];
            gldConfig.AdminCases += 1;
            var BanChannel = User.Guild.GetChannel(gldConfig.AdminLog.TextChannel) as ITextChannel;
            if (gldConfig.AdminLog.IsEnabled && gldConfig.AdminLog.TextChannel != 0)
            {
                var embed = EmbedExtension.Embed(EmbedColors.Red, ThumbUrl: User.GetAvatarUrl(),
                    FooterText: $"Kick Date: { DateTime.Now.ToString()}");
                embed.AddInlineField("Username", User.Username + "#" + User.Discriminator + $" ({User.Id})");
                embed.AddInlineField("Responsible Mod", Context.User.Username + "#" + Context.User.Discriminator);
                embed.AddInlineField("Case Number", gldConfig.AdminCases);
                embed.AddInlineField("Case Type", "Ban");
                embed.AddInlineField("Reason", Reason);
                await BanChannel.SendMessageAsync("", embed: embed);
            }
            await User.Guild.AddBanAsync(User);
            await ReplyAsync($"***{User.Username + '#' + User.Discriminator} GOT BENT*** :hammer: ");
            GuildHandler.GuildConfigs[User.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("Mute"), Summary("Adds user to mute role specified in Guild's Config."), Remarks("Mute @Username")]
        public async Task MuteAsync(SocketGuildUser User)
        {
            var gldConfig = GuildHandler.GuildConfigs[User.Guild.Id];
            IRole MuteRole = User.Guild.GetRole(gldConfig.MuteRoleID);
            if (gldConfig.MuteRoleID == 0 || Context.Guild.GetRole(gldConfig.MuteRoleID) == null)
            {
                var CreateNew = await Context.Guild.CreateRoleAsync("Mute Role", GuildPermissions.None, Color.Default);
                gldConfig.MuteRoleID = CreateNew.Id;
                await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
                await User.AddRoleAsync(MuteRole);
                return;
            }
            await User.AddRoleAsync(MuteRole);
            gldConfig.AdminCases += 1;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync("User has been added to Mute Role!");
        }

        [Command("Delete"), Alias("Del"), Summary("Deletes X amount of messages. Messages can't be old than 2 weeks."), Remarks("Delete 10")]
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

        [Command("Addrole"), Alias("Arole"), Summary("Adds user to the specified role."), Remarks("Addrole @Username @RoleName OR Addrole \"Username\" \"RoleName\"")]
        public async Task AroleAsync(SocketGuildUser User, SocketRole Role)
        {
            await User.AddRoleAsync(Role);
            string Description = $"{User.Username} has been added to {Role.Name}!";
            var embed = EmbedExtension.Embed(EmbedColors.Dark, User.Username, User.GetAvatarUrl(), Description: Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("Removerole"), Alias("Rrole"), Summary("Removes user from the specified role."), Remarks("RemoveRole @Username @RoleName OR Addrole \"Username\" \"RoleName\"")]
        public async Task RemoveRoleAsync(SocketGuildUser User, SocketRole Role)
        {
            await User.RemoveRoleAsync(Role);
            string Description = $"{User.Username} has been removed from {Role.Name}!";
            var embed = EmbedExtension.Embed(EmbedColors.Dark, User.Username, User.GetAvatarUrl(), Description: Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("MoneyShot"), Summary("Gives random Karma to everyone in the Guild's Karma list.")]
        public async Task MoneyShotAsync()
        {
            var Random = new Random();
            int Karma = Random.Next(0, 1000);
            var GldCfg = GuildHandler.GuildConfigs[Context.Guild.Id];

            if (GldCfg.KarmaList.Count <= 0)
            {
                await ReplyAsync("Karma List is empty! Please enable Karma first!");
                return;
            }

            foreach (var Key in GldCfg.KarmaList.Keys.ToList())
            {
                GldCfg.KarmaList[Key] += Karma;
            }
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            if (Karma > 500)
                await ReplyAsync("That was a massive money shot boiiii! :money_mouth: ");
            else
                await ReplyAsync("It was a decent money shot :point_up: ");
        }

        [Command("Clear"), Summary("Clears current Karma and AFK list.")]
        public async Task ClearAsync()
        {
            var GuildConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            GuildConfig.KarmaList.Clear();
            GuildConfig.AFKList.Clear();
            GuildHandler.GuildConfigs[Context.Guild.Id] = GuildConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync("Karma Leaderboard and AFK list has been cleared!");
        }
    }
}
