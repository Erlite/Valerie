using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Valerie.Handlers;
using Valerie.Attributes;
using Valerie.Enums;
using Valerie.Extensions;
using Discord.WebSocket;
using System.Text;

namespace Valerie.Modules
{
    [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.ManageMessages), CustomUserPermission]
    public class AdminModule : ValerieBase<ValerieContext>
    {
        [Command("Prefix"), Summary("Changes guild's prefix.")]
        public Task PrefixAsync(string NewPrefix)
        {
            Context.Config.Prefix = NewPrefix;
            return ReactAsync("👌");
        }

        [Command("RoleAdd"), Summary("Adds a role to assignable role list.")]
        public Task RoleAddAsync(IRole Role)
        {
            if (Context.Config.AssignableRoles.Contains($"{Role.Id}"))
                return ReplyAsync($"{Role.Name} already exists in assignable roles list.");
            Context.Config.AssignableRoles.Add($"{Role.Id}");
            return ReactAsync("👌");
        }

        [Command("RoleRemove"), Summary("Removes a role from assignable role list.")]
        public Task RoleRemoveAsync(IRole Role)
        {
            if (!Context.Config.AssignableRoles.Contains($"{Role.Id}"))
                return ReplyAsync($"{Role.Name} doesn't exists in assignable roles list.");
            Context.Config.AssignableRoles.Remove($"{Role.Id}");
            return ReactAsync("👌");
        }

        [Command("WelcomeAdd"),
            Summary("Adds a welcome message to welcome messages. User `{user}` to mention user and `{guild}` for guild name.")]
        public Task WelcomeAddAsync([Remainder] string WelcomeMessage)
        {
            if (Context.Config.WelcomeMessages.Count == 3)
                return ReplyAsync("Can't have more than 3 Welcome Messages.");
            if (Context.Config.WelcomeMessages.Contains(WelcomeMessage))
                return ReplyAsync("Welcome message already exists.");
            Context.Config.WelcomeMessages.Add(WelcomeMessage);
            return ReactAsync("👌");
        }

        [Command("WelcomeRemove"), Summary("Removes a welcome message from welcome messages.")]
        public Task WelcomeRemoveAsync([Remainder] string WelcomeMessage)
        {
            if (!Context.Config.WelcomeMessages.Contains(WelcomeMessage))
                return ReplyAsync("Welcome message doesn't exist.");
            Context.Config.WelcomeMessages.Remove(WelcomeMessage);
            return ReactAsync("👌");
        }

        [Command("LeaveAdd"), Summary("Adds a leave message to leave messages. User `{user}` to mention user and `{guild}` for guild name.")]
        public Task LeaveAddAsync([Remainder] string LeaveMessage)
        {
            if (Context.Config.LeaveMessages.Count == 3)
                return ReplyAsync("Can't have more than 3 leave messages.");
            if (Context.Config.LeaveMessages.Contains(LeaveMessage))
                return ReplyAsync("Leave message already exists.");
            Context.Config.LeaveMessages.Add(LeaveMessage);
            return ReactAsync("👌");
        }

        [Command("LeaveRemove"), Summary("Removes a leave message from leave messages.")]
        public Task LeaveRemoveAsync([Remainder] string LeaveMessage)
        {
            if (!Context.Config.LeaveMessages.Contains(LeaveMessage))
                return ReplyAsync("Leave message doesn't exist.");
            Context.Config.LeaveMessages.Remove(LeaveMessage);
            return ReactAsync("👌");
        }

        [Command("Toggle"), Summary("Enables/Disables various guild's actions. ValueType: Eridium, AutoMod")]
        public async Task ToggleAsync(CommandEnums ValueType)
        {
            switch (ValueType)
            {
                case CommandEnums.Eridium:
                    if (!Context.Config.EridiumHandler.IsEnabled)
                    {
                        Context.Config.EridiumHandler.IsEnabled = true;
                        await ReplyAsync("Eridium has been enabled.");
                    }
                    else
                    {
                        Context.Config.EridiumHandler.IsEnabled = false;
                        await ReplyAsync("Eridium has been disabled.");
                    }
                    break;
                case CommandEnums.AutoMod:
                    if (!Context.Config.ModLog.IsAutoModEnabled)
                    {
                        Context.Config.ModLog.IsAutoModEnabled = true;
                        await ReplyAsync("Auto Mod has been enabled.");
                    }
                    else
                    {
                        Context.Config.ModLog.IsAutoModEnabled = false;
                        await ReplyAsync("Auto Mod has has been disabled.");
                    }
                    break;
            }
        }

        [Command("EridiumBlacklist"), Summary("Adds/removes a role to/from blacklisted roles."), Alias("EB")]
        public async Task BlacklistRoleAsync(Actions Action, IRole Role)
        {
            switch (Action)
            {
                case Actions.Add:
                    if (Context.Config.EridiumHandler.BlacklistedRoles.Contains($"{Role.Id}"))
                    {
                        await ReplyAsync($"{Role} already exists in roles blacklist."); return;
                    }
                    Context.Config.EridiumHandler.BlacklistedRoles.Add($"{Role.Id}");
                    await ReplyAsync($"{Role} has been added."); break;

                case Actions.Delete:
                    if (!Context.Config.EridiumHandler.BlacklistedRoles.Contains($"{Role.Id}"))
                    {
                        await ReplyAsync($"{Role} doesn't exists in roles blacklist."); return;
                    }
                    Context.Config.EridiumHandler.BlacklistedRoles.Remove($"{Role.Id}");
                    await ReplyAsync($"{Role} has been removed."); break;
            }
        }

        [Command("EridiumBlacklist"), Summary("Shows all Blacklisted roles for Eridium."), Alias("EB")]
        public Task BlacklistedRolesAsync()
        {
            if (!Context.Config.EridiumHandler.BlacklistedRoles.Any())
                return ReplyAsync("Woops, there are no blacklisted roles.");
            return ReplyAsync($"**Blacklisted Eridium Roles:** {string.Join(", ", Context.Config.EridiumHandler.BlacklistedRoles.Select(x => StringExtension.IsValidRole(Context, x)))}");
        }

        [Command("Level"), Summary("Adds or Removes a level from Eridium Level Ups. Actions: Add, Remove")]
        public Task LevelsAsync(Actions Action, IRole Role, int Level)
        {
            switch (Action)
            {
                case Actions.Add:
                    if (Context.Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
                        return ReplyAsync($"{Role} already exists in level up roles.");
                    else if (Context.Config.EridiumHandler.LevelUpRoles.Count == 10)
                        return ReplyAsync("You have hit max level up roles.");
                    Context.Config.EridiumHandler.LevelUpRoles.TryAdd(Role.Id, Level);
                    return ReplyAsync($"{Role} has been added.");
                case Actions.Delete:
                    if (!Context.Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
                        return ReplyAsync($"{Role} doesn't exists in level up roles.");
                    Context.Config.EridiumHandler.LevelUpRoles.TryRemove(Role.Id, out Level);
                    return ReplyAsync($"{Role} has been removed.");
            }
            return Task.CompletedTask;
        }

        [Command("Level"), Summary("Shows all the level up roles.")]
        public Task LevelsAsync()
        {
            if (!Context.Config.EridiumHandler.LevelUpRoles.Any())
                return ReplyAsync("Woops, there are no level up roles.");
            var SB = new StringBuilder();
            foreach (var Roles in Context.Config.EridiumHandler.LevelUpRoles)
                SB.AppendLine($"{Roles.Value}                    {StringExtension.IsValidRole(Context, $"{Roles.Key}")}");
            return ReplyAsync($"**Level**            **Role**\n---------------------\n{SB.ToString()}");
        }

        [Command("LevelUpMessage"), Alias("LUM"), Summary("Sets Level Up message for when a user level ups.")]
        public Task LevelUpMessageAsync([Remainder]string Message)
        {
            Context.Config.EridiumHandler.LevelUpMessage = Message;
            return ReactAsync("👌");
        }

        [Command("EridiumRemove"), Summary("Removes a user from Eridium leaderboards.")]
        public async Task EridiumRemoveAsync(IGuildUser User)
        {
            if (!Context.Config.EridiumHandler.UsersList.ContainsKey(User.Id))
            {
                await ReplyAsync($"{User} was not found in Eridium list.");
                return;
            }
            Context.Config.EridiumHandler.UsersList.TryRemove(User.Id, out int Value);
            await ReplyAsync($"{User} was removed from Eridium list.");
        }

        [Command("Settings"), Summary("Shows Server Settings.")]
        public async Task SettingsAsync()
        {
            string Description =
                $"```diff\n- ======== [Main Information] ======== -\n" +
                $"+ Server's Prefix    : {Context.Config.Prefix}\n" +
                $"+ AFK Entries        : {Context.Config.AFKList.Count}\n" +
                $"+ Todo Entries       : {Context.Config.ToDo.Count}\n" +
                $"+ Tags Entries       : {Context.Config.TagsList.Count}\n" +
                $"+ Join Channel       : {StringExtension.IsValidChannel(Context, Context.Config.JoinChannel)}\n" +
                $"+ Leave Channel      : {StringExtension.IsValidChannel(Context, Context.Config.LeaveChannel)}\n" +
                $"+ Chatter Channel    : {StringExtension.IsValidChannel(Context, Context.Config.ChatterChannel)}\n" +
                $"+ Starboard Channel  : {StringExtension.IsValidChannel(Context, Context.Config.Starboard.TextChannel)}\n" +
                $"+ Starred Messages   : {Context.Config.Starboard.StarboardMessages.Count}\n" +
                $"+ Welcome Messages   : {Context.Config.WelcomeMessages.Count}\n" +
                $"+ Leave Messages     : {Context.Config.LeaveMessages.Count}\n" +
                $"\n- ======== [Mod  Information] ======== -\n" +
                $"+ Auto Assign Role   : {StringExtension.IsValidRole(Context, Context.Config.ModLog.AutoAssignRole)}\n" +
                $"+ User Mute Role     : {StringExtension.IsValidRole(Context, Context.Config.ModLog.MuteRole)}\n" +
                $"+ Ban/Kick Cases     : {Context.Config.ModLog.Cases}\n" +
                $"+ Auto Mod Enabled   : {Context.Config.ModLog.IsAutoModEnabled}\n" +
                $"+ Mod Channel        : {StringExtension.IsValidChannel(Context, Context.Config.ModLog.TextChannel)}\n" +
                $"+ Max Warnings       : {Context.Config.ModLog.MaxWarnings}\n" +
                $"+ Warnings           : {Context.Config.ModLog.Warnings.Count}\n" +
                $"\n- ======== [Eridium  Information] ======== -\n" +
                $"+ Blacklisted Roles  : {Context.Config.EridiumHandler.BlacklistedRoles.Count}\n" +
                $"+ User LevelUp Roles : {Context.Config.EridiumHandler.LevelUpRoles.Count}\n" +
                $"+ Is Eridium Enabled : {Context.Config.EridiumHandler.IsEnabled}\n" +
                $"+ Level Up Message   : {Context.Config.EridiumHandler.LevelUpMessage ?? "No Level Up Message."}\n" +
                $"+ Max LevelUp Level  : {Context.Config.EridiumHandler.MaxRoleLevel}\n" +
                $"```";
            await ReplyAsync("", embed: ValerieEmbed.Embed(EmbedColor.Green, Title: $"SETTINGS | {Context.Guild}", Description: Description).Build());
        }

        [Command("Clear"), Summary("Clears up blacklisted and levelup roles. ClearType: Blacklist, LevelUps, Join, Leave, Mod, CB, Starboard, EridiumLevel.")]
        public async Task ClearAsync(CommandEnums ClearType)
        {
            switch (ClearType)
            {
                case CommandEnums.Blacklist:
                    Context.Config.EridiumHandler.BlacklistedRoles.Clear(); await ReplyAsync("Blacklisted Roles have been cleared up."); break;
                case CommandEnums.LevelUps:
                    Context.Config.EridiumHandler.LevelUpRoles.Clear(); await ReplyAsync("Level Up Roles have been cleared up."); break;
                case CommandEnums.Join: Context.Config.JoinChannel = null; await ReplyAsync("Join channel has been cleared."); break;
                case CommandEnums.Leave: Context.Config.LeaveChannel = null; await ReplyAsync("Leave channel has been cleared."); break;
                case CommandEnums.Mod: Context.Config.ModLog.TextChannel = null; await ReplyAsync("Mod channel has been cleared."); break;
                case CommandEnums.CB: Context.Config.ChatterChannel = null; await ReplyAsync("Chatterbot channel has been cleared."); break;
                case CommandEnums.Starboard: Context.Config.Starboard.TextChannel = null; await ReplyAsync("Starboard channel has been cleared."); break;
                case CommandEnums.EridiumLevel: Context.Config.EridiumHandler.LevelUpMessage = null; await ReplyAsync("Eridium message has been cleared."); break;
            }
        }

        [Command("Debug"), Summary("Takes a debug report for your server's Context.Config.")]
        public async Task DebugAsync()
        {
            foreach (var Item in Context.Config.AFKList)
                if (await Context.Guild.GetUserAsync(Item.Key) == null)
                    Context.Config.AFKList.TryRemove(Item.Key, out string UselessValue);

            foreach (var Item in Context.Config.EridiumHandler.UsersList)
                if (await Context.Guild.GetUserAsync(Item.Key) == null)
                    Context.Config.EridiumHandler.UsersList.TryRemove(Item.Key, out int UselessValue);

            foreach (var Item in Context.Config.AssignableRoles)
                if (Context.Guild.GetRole(Convert.ToUInt64(Item)) == null)
                    Context.Config.AssignableRoles.Remove(Item);

            foreach (var Item in Context.Config.EridiumHandler.BlacklistedRoles)
                if (Context.Guild.GetRole(Convert.ToUInt64(Item)) == null)
                    Context.Config.EridiumHandler.BlacklistedRoles.Remove(Item);

            foreach (var Item in Context.Config.EridiumHandler.LevelUpRoles)
                if (Context.Guild.GetRole(Item.Key) == null)
                    Context.Config.EridiumHandler.LevelUpRoles.TryRemove(Item.Key, out int UselessValue);

            await ReplyAsync($"All errors have been cleared up.");
        }

        [Command("MaxWarns"), Summary("Set's Max number of Warnings.")]
        public Task MaxWarnsAsync(int MaxWarns)
        {
            Context.Config.ModLog.MaxWarnings = MaxWarns;
            return ReplyAsync($"Max Warnings has been set to **{MaxWarns}**.");
        }

        [Command("SetLevel"), Summary("Sets Max level for auto roles.")]
        public Task LevelAsync(int MaxLevel)
        {
            if (MaxLevel < 10)
            {
                return ReplyAsync("Max level can't be lower than 10");
            }
            Context.Config.EridiumHandler.MaxRoleLevel = MaxLevel;
            return ReplyAsync($"Max level has been set to: {MaxLevel}");
        }

        [Command("AutoRole"), Summary("Sets auto assign role for when user joins.")]
        public Task AssignRoleAsync(IRole Role)
        {
            Context.Config.ModLog.AutoAssignRole = $"{Role.Id}";
            return ReplyAsync($"**Auto assign role has been set to {Role}** :v:");
        }

        [Command("Channel"), Summary("Sets channel for varios guild's actions. ValueType include: CB, Join, Eridium, Leave, Starboard, Mod.")]
        public async Task ChannelAsync(CommandEnums ValueType, ITextChannel Channel)
        {
            switch (ValueType)
            {
                case CommandEnums.CB:
                    Context.Config.ChatterChannel = $"{Channel.Id}";
                    await ReplyAsync($"Chatterbot channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Join:
                    Context.Config.JoinChannel = $"{Channel.Id}";
                    await ReplyAsync($"Join channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Leave:
                    Context.Config.LeaveChannel = $"{Channel.Id}";
                    await ReplyAsync($"Leave channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Starboard:
                    Context.Config.Starboard.TextChannel = $"{Channel.Id}";
                    await ReplyAsync($"Starboard channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Mod:
                    Context.Config.ModLog.TextChannel = $"{Channel.Id}";
                    await ReplyAsync($"Mod channel has been set to: {Channel.Mention}");
                    break;
            }
        }

        [Command("Setup"), Summary("Set ups Valerie for your Server.")]
        public async Task SetupAsync()
        {
            var Channels = await Context.Guild.GetTextChannelsAsync();
            var SetupMessage = await ReplyAsync($"Initializing *{Context.Guild}'s* config .... ");
            OverwritePermissions Permissions = new OverwritePermissions(sendMessages: PermValue.Deny);
            var HasStarboard = Channels.FirstOrDefault(x => x.Name == "starboard");
            var HasMod = Channels.FirstOrDefault(x => x.Name == "logs");
            if (Channels.Contains(HasStarboard))
                Context.Config.Starboard.TextChannel = $"{HasStarboard.Id}";
            else
            {
                var Starboard = await Context.Guild.CreateTextChannelAsync("starboard");
                await Starboard.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, Permissions);
                Context.Config.Starboard.TextChannel = $"{Starboard.Id}";
            }
            if (Channels.Contains(HasMod))
                Context.Config.ModLog.TextChannel = $"{HasMod}";
            else
            {
                var Mod = await Context.Guild.CreateTextChannelAsync("logs");
                await Mod.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, Permissions);
                Context.Config.ModLog.TextChannel = $"{Mod.Id}";
            }
            Context.Config.ChatterChannel = $"{Context.Guild.DefaultChannelId}";
            Context.Config.JoinChannel = $"{Context.Guild.DefaultChannelId}";
            Context.Config.LeaveChannel = $"{Context.Guild.DefaultChannelId}";
            Context.Config.EridiumHandler.LevelUpMessage = "Congrats on hitting level **{rank}**! :beginner:";
            Context.Config.EridiumHandler.IsEnabled = true;
            await SetupMessage.ModifyAsync(x => x.Content = $"*{Context.Guild}'s* configuration has been completed!");
            await SettingsAsync();
        }
    }
}