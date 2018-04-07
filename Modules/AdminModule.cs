using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Helpers;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Preconditions;
using System.Threading.Tasks;
using static Valerie.Addons.Embeds;

namespace Valerie.Modules
{
    [Name("Administrative Commands"), RequirePermission(AccessLevel.ADMINISTRATOR), RequireBotPermission(ChannelPermission.SendMessages)]
    public class AdminModule : Base
    {
        [Command("Settings"), Summary("Displays current guild's settings.")]
        public Task SettingsAsync()
        {
            string Feed = Context.Server.Reddit.IsEnabled ? "Enabled." : "Disabled.";
            string XP = Context.Server.ChatXP.IsEnabled ? "Enabled." : "Disabled.";
            var Embed = GetEmbed(Paint.PaleYellow)
               .WithAuthor($"{Context.Guild} Settings", Context.Guild.IconUrl)
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

        [Command("Setup"), Summary("Set ups Valerie for your Server.")]
        public async Task SetupAsync()
        {
            if (Context.Server.IsConfigured == true)
            {
                await ReplyAsync($"{Context.Guild} has already been configured.");
                return;
            }
            var Channels = await Context.Guild.GetTextChannelsAsync();
            var SetupMessage = await ReplyAsync($"Initializing *{Context.Guild}'s* config .... ");
            OverwritePermissions Permissions = new OverwritePermissions(sendMessages: PermValue.Deny);
            OverwritePermissions VPermissions = new OverwritePermissions(sendMessages: PermValue.Allow);
            var HasStarboard = Channels.FirstOrDefault(x => x.Name == "starboard");
            var HasMod = Channels.FirstOrDefault(x => x.Name == "logs");
            if (Channels.Contains(HasStarboard)) Context.Server.Starboard.TextChannel = HasStarboard.Id;
            else
            {
                var Starboard = await Context.Guild.CreateTextChannelAsync("starboard");
                await Starboard.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, Permissions);
                await Starboard.AddPermissionOverwriteAsync(Context.Client.CurrentUser, VPermissions);
                Context.Server.Starboard.TextChannel = Starboard.Id;
            }
            if (Channels.Contains(HasMod)) Context.Server.Mod.TextChannel = HasMod.Id;
            else
            {
                var Mod = await Context.Guild.CreateTextChannelAsync("logs");
                await Mod.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, Permissions);
                await Mod.AddPermissionOverwriteAsync(Context.Client.CurrentUser, VPermissions);
                Context.Server.Mod.TextChannel = Mod.Id;
            }
            Context.Server.ChatterChannel = Context.GuildHelper.DefaultChannel(Context.Guild.Id).Id;
            Context.Server.JoinChannel = Context.GuildHelper.DefaultChannel(Context.Guild.Id).Id;
            Context.Server.LeaveChannel = Context.GuildHelper.DefaultChannel(Context.Guild.Id).Id;
            Context.Server.ChatXP.LevelMessage = "👾 Congrats **{user}** on hitting level {level}! You received **{crystals}** crystals.";
            Context.Server.JoinMessages.Add("{user} in da houuuuuuseeeee! Turn up!");
            Context.Server.JoinMessages.Add("Whalecum to {guild}, {user}! Make yourself comfy wink wink.");
            Context.Server.LeaveMessages.Add("{user} abandoned us ... Fake frens :((");
            Context.Server.LeaveMessages.Add("Fuck {user} and fuck this guild and fuck all of you!");
            Context.Server.ChatXP.IsEnabled = true;
            Context.Server.IsConfigured = true;
            await ReplyAsync($"Configuration for {Context.Guild} is finished.", Document: DocumentType.Server);
        }

        [Command("Subreddits"), Summary("Shows all the subreddits this server is subbed to.")]
        public Task SubredditsAsync() =>
            !Context.Server.Reddit.Subreddits.Any() ? ReplyAsync($"{Context.Guild} isn't subbed to any subreddits.") :
            ReplyAsync($"**Subbed Subreddits**\n{string.Join(", ", Context.Server.Reddit.Subreddits)}");
    }
}