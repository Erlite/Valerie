using Discord;
using System.Linq;
using Discord.Commands;
using Valerie.Attributes;
using Valerie.Extensions;
using Discord.WebSocket;
using Valerie.JsonModels;
using Valerie.Modules.Addons;
using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules
{
    [Name("Admin/Server Owner Commands"), RequireAccess(AccessLevel.Admins), RequireBotPermission(ChannelPermission.SendMessages)]
    public class AdminModule : ValerieBase
    {
        [Command("Set Prefix"), Summary("Updates current server's prefix.")]
        public Task PrefixAsync(string Prefix)
        {
            Context.Server.Prefix = Prefix;
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Set Channel"), Summary("Sets channel for Join/Leave/Chatter/Mod/Reddit/Starboard. To remove channel, don't provide channel name.")]
        public Task SetChannelAsync(ModuleEnums ChannelType, ITextChannel Channel = null)
        {
            string ChannelId = $"{Channel?.Id}" ?? string.Empty;
            switch (ChannelType)
            {
                case ModuleEnums.Join: Context.Server.JoinChannel = ChannelId; break;
                case ModuleEnums.Leave: Context.Server.LeaveChannel = ChannelId; break;
                case ModuleEnums.Chatter: Context.Server.ChatterChannel = ChannelId; break;
                case ModuleEnums.Mod: Context.Server.ModLog.TextChannel = ChannelId; break;
                case ModuleEnums.Reddit: Context.Server.Reddit.TextChannel = ChannelId; break;
                case ModuleEnums.Starboard: Context.Server.Starboard.TextChannel = ChannelId; break;
            }
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Set MaxWarns"), Summary("Sets how many warning a user can get before getting kicked by automod. -1 to disable it.")]
        public Task SetMaxWarnsAsync(int MaxWarnings)
        {
            Context.Server.ModLog.MaxWarnings = MaxWarnings;
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Set AutoRole"), Summary("Sets auto role for this server.")]
        public Task SetAutoRoleAsync(IRole Role)
        {
            Context.Server.ModLog.AutoAssignRole = $"{Role.Id}";
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("Set LvlUpMsg"), Summary("Sets level up message for when a user level ups. To remove message,  don't provide any level up message.")]
        public Task SetLevelUpMessageAsync([Remainder]string Message = null)
        {
            Context.Server.ChatXP.LevelMessage = Message;
            return SaveAsync(ModuleEnums.Server);
        }

        [Command("SelfRoles"), Alias("SlfR"), Summary("Adds/Removes role to/from self assingable roles. Action: Add, Remove")]
        public Task SelfRoleAsync(ModuleEnums Action, IRole Role)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.AssignableRoles.Contains(Role.Id)) return ReplyAsync($"{Role.Name} already exists in assignable roles.");
                    else if (Context.Server.AssignableRoles.Count == Context.Server.AssignableRoles.Capacity)
                        return ReplyAsync($"It seems you have reached max number of assignable roles.");
                    Context.Server.AssignableRoles.Add(Role.Id);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.AssignableRoles.Contains(Role.Id)) return ReplyAsync($"I couldn't find  {Role.Name} role.");
                    Context.Server.AssignableRoles.Remove(Role.Id);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("JoinMsg"), Alias("JMsg"),
            Summary("Add/Removes join message. Action: Add, Remove. {user} to mention user. {guild} to print server name.")]
        public Task JoinAsync(ModuleEnums Action, [Remainder] string Message)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.JoinMessages.Count == Context.Server.JoinMessages.Capacity) return ReplyAsync("You have reached max number of join messages.");
                    else if (Context.Server.JoinMessages.Contains(Message)) return ReplyAsync("Join message already exists. Try something different?");
                    Context.Server.JoinMessages.Add(Message);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.JoinMessages.Contains(Message)) return ReplyAsync("I couldn't find the specified join message.");
                    Context.Server.JoinMessages.Remove(Message);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("LeaveMsg"), Alias("LMsg"),
            Summary("Add/Removes leave message. Action: Add, Remove. {user} to mention user. {guild} to print server name.")]
        public Task LeaveAsync(ModuleEnums Action, [Remainder] string Message)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.LeaveMessages.Count == Context.Server.LeaveMessages.Capacity) return ReplyAsync("You have reached max number of leave messages.");
                    else if (Context.Server.LeaveMessages.Contains(Message)) return ReplyAsync("Leave message already exists. Try something different?");
                    Context.Server.LeaveMessages.Add(Message);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.LeaveMessages.Contains(Message)) return ReplyAsync("I couldn't find the specified leave message.");
                    Context.Server.LeaveMessages.Remove(Message);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Toggle"), Summary("Enables/Disables AutoMod/Xp/Feed")]
        public Task ToggleAsync(ModuleEnums Action)
        {
            string State = null;
            switch (Action)
            {
                case ModuleEnums.AutoMod:
                    Context.Server.ModLog.IsAutoModEnabled = !Context.Server.ModLog.IsAutoModEnabled;
                    State = Context.Server.ModLog.IsAutoModEnabled ? "enabled" : "disabled.";
                    return SaveAsync(ModuleEnums.Server, $"{Action} has been {State}.");
                case ModuleEnums.XP:
                    Context.Server.ChatXP.IsEnabled = !Context.Server.ChatXP.IsEnabled;
                    State = Context.Server.ChatXP.IsEnabled ? "enabled" : "disabled";
                    return SaveAsync(ModuleEnums.Server, $"{Action} has been {State}.");
                case ModuleEnums.Reddit:
                    if (!Context.Server.Reddit.IsEnabled)
                    {
                        Context.RedditService.Start(Context.Guild as SocketGuild);
                        Context.Server.Reddit.IsEnabled = true;
                    }
                    else
                    {
                        Context.RedditService.Stop(Context.Server.Reddit.TextChannel);
                        Context.Server.Reddit.IsEnabled = false;
                    }
                    State = Context.Server.Reddit.IsEnabled ? "enabled" : "disabled";
                    return SaveAsync(ModuleEnums.Server, $"{Action} feed has been {State}.");
            }
            return Task.CompletedTask;
        }

        [Command("Admins"), Summary("Adds/Removes users from server's admins. Action: Add, Remove. Admins are able to change bot's server configuration.")]
        public Task AdminsAsync(ModuleEnums Action, IGuildUser User)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.Profiles.ContainsKey(User.Id) && Context.Server.Profiles[User.Id].IsAdmin) return ReplyAsync($"{User} is already an Admin.");
                    if (Context.Server.Profiles.ContainsKey(User.Id)) Context.Server.Profiles[User.Id].IsAdmin = true;
                    else Context.Server.Profiles.Add(User.Id, new UserProfile { IsAdmin = true });
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.Profiles.ContainsKey(User.Id) || !Context.Server.Profiles[User.Id].IsAdmin) return ReplyAsync($"{User} isn't an Admin.");
                    Context.Server.Profiles[User.Id].IsAdmin = false;
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("XpForbid"), Summary("Forbids a role from gaining XP. Action: Add, Remove")]
        public Task ForbidAsync(ModuleEnums Action, IRole Role)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.ChatXP.ForbiddenRoles.Contains(Role.Id)) return ReplyAsync($"{Role} already is forbidden from gaining XP.");
                    else if (Context.Server.ChatXP.ForbiddenRoles.Count == Context.Server.ChatXP.ForbiddenRoles.Capacity)
                        return ReplyAsync("It seems you have reached max number of forbidden roles.");
                    Context.Server.ChatXP.ForbiddenRoles.Add(Role.Id);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.ChatXP.ForbiddenRoles.Contains(Role.Id)) return ReplyAsync($"{Role} isn't forbidden from gaining XP.");
                    Context.Server.ChatXP.ForbiddenRoles.Remove(Role.Id);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("XpLevel"), Summary("Adds/Removes a level up role. Action: Add, Remove, Modify")]
        public Task LevelAsync(ModuleEnums Action, IRole Role, int Level = 10)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.ChatXP.LevelRoles.Count == 20) return ReplyAsync("You have reached max number of level up roles.");
                    else if (Context.Server.ChatXP.LevelRoles.ContainsKey(Role.Id)) return ReplyAsync($"{Role} is already a level-up role.");
                    Context.Server.ChatXP.LevelRoles.Add(Role.Id, Level);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.ChatXP.LevelRoles.ContainsKey(Role.Id)) return ReplyAsync($"{Role} isn't a level-up role.");
                    Context.Server.ChatXP.LevelRoles.Remove(Role.Id);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Modify:
                    if (!Context.Server.ChatXP.LevelRoles.ContainsKey(Role.Id)) return ReplyAsync($"{Role} isn't a level-up role.");
                    Context.Server.ChatXP.LevelRoles[Role.Id] = Level;
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Badwords"), Summary("Add/remove bad words. If a user says a bad word their message will be deleted. Action: Add, Remove")]
        public Task BadWordsAsync(ModuleEnums Action, string Badword)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.ModLog.BadWords.Count == Context.Server.ModLog.BadWords.Capacity)
                        return ReplyAsync("You have reached max number of bad words.");
                    else if (Context.Server.ModLog.BadWords.Contains(Badword)) return ReplyAsync($"{Badword} already exists.");
                    Context.Server.ModLog.BadWords.Add(Badword);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.ModLog.BadWords.Contains(Badword)) return ReplyAsync($"{Badword} doesn't exists.");
                    Context.Server.ModLog.BadWords.Remove(Badword);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Blocklink"), Alias("Add/removes a link from blocked urls. Users will be warned for posting a blocked link. Action: Add, Remove")]
        public Task BlocklinkAsync(ModuleEnums Action, string Url)
        {
            var Check = BoolExt.IsValidUrl(Url);
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (!Check.Item1) return ReplyAsync($"{Url} isn't a valid Url.");
                    else if (Context.Server.ModLog.BlockedUrls.Contains(Check.Item2)) return ReplyAsync($"{Url} already is blocked.");
                    else if (Context.Server.ModLog.BlockedUrls.Count == Context.Server.ModLog.BlockedUrls.Capacity)
                        return ReplyAsync($"You have reached max number of blocked links.");
                    Context.Server.ModLog.BlockedUrls.Add(Check.Item2);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.ModLog.BlockedUrls.Contains(Check.Item2)) return ReplyAsync($"{Url} isn't blocked.");
                    Context.Server.ModLog.BlockedUrls.Remove(Check.Item2);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Subreddit"), Summary("Add/remove subreddit. You will get live feed from specified subreddits.. Action: Add, Remove")]
        public Task SubAsync(ModuleEnums Action, string Subreddit)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.Reddit.Subreddits.Count == Context.Server.Reddit.Subreddits.Capacity)
                        return ReplyAsync("You have reached max number of subreddits");
                    else if (Context.Server.Reddit.Subreddits.Contains(Subreddit)) return ReplyAsync($"{Subreddit} already exists.");
                    else if (!Context.RedditService.VerifySubredditAsync(Subreddit).Result) return ReplyAsync($"{Subreddit} is an invalid subreddit.");
                    Context.Server.Reddit.Subreddits.Add(Subreddit);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.Reddit.Subreddits.Contains(Subreddit)) return ReplyAsync($"{Subreddit} doesn't exists.");
                    Context.Server.Reddit.Subreddits.Remove(Subreddit);
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Show JoinMsgs"), Alias("Showjms"), Summary("Shows all the join messages for this server.")]
        public Task ShowJoinMsgsAsync()
        {
            if (!Context.Server.JoinMessages.Any()) return ReplyAsync($"{Context.Guild} has no join messages.");
            return ReplyAsync($"**Join Messages**\n{string.Join("\n", $"-> {Context.Server.JoinMessages}")}");
        }

        [Command("Show LeaveMsgs"), Alias("Slms"), Summary("Shows all the join messages for this server.")]
        public Task ShowLeaveMsgsAsync()
        {
            if (!Context.Server.LeaveMessages.Any()) return ReplyAsync($"{Context.Guild} has no leave messages.");
            return ReplyAsync($"**Leave Messages**\n{string.Join("\n", $"-> {Context.Server.LeaveMessages}")}");
        }

        [Command("Show Admins"), Alias("SAs"), Summary("Shows all the current admins for this server.")]
        public async Task ShowAdminsAsync()
        {
            if (!Context.Server.Profiles.Any())
            {
                await ReplyAsync($"{Context.Guild} has no admins.");
                return;
            }
            string Admins = null;
            foreach (var Profile in Context.Server.Profiles.Where(x => x.Value.IsAdmin == true))
                Admins += $"\n-> {Profile.Key} | {await StringExt.CheckUserAsync(Context, Profile.Key)}";
            await ReplyAsync($"**{Context.Guild} Admins**\n{Admins}");
        }

        [Command("Show Forbidden"), Alias("Sfb"), Summary("Shows all the forbidden roles for this server.")]
        public Task ShowForbiddenAsync()
        {
            if (!Context.Server.ChatXP.ForbiddenRoles.Any()) return ReplyAsync($"{Context.Guild} has no forbidden roles.");
            string Roles = null;
            foreach (var Id in Context.Server.ChatXP.ForbiddenRoles)
                Roles += $"-> {Id} | {StringExt.CheckRole(Context, Id)}";
            return ReplyAsync($"**Forbidden Roles**\n{Roles}");
        }

        [Command("Show Levels"), Alias("Slvls"), Summary("Shows all the level up roles for this server.")]
        public Task ShowLevelsAsync()
        {
            if (!Context.Server.ChatXP.LevelRoles.Any()) return ReplyAsync($"{Context.Guild} has no level-up roles.");
            string Roles = null;
            foreach (var Id in Context.Server.ChatXP.LevelRoles.Keys)
                Roles += $"-> {Id} | {StringExt.CheckRole(Context, Id)}";
            return ReplyAsync($"**Level-Up Roles**\n{Roles}");
        }

        [Command("Show Badwords"), Alias("Sbws"), Summary("Shows all the bad words for this server.")]
        public Task ShowBadwordsAsync()
        {
            if (!Context.Server.ModLog.BadWords.Any()) return ReplyAsync($"{Context.Guild} has no prohibited words.");
            return ReplyAsync($"**Bad Words**\n{string.Join(", ", Context.Server.ModLog.BadWords)}");
        }

        [Command("Show BlockedUrls"), Alias("Sbls"), Summary("Shows all the blocked links for this server.")]
        public Task ShowBlockedUrlsAsync()
        {
            if (!Context.Server.ModLog.BlockedUrls.Any()) return ReplyAsync($"{Context.Guild} has no blocked urls.");
            return ReplyAsync($"**Blocked Links**\n{string.Join(", ", Context.Server.ModLog.BlockedUrls)}");
        }

        [Command("Show Subs"), Alias("Ssubs"), Summary("Shows all the subreddits this server is subbed to.")]
        public Task ShowSubsAsync()
        {
            if (!Context.Server.Reddit.Subreddits.Any()) return ReplyAsync($"{Context.Guild} isn't subbed to any subreddits.");
            return ReplyAsync($"**Subbed Subreddits**\n{string.Join(", ", Context.Server.Reddit.Subreddits)}");
        }

        [Command("Setup"), Summary("Set ups Valerie for your Server.")]
        public async Task SetupAsync()
        {
            var Channels = await Context.Guild.GetTextChannelsAsync();
            var SetupMessage = await ReplyAsync($"Initializing *{Context.Guild}'s* config .... ");
            OverwritePermissions Permissions = new OverwritePermissions(sendMessages: PermValue.Deny);
            OverwritePermissions VPermissions = new OverwritePermissions(sendMessages: PermValue.Allow);
            var HasStarboard = Channels.FirstOrDefault(x => x.Name == "starboard");
            var HasMod = Channels.FirstOrDefault(x => x.Name == "logs");
            if (Channels.Contains(HasStarboard)) Context.Server.Starboard.TextChannel = $"{HasStarboard.Id}";
            else
            {
                var Starboard = await Context.Guild.CreateTextChannelAsync("starboard");
                await Starboard.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, Permissions);
                await Starboard.AddPermissionOverwriteAsync(Context.Client.CurrentUser, VPermissions);
                Context.Server.Starboard.TextChannel = $"{Starboard.Id}";
            }
            if (Channels.Contains(HasMod)) Context.Server.ModLog.TextChannel = $"{HasMod.Id}";
            else
            {
                var Mod = await Context.Guild.CreateTextChannelAsync("logs");
                await Mod.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, Permissions);
                await Mod.AddPermissionOverwriteAsync(Context.Client.CurrentUser, VPermissions);
                Context.Server.ModLog.TextChannel = $"{Mod.Id}";
            }
            Context.Server.ChatterChannel = $"{DefaultChannel.Id}";
            Context.Server.JoinChannel = $"{DefaultChannel.Id}";
            Context.Server.LeaveChannel = $"{DefaultChannel.Id}";
            Context.Server.ChatXP.LevelMessage = "👾 Congarts **{user}** on hitting level {level}! You received **{bytes}** bytes.";
            await SaveAsync(ModuleEnums.Server, $"*{Context.Guild}'s* configuration has been completed!");
        }

        ITextChannel DefaultChannel => Task.Run(async () =>
             (await Context.Guild.GetTextChannelsAsync()).FirstOrDefault(x => x.Name.Contains("general") || x.Name.Contains("lobby") || x.Id == Context.Guild.Id)
             ?? await Context.Guild.GetDefaultChannelAsync()).Result;
    }
}