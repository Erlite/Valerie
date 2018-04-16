using Discord;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Helpers;
using Newtonsoft.Json;
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
        [Command("Settings"), Summary("Displays current server's settings.")]
        public Task SettingsAsync()
        {
            string XP = Context.Server.ChatXP.IsEnabled ? "Enabled." : "Disabled.";
            string Feed = Context.Server.Reddit.IsEnabled ? "Enabled." : "Disabled.";
            string AntiInvite = Context.Server.Mod.AntiInvite ? "Enabled." : "Disabled.";
            string AntiProfanity = Context.Server.Mod.AntiProfanity ? "Enabled." : "Disabled.";

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
                $"+ Join Role           : {StringHelper.CheckRole(Context.Guild as SocketGuild, Context.Server.Mod.JoinRole)}\n" +
                $"+ Mute Role           : {StringHelper.CheckRole(Context.Guild as SocketGuild, Context.Server.Mod.MuteRole)}\n" +
                $"+ Subreddits          : {Context.Server.Reddit.Subreddits.Count}\n" +
                $"+ Profanity Check     : {AntiProfanity}\n" +
                $"+ Invite Check        : {AntiInvite}\n" +
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

        [Command("Setup"), Summary("Set ups Valerie for your server.")]
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

        [Command("Set"), Summary("Sets certain values for current server's config.")]
        public Task SetAsync(SettingType SettingType, [Remainder] string Value)
        {
            var ChannelCheck = Context.GuildHelper.GetChannelId(Context.Guild as SocketGuild, Value);
            var RoleCheck = Context.GuildHelper.GetRoleId(Context.Guild as SocketGuild, Value);
            if ((ChannelCheck.Item1 || RoleCheck.Item1) == false)
                return ReplyAsync($" {Emotes.TickNo} {SettingType} value was provided in incorrect format. If it's a role or channel, try mentioning it?");
            switch (SettingType)
            {
                case SettingType.Prefix: Context.Server.Prefix = Value; break;
                case SettingType.ChatterChannel: Context.Server.ChatterChannel = ChannelCheck.Item2; break;
                case SettingType.JoinChannel: Context.Server.JoinChannel = ChannelCheck.Item2; break;
                case SettingType.LeaveChannel: Context.Server.LeaveChannel = ChannelCheck.Item2; break;
                case SettingType.ModChannel: Context.Server.Mod.TextChannel = ChannelCheck.Item2; break;
                case SettingType.RedditChannel: Context.Server.Reddit.TextChannel = ChannelCheck.Item2; break;
                case SettingType.StarboardChannel: Context.Server.Starboard.TextChannel = ChannelCheck.Item2; break;
                case SettingType.JoinRole: Context.Server.Mod.JoinRole = RoleCheck.Item2; break;
                case SettingType.MuteRole: Context.Server.Mod.MuteRole = RoleCheck.Item2; break;
                case SettingType.MaxWarnings: Context.Server.Mod.MaxWarnings = int.TryParse(Value, out int Result) ? Result : 0; break;
            }
            return ReplyAsync($"{SettingType} has been updated {Emotes.DWink}", Document: DocumentType.Server);
        }

        [Command("Set"), Summary("Sets certain values for current server's config.")]
        public Task SetAsync(SettingType SettingType)
        {
            string State = null;
            switch (SettingType)
            {
                case SettingType.ToggleChatXP:
                    Context.Server.ChatXP.IsEnabled = !Context.Server.ChatXP.IsEnabled;
                    State = Context.Server.ChatXP.IsEnabled ? "enabled" : "disabled";
                    break;
                case SettingType.ToggleNSFWFeed: break;
                case SettingType.ToggleRedditFeed:
                    Context.Server.Reddit.IsEnabled = !Context.Server.Reddit.IsEnabled;
                    State = Context.Server.Reddit.IsEnabled ? "enabled" : "disabled";
                    break;
                case SettingType.ToggleAntiInvite:
                    Context.Server.Mod.AntiInvite = !Context.Server.Mod.AntiInvite;
                    State = Context.Server.Mod.AntiInvite ? "enabled" : "disabled";
                    break;
                case SettingType.ToggleAntiProfanity:
                    Context.Server.Mod.AntiProfanity = !Context.Server.Mod.AntiProfanity;
                    State = Context.Server.Mod.AntiProfanity ? "enabled" : "disabled";
                    break;
            }
            return ReplyAsync($"{SettingType} has been {State} {Emotes.DWink}", Document: DocumentType.Server);
        }

        [Command("Export"), Summary("Exports your server config as a json file.")]
        public async Task ExportAsync()
        {
            var Owner = await (Context.Guild as SocketGuild).Owner.GetOrCreateDMChannelAsync();
            if (Context.Guild.OwnerId != Context.User.Id)
            {
                await ReplyAsync($"Requires Server's Owner.");
                return;
            }
            var Serialize = JsonConvert.SerializeObject(Context.Server, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include
            });
            await Owner.SendFileAsync(new MemoryStream(Encoding.Unicode.GetBytes(Serialize)), $"{Context.Guild.Id}-Config.json");
        }

        [Command("Reset"), Summary("Resets your server config.")]
        public Task ResetAsync()
        {
            if (Context.Guild.OwnerId != Context.User.Id) return ReplyAsync($"Requires Server's Owner.");
            return ReplyAsync($"not working.");
        }

        [Command("SelfRoles"), Summary("Adds/Removes role to/from self assingable roles.")]
        public Task SelfRoleAsync(char Action, IRole Role)
        {
            if (Role == Context.Guild.EveryoneRole) return ReplyAsync($"Role can't be everyone role.");
            var Check = Context.GuildHelper.ListCheck(Context.Server.AssignableRoles, Role.Id, Role.Name, "assignable roles");
            switch (Action)
            {
                case 'a':
                    if (!Check.Item1) return ReplyAsync(Check.Item2);
                    Context.Server.AssignableRoles.Add($"{Role.Id}");
                    return ReplyAsync(Check.Item2, Document: DocumentType.Server);
                case 'r':
                    if (!Context.Server.AssignableRoles.Contains($"{Role.Id}")) return ReplyAsync($"{Role.Name} isn't an assignable role {Emotes.PepeSad}");
                    Context.Server.AssignableRoles.Remove($"{Role.Id}");
                    return ReplyAsync($"`{Role.Name}` is no longer an assignable role.", Document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Forbid"), Summary("Forbids a role from gaining XP.")]
        public Task ForbidAsync(char Action, IRole Role)
        {
            if (Role == Context.Guild.EveryoneRole) return ReplyAsync($"Role can't be everyone role.");
            var Check = Context.GuildHelper.ListCheck(Context.Server.ChatXP.ForbiddenRoles, Role.Id, Role.Name, "forbidden roles");
            switch (Action)
            {
                case 'a':
                    if (!Check.Item1) return ReplyAsync(Check.Item2);
                    Context.Server.ChatXP.ForbiddenRoles.Add($"{Role.Id}");
                    return ReplyAsync(Check.Item2, Document: DocumentType.Server);
                case 'r':
                    if (!Context.Server.ChatXP.ForbiddenRoles.Contains($"{Role.Id}")) return ReplyAsync($"{Role} isn't forbidden from gaining XP.");
                    Context.Server.ChatXP.ForbiddenRoles.Remove($"{Role.Id}");
                    return ReplyAsync($"`{Role}` has been removed from forbidden roles.", Document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Level"), Summary("Adds/Removes a level up role.")]
        public Task LevelAsync(char Action, IRole Role, int Level = 10)
        {
            switch (Action)
            {
                case 'a':
                    if (Context.Server.ChatXP.LevelRoles.Count == 20) return ReplyAsync("You have reached max number of level up roles.");
                    else if (Context.Server.ChatXP.LevelRoles.ContainsKey(Role.Id)) return ReplyAsync($"{Role} is already a level-up role.");
                    Context.Server.ChatXP.LevelRoles.Add(Role.Id, Level);
                    return ReplyAsync($"Added `{Role}` role as a levelup role.", Document: DocumentType.Server);
                case 'r':
                    if (!Context.Server.ChatXP.LevelRoles.ContainsKey(Role.Id)) return ReplyAsync($"{Role} isn't a level-up role.");
                    Context.Server.ChatXP.LevelRoles.Remove(Role.Id);
                    return ReplyAsync($"Removed `{Role}`role from levelup roles.", Document: DocumentType.Server);
                case 'm':
                    if (!Context.Server.ChatXP.LevelRoles.ContainsKey(Role.Id)) return ReplyAsync($"{Role} isn't a level-up role.");
                    Context.Server.ChatXP.LevelRoles[Role.Id] = Level;
                    return ReplyAsync($"Modified level up role `{Role}.", Document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("JoinMessages"), Summary("Add/Removes join message. {user} to mention user. {guild} to print server name.")]
        public Task JoinMessagesAsync(char Action, [Remainder] string Message)
        {
            var Check = Context.GuildHelper.ListCheck(Context.Server.JoinMessages, Message, $"```{Message}```", "join messages");
            switch (Action)
            {
                case 'a':
                    if (!Check.Item1) return ReplyAsync(Check.Item2);
                    Context.Server.JoinMessages.Add(Message);
                    return ReplyAsync("Join message has been added.", Document: DocumentType.Server);
                case 'r':
                    if (!Context.Server.JoinMessages.Contains(Message)) return ReplyAsync("I couldn't find the specified join message.");
                    Context.Server.JoinMessages.Remove(Message);
                    return ReplyAsync("Join message has been removed.", Document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("LeaveMessages"), Summary("Add/Removes leave message. {user} to mention user. {guild} to print server name.")]
        public Task LeaveMessagesAsync(char Action, [Remainder] string Message)
        {
            var Check = Context.GuildHelper.ListCheck(Context.Server.LeaveMessages, Message, $"```{Message}```", "leave messages");
            switch (Action)
            {
                case 'a':
                    if (!Check.Item1) return ReplyAsync(Check.Item2);
                    Context.Server.LeaveMessages.Add(Message);
                    return ReplyAsync("Leave message has been added.", Document: DocumentType.Server);
                case 'r':
                    if (!Context.Server.LeaveMessages.Contains(Message)) return ReplyAsync("I couldn't find the specified leave message.");
                    Context.Server.LeaveMessages.Remove(Message);
                    return ReplyAsync("Leave message has been removed.", Document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Subreddit"), Summary("Add/remove subreddit. You will get live feed from specified subreddits.")]
        public Task SubredditAsync(char Action, string Subreddit)
        {
            var Check = Context.GuildHelper.ListCheck(Context.Server.Reddit.Subreddits, Subreddit, Subreddit, "server's subreddits.");
            switch (Action)
            {
                case 'a':
                    if (!Check.Item1) return ReplyAsync(Check.Item2);
                    Context.Server.Reddit.Subreddits.Add(Subreddit);
                    return ReplyAsync(Check.Item2, Document: DocumentType.Server);
                case 'r':
                    if (!Context.Server.Reddit.Subreddits.Contains(Subreddit)) return ReplyAsync($"You aren't subbed to {Subreddit}.");
                    Context.Server.Reddit.Subreddits.Remove(Subreddit);
                    return ReplyAsync($"Removed {Subreddit} from server's subreddit.", Document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("JoinMessages"), Summary("Shows all the join messages for this server.")]
        public Task JoinMessagesAsync()
            => ReplyAsync(!Context.Server.JoinMessages.Any() ? $"{Context.Server} doesn't have any user join messages {Emotes.PepeSad}" :
                $"**Join Messages**\n{string.Join("\n", $"-> {Context.Server.JoinMessages}")}");

        [Command("LeaveMessages"), Summary("Shows all the join messages for this server.")]
        public Task LeaveMessagesAsync()
            => ReplyAsync(!Context.Server.JoinMessages.Any() ? $"{Context.Server} doesn't have any user leave messages {Emotes.PepeSad} " :
                $"**Leave Messages**\n{string.Join("\n", $"-> {Context.Server.LeaveMessages}")}");

        [Command("Forbid"), Summary("Shows all the forbidden roles for this server.")]
        public Task ForbiddenAsync()
            => ReplyAsync(!Context.Server.ChatXP.ForbiddenRoles.Any() ? $"{Context.Guild} has no forbidden roles." :
                $"**Forbidden Roles:**\n{Context.Server.ChatXP.ForbiddenRoles.Select(x => $"-> {x} | {StringHelper.CheckRole(Context.Guild as SocketGuild, Convert.ToUInt64(x))}")}");

        [Command("Level"), Summary("Shows all the level up roles for this server.")]
        public Task LevelsAsync()
            => ReplyAsync(!Context.Server.ChatXP.LevelRoles.Any() ? $"{Context.Guild} has no level-up roles." :
                $"**Level Up Roles:**\n{Context.Server.ChatXP.LevelRoles.Keys.Select(x => $"-> {x} | {StringHelper.CheckRole(Context.Guild as SocketGuild, x)}")}");

        [Command("Subreddit"), Summary("Shows all the subreddits this server is subbed to.")]
        public Task SubredditAsync()
            => ReplyAsync(!Context.Server.Reddit.Subreddits.Any() ? $"This server isn't subscribed to any subreddits {Emotes.PepeSad}" :
                $"**Subbed To Following Subreddits:** {string.Join(", ", Context.Server.Reddit.Subreddits)}");
    }
}