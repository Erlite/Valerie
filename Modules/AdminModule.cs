using System;
using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Helpers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static Valerie.Addons.Embeds;

namespace Valerie.Modules
{
    [Name("Administrative Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class AdminModule : Base
    {
        [Command("Settings"), Summary("Displays current guild's settings.")]
        public Task SettingsAsync()
        {
            string Feed = Context.Server.Reddit.IsEnabled ? "Enabled." : "Disabled.";
            string XP = Context.Server.ChatXP.IsEnabled ? "Enabled." : "Disabled.";
            var Embed = GetEmbed(Paint.PaleYellow)
               .WithAuthor(x =>
               {
                   x.Name = $"{Context.Guild} Settings";
                   x.IconUrl = Context.Guild.IconUrl;
               })
               .AddField("General Information",
                $"```ebnf\n" +
                $"Prefix                : {Context.Server.Prefix}\n" +
                $"Log Channel           : {StringHelper.CheckChannel(Context.Guild as SocketGuild, Context.Server.Mod.TextChannel)}\n" +
                $"Join Channel          : {StringHelper.CheckChannel(Context.Guild as SocketGuild, Context.Server.JoinChannel)}\n" +
                $"Leave Channel         : {StringHelper.CheckChannel(Context.Guild as SocketGuild, Context.Server.LeaveChannel)}\n" +
                $"Reddit Channel        : {StringHelper.CheckChannel(Context.Guild as SocketGuild, Context.Server.Reddit.TextChannel)}\n" +
                $"Starboard Channel     : {StringHelper.CheckChannel(Context.Guild as SocketGuild, Context.Server.Starboard.TextChannel)}\n" +
                $"Chatterbot Channel    : {StringHelper.CheckChannel(Context.Guild as SocketGuild, Context.Server.ChatterChannel)}\n" +
                $"Join Messages         : {Context.Server.JoinMessages.Count}\n" +
                $"Leave Messages        : {Context.Server.LeaveMessages.Count}\n" +
                $"AFK Users             : {Context.Server.AFK.Count}\n" +
                $"Self Assignable Roles : {Context.Server.AssignableRoles.Count}\n" +
                $"```", false)
                .AddField("Admin Information",
                $"```diff\n" +
                $"+ Reddit Feed         : {Feed}\n" +
                $"+ Chat XP             : {XP}\n" +
                $"+ Admins              : {Context.Server.Profiles.Where(x => x.Value.IsAdmin).Count()}\n" +
                $"+ Join Role           : {StringHelper.CheckRole(Context.Guild as SocketGuild, Context.Server.Mod.JoinRole)}\n" +
                $"+ Mute Role           : {StringHelper.CheckRole(Context.Guild as SocketGuild, Context.Server.Mod.MuteRole)}\n" +
                $"+ Subreddits          : {Context.Server.Reddit.Subreddits.Count}\n" +
                $"+ Blocked Words       : {Context.Server.Mod.BlockedWords.Count}\n" +
                $"+ Blocked URLS        : {Context.Server.Mod.BlockedUrls.Count}\n" +
                $"+ Max Warnings        : {Context.Server.Mod.MaxWarnings}\n" +
                $"+ Level Up Roles      : {Context.Server.ChatXP.LevelRoles.Count}\n" +
                $"+ Blacklisted Users   : {Context.Server.Profiles.Where(x => x.Value.IsBlacklisted).Count()}\n" +
                $"+ Non-Level Up Roles  : {Context.Server.ChatXP.ForbiddenRoles.Count}\n" +
                $"```", false)
                .AddField("Guild Statistics",
                $"```diff\n" +
                $"- Users Banned        : {Context.Server.Mod.Cases.Where(x => x.CaseType == CaseType.Ban).Count()}\n" +
                $"- Users Kicked        : {Context.Server.Mod.Cases.Where(x => x.CaseType == CaseType.Kick).Count()}\n" +
                $"- Total Chat XP       : {Context.Server.Profiles.Sum(x => x.Value.ChatXP)}\n" +
                $"- Total Tags          : {Context.Server.Tags.Count}\n" +
                $"- Stars Given         : {Context.Server.Starboard.StarboardMessages.Sum(x => x.Stars)}\n" +
                $"- Total Crystals      : {Context.Server.Profiles.Sum(x => x.Value.Crystals)}\n" +
                $"- Total Mod Cases     : {Context.Server.Mod.Cases.Count}\n" +
                $"```", false)
               .Build();
            return ReplyAsync(string.Empty, Embed);
        }
    }
}