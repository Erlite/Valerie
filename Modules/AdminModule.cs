﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Valerie.Handlers;
using Valerie.Attributes;
using Valerie.Enums;

namespace Valerie.Modules
{
    [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.ManageMessages), CustomUserPermission]
    public class AdminModule : ValerieBase<ValerieContext>
    {
        [Command("Prefix"), Summary("Changes guild's prefix.")]
        public async Task PrefixAsync(string NewPrefix)
        {
            Context.Config.Prefix = NewPrefix;
            await ReplyAsync("Done.");
        }

        [Command("RoleAdd"), Summary("Adds a role to assignable role list.")]
        public async Task RoleAddAsync(IRole Role)
        {
            if (Context.Config.AssignableRoles.Contains($"{Role.Id}"))
            {
                await ReplyAsync($"{Role.Name} already exists in assignable roles list.");
                return;
            }
            Context.Config.AssignableRoles.Add($"{Role.Id}");
            await ReplyAsync("Done.");
        }

        [Command("RoleRemove"), Summary("Removes a role from assignable role list.")]
        public async Task RoleRemoveAsync(IRole Role)
        {
            if (!Context.Config.AssignableRoles.Contains($"{Role.Id}"))
            {
                await ReplyAsync($"{Role.Name} doesn't exists in assignable roles list.");
                return;
            }
            Context.Config.AssignableRoles.Remove($"{Role.Id}");
            await ReplyAsync("Done.");
        }

        [Command("WelcomeAdd"),
            Summary("Adds a welcome message to welcome messages. User `{user}` to mention user and `{guild}` for guild name.")]
        public async Task WelcomeAddAsync([Remainder] string WelcomeMessage)
        {
            if (Context.Config.WelcomeMessages.Count == 3)
            {
                await ReplyAsync("Can't have more than 3 Welcome Messages.");
                return;
            }
            if (Context.Config.WelcomeMessages.Contains(WelcomeMessage))
            {
                await ReplyAsync("Welcome message already exists.");
                return;
            }
            Context.Config.WelcomeMessages.Add(WelcomeMessage);
            await ReplyAsync("Welcome Message has been added.");
        }

        [Command("WelcomeRemove"), Summary("Removes a welcome message from welcome messages.")]
        public async Task WelcomeRemoveAsync([Remainder] string WelcomeMessage)
        {
            if (!Context.Config.WelcomeMessages.Contains(WelcomeMessage))
            {
                await ReplyAsync("Welcome message doesn't exist.");
                return;
            }
            Context.Config.WelcomeMessages.Remove(WelcomeMessage);
            await ReplyAsync("Welcome Message has been removed");
        }

        [Command("LeaveAdd"), Summary("Adds a leave message to leave messages. User `{user}` to mention user and `{guild}` for guild name.")]
        public async Task LeaveAddAsync([Remainder] string LeaveMessage)
        {
            if (Context.Config.LeaveMessages.Count == 3)
            {
                await ReplyAsync("Can't have more than 3 Leave Messages.");
                return;
            }
            if (Context.Config.LeaveMessages.Contains(LeaveMessage))
            {
                await ReplyAsync("Leave message already exists.");
                return;
            }
            Context.Config.LeaveMessages.Add(LeaveMessage);
            await ReplyAsync("Leave Message has been added.");
        }

        [Command("LeaveRemove"), Summary("Removes a leave message from leave messages.")]
        public async Task LeaveRemoveAsync([Remainder] string LeaveMessage)
        {
            if (!Context.Config.LeaveMessages.Contains(LeaveMessage))
            {
                await ReplyAsync("Leave message doesn't exist.");
                return;
            }
            Context.Config.LeaveMessages.Remove(LeaveMessage);
            await ReplyAsync("Leave Message has been removed.");
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

        [Command("LevelAdd"), Summary("Adds a level to level up list.")]
        public Task LevelAddAsync(IRole Role, int Level)
        {
            if (Context.Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                return ReplyAsync($"{Role} already exists in level up roles.");
            }
            Context.Config.EridiumHandler.LevelUpRoles.TryAdd(Role.Id, Level);
            return ReplyAsync($"{Role} has been added.");
        }

        [Command("LevelRemove"), Summary("Removes a role from level up roles.")]
        public Task EridiumLevelAsync(IRole Role)
        {
            if (!Context.Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                return ReplyAsync($"{Role} doesn't exists in level up roles.");
            }
            Context.Config.EridiumHandler.LevelUpRoles.TryRemove(Role.Id, out int Value);
            return ReplyAsync($"{Role} has been removed.");
        }

        [Command("LevelUpMessage"), Alias("LUM"), Summary("Sets Level Up message for when a user level ups.")]
        public async Task LevelUpMessageAsync([Remainder]string Message)
        {
            Context.Config.EridiumHandler.LevelUpMessage = Message;
            await ReplyAsync("Done.");
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
            string AutoRole = Context.Guild.GetRole(Convert.ToUInt64(Context.Config.ModLog.AutoAssignRole)) != null ?
                 Context.Guild.GetRole(Convert.ToUInt64(Context.Config.ModLog.AutoAssignRole)).Name : "Unknown Role.";
            string MuteRole = Context.Guild.GetRole(Convert.ToUInt64(Context.Config.ModLog.MuteRole)) != null ?
                 Context.Guild.GetRole(Convert.ToUInt64(Context.Config.ModLog.MuteRole)).Name : "Unknown Role";
            string ModChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.ModLog.TextChannel)) != null ?
                (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.ModLog.TextChannel))).Name : "Unknown Channel.";
            string StarboardChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.Starboard.TextChannel)) != null ?
                (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.Starboard.TextChannel))).Name : "Unknown Channel.";
            string ChatterChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.ChatterChannel)) != null ?
                (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.ChatterChannel))).Name : "Unknown Channel.";
            string JoinChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.JoinChannel)) != null ?
                (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.JoinChannel))).Name : "Unknown Channel.";
            string LeaveChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.JoinChannel)) != null ?
                (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Config.JoinChannel))).Name : "Unknown Channel.";

            string Description =
                $"```diff\n" +
                $"+ [{Context.Guild} SETTINGS]\n" +
                $"- ======== [Main Information] ======== -\n" +
                $"+ Server's Prefix    : {Context.Config.Prefix}\n" +
                $"+ AFK Entries        : {Context.Config.AFKList.Count}\n" +
                $"+ Todo Entries       : {Context.Config.ToDo.Count}\n" +
                $"+ Tags Entries       : {Context.Config.TagsList.Count}\n" +
                $"+ Join Channel       : {JoinChannel}\n" +
                $"+ Leave Channel      : {LeaveChannel}\n" +
                $"+ Chatter Channel    : {ChatterChannel}\n" +
                $"+ Starboard Channel  : {StarboardChannel}\n" +
                $"+ Starred Messages   : {Context.Config.Starboard.StarboardMessages.Count}\n" +
                $"+ Welcome Messages   : {Context.Config.WelcomeMessages.Count}\n" +
                $"+ Leave Messages     : {Context.Config.LeaveMessages.Count}\n" +
                $"\n- ======== [Mod  Information] ======== -\n" +
                $"+ Auto Assign Role   : {AutoRole}\n" +
                $"+ User Mute Role     : {MuteRole}\n" +
                $"+ Ban/Kick Cases     : {Context.Config.ModLog.Cases}\n" +
                $"+ Auto Mod Enabled   : {Context.Config.ModLog.IsAutoModEnabled}\n" +
                $"+ Mod Channel        : {ModChannel}\n" +
                $"\n- ======== [Eridium  Information] ======== -\n" +
                $"+ Blacklisted Roles  : {Context.Config.EridiumHandler.BlacklistedRoles.Count}\n" +
                $"+ User LevelUp Roles : {Context.Config.EridiumHandler.LevelUpRoles.Count}\n" +
                $"+ Is Eridium Enabled : {Context.Config.EridiumHandler.IsEnabled}\n" +
                $"+ Level Up Message   : {Context.Config.EridiumHandler.LevelUpMessage ?? "No Level Up Message."}\n" +
                $"+ Max LevelUp Level  : {Context.Config.EridiumHandler.MaxRoleLevel}\n" +
                $"```";
            await ReplyAsync(Description);
        }

        [Command("Clear"),
            Summary("Clears up blacklisted and levelup roles. ClearType: Blacklist, LevelUps, Join, Leave, Mod, CB, Starboard, EridiumLevel.")]
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
            var Reply = await ReplyAsync($"Starting up {Context.Guild}'s diagonastic  ...");
            int AFKErr = 0; int EriErr = 0; int Assignable = 0; int Blacklisted = 0; int Levelups = 0;
            foreach (var Item in Context.Config.AFKList)
            {
                if (await Context.Guild.GetUserAsync(Item.Key) == null)
                {
                    AFKErr += 1;
                    Context.Config.AFKList.TryRemove(Item.Key, out string UselessValue);
                }
            }
            foreach (var Item in Context.Config.EridiumHandler.UsersList)
            {
                if (await Context.Guild.GetUserAsync(Item.Key) == null)
                {
                    EriErr += 1;
                    Context.Config.EridiumHandler.UsersList.TryRemove(Item.Key, out int UselessValue);
                }
            }
            foreach (var Item in Context.Config.AssignableRoles)
            {
                if (Context.Guild.GetRole(Convert.ToUInt64(Item)) == null)
                {
                    Assignable += 1;
                    Context.Config.AssignableRoles.Remove(Item);
                }
            }
            foreach (var Item in Context.Config.EridiumHandler.BlacklistedRoles)
            {
                if (Context.Guild.GetRole(Convert.ToUInt64(Item)) == null)
                {
                    Blacklisted += 1;
                    Context.Config.EridiumHandler.BlacklistedRoles.Remove(Item);
                }
            }
            foreach (var Item in Context.Config.EridiumHandler.LevelUpRoles)
            {
                if (Context.Guild.GetRole(Item.Key) == null)
                {
                    Levelups += 1;
                    Context.Config.EridiumHandler.LevelUpRoles.TryRemove(Item.Key, out int UselessValue);
                }
            }
            string Description = (AFKErr + EriErr + Assignable + Blacklisted + Levelups) != 0 ?
                $"```TOTAL ERRORS      : {AFKErr + EriErr + Assignable + Blacklisted + Levelups}\n" +
                $"ERIDIUM ERRORS    : {EriErr}\n" +
                $"ROLES ERRORS      : {Blacklisted + Levelups + Assignable}\n" +
                $"AFK ERRORS        : {AFKErr}```" : ":blush: No errors were found!";
            await Reply.DeleteAsync();
            await ReplyAsync(Description);
        }

        [Group("Set"), RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.ManageMessages), CustomUserPermission]
        public class SetModule : ValerieBase<ValerieContext>
        {
            [Command("Level"), Summary("Sets Max level for auto roles.")]
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
        }
    }
}
