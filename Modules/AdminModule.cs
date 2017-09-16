using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Valerie.Handlers.Server;
using Valerie.Extensions;
using Valerie.Attributes;
using Valerie.Modules.Enums;
using Valerie.Handlers.Server.Models;

namespace Valerie.Modules
{
    [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.ManageMessages), CustomUserPermission]
    public class AdminModule : ValerieContext
    {
        ServerModel GuildConfig => ServerConfig.ConfigAsync(Context.Guild.Id).GetAwaiter().GetResult();
        ServerModel Config => ServerConfig.Config;

        [Command("Prefix"), Summary("Changes guild's prefix.")]
        public async Task PrefixAsync(string NewPrefix)
        {
            Config.Prefix = NewPrefix;
            await ReplyAsync("Done.");
        }

        [Command("RoleAdd"), Alias("RA"), Summary("Adds a role to assignable role list.")]
        public async Task RoleAddAsync(IRole Role)
        {
            if (Config.AssignableRoles.Contains($"{Role.Id}"))
            {
                await ReplyAsync($"{Role.Name} already exists in assignable roles list.");
                return;
            }
            Config.AssignableRoles.Add($"{Role.Id}");
            await ReplyAsync("Done.");
        }

        [Command("RoleRemove"), Alias("RR"), Summary("Removes a role from assignable role list.")]
        public async Task RoleRemoveAsync(IRole Role)
        {
            if (!Config.AssignableRoles.Contains($"{Role.Id}"))
            {
                await ReplyAsync($"{Role.Name} doesn't exists in assignable roles list.");
                return;
            }
            Config.AssignableRoles.Remove($"{Role.Id}");
            await ReplyAsync("Done.");
        }

        [Command("WelcomeAdd"), Alias("WA"), Summary("Adds a welcome message to welcome messages list.")]
        public async Task WelcomeAddAsync([Remainder] string WelcomeMessage)
        {
            if (Config.WelcomeMessages.Count == 3)
            {
                await ReplyAsync("Can't have more than 3 Welcome Messages.");
                return;
            }
            if (Config.WelcomeMessages.Contains(WelcomeMessage))
            {
                await ReplyAsync("Welcome message already exists.");
                return;
            }
            Config.WelcomeMessages.Add(WelcomeMessage);
            await ReplyAsync("Welcome Message has been added.");
        }

        [Command("WelcomeRemove"), Alias("WR"), Summary("Removes a welcome message from welcome messages list.")]
        public async Task WelcomeRemoveAsync([Remainder] string WelcomeMessage)
        {
            if (!Config.WelcomeMessages.Contains(WelcomeMessage))
            {
                await ReplyAsync("Welcome message doesn't exist.");
                return;
            }
            Config.WelcomeMessages.Remove(WelcomeMessage);
            await ReplyAsync("Welcome Message has been removed");
        }

        [Command("LeaveAdd"), Alias("LA"), Summary("Adds a leave message to leave messages list.")]
        public async Task LeaveAddAsync([Remainder] string LeaveMessage)
        {
            if (Config.LeaveMessages.Count == 3)
            {
                await ReplyAsync("Can't have more than 3 Leave Messages.");
                return;
            }
            if (Config.LeaveMessages.Contains(LeaveMessage))
            {
                await ReplyAsync("Leave message already exists.");
                return;
            }
            Config.LeaveMessages.Add(LeaveMessage);
            await ReplyAsync("Leave Message has been added.");
        }

        [Command("LeaveRemove"), Alias("LR"), Summary("Removes a leave message from leaves message list.")]
        public async Task LeaveRemoveAsync([Remainder] string LeaveMessage)
        {
            if (!Config.LeaveMessages.Contains(LeaveMessage))
            {
                await ReplyAsync("Leave message doesn't exist.");
                return;
            }
            Config.LeaveMessages.Remove(LeaveMessage);
            await ReplyAsync("Leave Message has been removed.");
        }

        [Command("Toggle"), Summary("Enables/Disables various guild's actions. ValueType include: Eridium, NoAds")]
        public async Task ToggleAsync(CommandEnums ValueType)
        {
            switch (ValueType)
            {
                case CommandEnums.Eridium:
                    if (!Config.EridiumHandler.IsEnabled)
                    {
                        Config.EridiumHandler.IsEnabled = true;
                        await ReplyAsync("Eridium has been enabled.");
                    }
                    else
                    {
                        Config.EridiumHandler.IsEnabled = false;
                        await ReplyAsync("Eridium has been disabled.");
                    }
                    break;
                case CommandEnums.NoAds:
                    if (!Config.ModLog.AntiAdvertisement)
                    {
                        Config.ModLog.AntiAdvertisement = true;
                        await ReplyAsync("AntiAdvertisement has been enabled.");
                    }
                    else
                    {
                        Config.ModLog.AntiAdvertisement = false;
                        await ReplyAsync("AntiAdvertisement has been disabled.");
                    }
                    break;
            }
        }

        [Command("EridiumBlacklist"), Summary("Adds/removes a role to/from blacklisted roles"), Alias("EB")]
        public async Task BlacklistRoleAsync(Actions Action, IRole Role)
        {
            switch (Action)
            {
                case Actions.Add:
                    if (Config.EridiumHandler.BlacklistedRoles.Contains($"{Role.Id}"))
                    {
                        await ReplyAsync($"{Role} already exists in roles blacklist."); return;
                    }
                    Config.EridiumHandler.BlacklistedRoles.Add($"{Role.Id}");
                    await ReplyAsync($"{Role} has been added."); break;

                case Actions.Remove:
                    if (!Config.EridiumHandler.BlacklistedRoles.Contains($"{Role.Id}"))
                    {
                        await ReplyAsync($"{Role} doesn't exists in roles blacklist."); return;
                    }
                    Config.EridiumHandler.BlacklistedRoles.Remove($"{Role.Id}");
                    await ReplyAsync($"{Role} has been removed."); break;
            }
        }

        [Command("LevelAdd"), Summary("Adds a level to level up list."), Alias("LA")]
        public Task LevelAddAsync(IRole Role, int Level)
        {
            if (Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                return ReplyAsync($"{Role} already exists in level up roles.");
            }
            Config.EridiumHandler.LevelUpRoles.TryAdd(Role.Id, Level);
            return ReplyAsync($"{Role} has been added.");
        }

        [Command("LevelRemove"), Summary("Removes a role from level up roles"), Alias("LR")]
        public Task EridiumLevelAsync(IRole Role)
        {
            if (!Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                return ReplyAsync($"{Role} doesn't exists in level up roles.");
            }
            Config.EridiumHandler.LevelUpRoles.TryRemove(Role.Id, out int Value);
            return ReplyAsync($"{Role} has been removed.");
        }

        [Command("LevelUpMessage"), Alias("LUM"), Summary("Sets Level Up message.")]
        public async Task LevelUpMessageAsync([Remainder]string Message)
        {
            Config.EridiumHandler.LevelUpMessage = Message;
            await ReplyAsync("Done.");
        }

        [Command("EridiumRemove"), Summary("Removes a user from Eridium list."), Alias("ER")]
        public async Task EridiumRemoveAsync(IGuildUser User)
        {
            if (!Config.EridiumHandler.UsersList.ContainsKey(User.Id))
            {
                await ReplyAsync($"{User} was not found in Eridium list.");
                return;
            }
            Config.EridiumHandler.UsersList.TryRemove(User.Id, out int Value);
            await ReplyAsync($"{User} was removed from Eridium list.");
        }

        [Command("Settings"), Summary("Shows Server Settings.")]
        public async Task SettingsAsync()
        {
            string AutoRole = Context.Guild.GetRole(Convert.ToUInt64(Config.ModLog.AutoAssignRole)) != null ?
                 Context.Guild.GetRole(Convert.ToUInt64(Config.ModLog.AutoAssignRole)).Name : "Unknown Role.";
            string MuteRole = Context.Guild.GetRole(Convert.ToUInt64(Config.ModLog.MuteRole)) != null ?
                 Context.Guild.GetRole(Convert.ToUInt64(Config.ModLog.MuteRole)).Name : "Unknown Role";
            string ModChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.ModLog.TextChannel)) != null ?
                (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.ModLog.TextChannel))).Name : "Unknown Channel.";
            string StarboardChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.Starboard.TextChannel)) != null ?
                (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.Starboard.TextChannel))).Name : "Unknown Channel.";
            string ChatterChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.ChatterChannel)) != null ?
                (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.ChatterChannel))).Name : "Unknown Channel.";
            string JoinChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.JoinChannel)) != null ?
                (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.JoinChannel))).Name : "Unknown Channel.";
            string LeaveChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.JoinChannel)) != null ?
                (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Config.JoinChannel))).Name : "Unknown Channel.";

            string Description =
                $"```diff\n- ======== [Main Information] ======== -\n" +
                $"+ Server's Prefix    : {Config.Prefix}\n" +
                $"+ AFK Entries        : {Config.AFKList.Count}\n" +
                $"+ Todo Entries       : {Config.ToDo.Count}\n" +
                $"+ Tags Entries       : {Config.TagsList.Count}\n" +
                $"+ Join Channel       : {JoinChannel}\n" +
                $"+ Leave Channel      : {LeaveChannel}\n" +
                $"+ Chatter Channel    : {ChatterChannel}\n" +
                $"+ Starboard Channel  : {StarboardChannel}\n" +
                $"+ Starred Messages   : {Config.Starboard.StarboardMessages.Count}\n" +
                $"+ Welcome Messages   : {Config.WelcomeMessages.Count}\n" +
                $"+ Leave Messages     : {Config.LeaveMessages.Count}\n" +
                $"\n- ======== [Mod  Information] ======== -\n" +
                $"+ Auto Assign Role   : {AutoRole}\n" +
                $"+ User Mute Role     : {MuteRole}\n" +
                $"+ Ban/Kick Cases     : {Config.ModLog.Cases}\n" +
                $"+ Auto Mod Enabled   : {Config.ModLog.IsAutoModEnabled}\n" +
                $"+ Mod Channel        : {ModChannel}\n" +
                $"\n- ======== [Eridium  Information] ======== -\n" +
                $"+ Blacklisted Roles  : {Config.EridiumHandler.BlacklistedRoles.Count}\n" +
                $"+ User LevelUp Roles : {Config.EridiumHandler.LevelUpRoles.Count}\n" +
                $"+ Is Eridium Enabled : {Config.EridiumHandler.IsEnabled}\n" +
                $"+ Level Up Message   : {Config.EridiumHandler.LevelUpMessage ?? "No Level Up Message."}\n" +
                $"+ Max LevelUp Level  : {Config.EridiumHandler.MaxRoleLevel}\n" +
                $"```";
            await ReplyAsync(Description);
        }

        [Command("Clear"), Summary("Clears up blacklisted and levelup roles. ClearType: Blacklist, LevelUps")]
        public async Task ClearAsync(CommandEnums ClearType)
        {
            switch (ClearType)
            {
                case CommandEnums.Blacklist:
                    Config.EridiumHandler.BlacklistedRoles.Clear();
                    await ReplyAsync("Blacklisted Roles have been cleared up."); break;
                case CommandEnums.LevelUps:
                    Config.EridiumHandler.LevelUpRoles.Clear();
                    await ReplyAsync("Level Up Roles have been cleared up."); break;
            }
        }

        [Group("Set")]
        public class SetModule : ValerieContext
        {
            ServerModel GuildConfig => ServerConfig.ConfigAsync(Context.Guild.Id).GetAwaiter().GetResult();
            ServerModel Config => ServerConfig.Config;

            [Command("Level"), Summary("Sets Max level for auto roles")]
            public Task LevelAsync(int MaxLevel)
            {
                if (MaxLevel < 10)
                {
                    return ReplyAsync("Max level can't be lower than 10");
                }
                Config.EridiumHandler.MaxRoleLevel = MaxLevel;
                return ReplyAsync($"Max level has been set to: {MaxLevel}");
            }

            [Command("AutoRole"), Summary("Sets auto assign role for when user joins.")]
            public Task AssignRoleAsync(IRole Role)
            {
                Config.ModLog.AutoAssignRole = $"{Role.Id}";
                return ReplyAsync($"**Auto assign role has been set to {Role}** :v:");
            }

            [Command("Channel"), Summary("Sets channel for varios guild's actions. ValueType include: CB, Join, Eridium, Leave, Starboard, Mod.")]
            public async Task ChannelAsync(CommandEnums ValueType, ITextChannel Channel)
            {
                switch (ValueType)
                {
                    case CommandEnums.CB:
                        Config.ChatterChannel = $"{Channel.Id}";
                        await ReplyAsync($"Chatterbot channel has been set to: {Channel.Mention}");
                        break;
                    case CommandEnums.Join:
                        Config.JoinChannel = $"{Channel.Id}";
                        await ReplyAsync($"Join channel has been set to: {Channel.Mention}");
                        break;
                    case CommandEnums.Leave:
                        Config.LeaveChannel = $"{Channel.Id}";
                        await ReplyAsync($"Leave channel has been set to: {Channel.Mention}");
                        break;
                    case CommandEnums.Starboard:
                        Config.Starboard.TextChannel = $"{Channel.Id}";
                        await ReplyAsync($"Starboard channel has been set to: {Channel.Mention}");
                        break;
                    case CommandEnums.Mod:
                        Config.ModLog.TextChannel = $"{Channel.Id}";
                        await ReplyAsync($"Mod channel has been set to: {Channel.Mention}");
                        break;
                }
            }
        }
    }
}
