using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;
using Discord.WebSocket;
using Rick.Models;
using Rick.Attributes;
using System.Text;

namespace Rick.Modules
{
    [Group("Guild"), RequireUserPermission(GuildPermission.Administrator), CheckBlacklist]
    public class GuildModule : ModuleBase
    {
        private GuildHandler model;
        private EventService Log;

        public GuildModule(GuildHandler gld, EventService Logger)
        {
            model = gld;
            Log = Logger;
        }

        [Command("ModChannel"), Summary("ModChannel #ChannelName"), Remarks("Sets the Modchannel to log bans, etc")]
        public async Task SetModLogChannelAsync(ITextChannel channel)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.ModChannelID = channel.Id;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await ReplyAsync($"Mod Channel has been set to **{channel.Name}**");
        }

        [Command("SetPrefix"), Summary("SetPrefix ?"), Remarks("Sets Guild prefix")]
        public async Task SetPrefixAsync(string prefix)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.GuildPrefix = prefix;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await ReplyAsync($"Guild Prefix has been set to: **{prefix}**");
        }

        [Command("WelcomeMsg"), Summary("WelcomeMsg This is a welcome Msg"), Remarks("Sets welcome message for your server")]
        public async Task WelcomeMessageAsync([Remainder]string msg)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.WelcomeMessage = msg;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await ReplyAsync($"Guild Welcome Message has been set to:\n```{msg}```");
        }

        [Command("Actions"), Summary("Normal Command"), Remarks("Shows what Actions are being logged")]
        public async Task ListLogActionsAsync()
        {
            var GConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var Joins = GConfig.JoinLogs ? "Enabled" : "Disabled";
            var Leaves = GConfig.LeaveLogs ? "Enabled" : "Disabled";
            var Username = GConfig.NameChangesLogged ? "Enabled" : "Disabled";
            var Nickname = GConfig.NickChangesLogged ? "Enabled" : "Disabled";
            var Bans = GConfig.UserBannedLogged ? "Enabled" : "Disabled";
            var Karma = GConfig.ChatKarma ? "Enabled" : "Disabled";
            var Chatterbot = GConfig.ChatterBot ? "Enabled" : "Disabled";
            var SB = new StringBuilder();
            foreach(var Names in GConfig.RequiredChannelNames)
            {
                SB.AppendLine(Names);
            }
            if (string.IsNullOrWhiteSpace(SB.ToString()))
                SB = SB.AppendLine("No channels found in required channel list.");
            string Description =
                                $"**Guild Prefix:** {GConfig.GuildPrefix}\n" +
                                $"**Server Mod Channel:** {GConfig.ModChannelID}\n" +
                                $"**Mute Role ID:** {GConfig.MuteRoleId}\n" +
                                $"**Welcome Message:** {GConfig.WelcomeMessage}\n" +
                                $"**User Join Logging:** {Joins}\n" +
                                $"**User Leave Logging:** {Leaves}\n" +
                                $"**Username Change Logging:** {Username}\n" +
                                $"**Nickname Change Logging:** {Nickname}\n" +
                                $"**User Ban Logging:** {Bans}\n" +
                                $"**Chat Karma:** {Karma}\n" +
                                $"**Chatter Bot:** {Chatterbot}\n" +
                                $"**Total Bans/Kicks Cases:** {GConfig.CaseNumber}\n" +
                                $"**AFK Members:** {GConfig.AfkList.Count}\n" +
                                $"**Total Tags:** {GConfig.TagsList.Count}\n" +
                                $"**Required Channels for NSFW:** {SB.ToString()}";
            var embed = EmbedService.Embed(EmbedModel.Teal, $"{Context.Guild.Name} || {(await Context.Guild.GetOwnerAsync()).Username}", Context.Guild.IconUrl, Description: Description, ThumbUrl: Context.Guild.IconUrl);
            await ReplyAsync("", embed: embed);
        }

        [Command("ToggleJoins"), Summary("Normal Command"), Remarks("Toggles Join logging")]
        public async Task ToggleJoinsAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.JoinLogs)
            {
                Log.EnableJoinLogging();
                gldConfig.JoinLogs = true;
                await ReplyAsync(":gear:   Now logging joins.");
            }
            else
            {
                Log.DisableJoinLogging();
                gldConfig.JoinLogs = false;
                await ReplyAsync(":skull_crossbones:   No longer logging joins.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("ToggleLeaves"), Summary("Normal Command"), Remarks("Toggle Leaves logging")]
        public async Task ToggleLeavesAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.LeaveLogs)
            {
                gldConfig.LeaveLogs = true;
                Log.EnableLeaveLogging();
                await ReplyAsync(":gear:   Now logging leaves.");
            }
            else
            {
                gldConfig.LeaveLogs = false;
                Log.DisableLeaveLogging();
                await ReplyAsync(":skull_crossbones:  No longer logging leaves.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("ToggleUsername"), Summary("Normal Command"), Remarks("Toggles Name change logging")]
        public async Task ToggleUsernamesAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.NameChangesLogged)
            {
                gldConfig.NameChangesLogged = true;
                Log.EnableNameChangeLogging();
                await ReplyAsync(":gear:   Now logging username changes.");
            }
            else
            {
                gldConfig.NameChangesLogged = false;
                Log.DisableNameChangeLogging();
                await ReplyAsync(":skull_crossbones:  No longer logging username changes.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("ToggleNicknames"), Summary("Normal Command"), Remarks("Toggles Nickname changes loggig")]
        public async Task ToggleNicknamesAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.NickChangesLogged)
            {
                gldConfig.NickChangesLogged = true;
                Log.EnableNickChangeLogging();
                await ReplyAsync(":gear:   Now logging nickname changes.");
            }
            else
            {
                gldConfig.NickChangesLogged = false;
                Log.DisableNickChangeLogging();
                await ReplyAsync(":skull_crossbones:   No longer logging nickname changes.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("ToggleBans"), Summary("Normal Command"), Remarks("Toggles ban logging")]
        public async Task ToggleBansAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.UserBannedLogged)
            {
                gldConfig.UserBannedLogged = true;
                await ReplyAsync(":gear:   Now logging bans.");
            }
            else
            {
                gldConfig.UserBannedLogged = false;
                await ReplyAsync(":skull_crossbones:  No longer logging bans.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("Channel"), Summary("Channel Add #ChannelName/Channel AddId #ChannelName"), Remarks("Adds/Removes channel names/ids from the list")]
        public async Task ChannelAsync(GlobalModel Prop, ITextChannel channel)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            switch (Prop)
            {
                case GlobalModel.Add:
                    gldConfig.RequiredChannelNames.Add(channel.Name);
                    await ReplyAsync($"Channel **{channel.Name}** has been added to RequiredChannel Attribute.");
                    break;

                case GlobalModel.Remove:
                    gldConfig.RequiredChannelNames.Remove(channel.Name);
                    await ReplyAsync($"Channel **{channel.Name}** has been removed from RequiredChannel Attribute.");
                    break;

                case GlobalModel.AddId:
                    gldConfig.RequiredRoleIDs.Add(channel.Id);
                    await ReplyAsync($"Channel **{channel.Id}** has been added to RequiredChannel Attribute.");
                    break;

                case GlobalModel.RemoveId:
                    gldConfig.RequiredRoleIDs.Remove(channel.Id);
                    await ReplyAsync($"Channel **{channel.Id}** has been removed from RequiredChannel Attribute.");
                    break;
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("Role"), Summary("Role AddId RoleName"), Remarks("Adds/Removes role ids from the list")]
        public async Task RoleAsync(GlobalModel Prop, IRole Role)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            switch (Prop)
            {
                case GlobalModel.AddId:
                    gldConfig.RequiredRoleIDs.Add(Role.Id);
                    await ReplyAsync($"Role **{Role.Id}** has been added to RequiredRoleIDs Attribute");
                    break;

                case GlobalModel.RemoveId:
                    gldConfig.RequiredRoleIDs.Remove(Role.Id);
                    await ReplyAsync($"Role **{Role.Id}** has been removed to RequiredRoleIDs Attribute");
                    break;
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("ToggleKarma"), Summary("Normal Command"), Remarks("Toggles Chat Karma")]
        public async Task ToggleKarmaAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.ChatKarma)
            {
                gldConfig.ChatKarma = true;
                await ReplyAsync(":gear: Users will now be awarded random Karma based on their chat activity!");
            }
            else
            {
                gldConfig.ChatKarma = false;
                await ReplyAsync(":skull_crossbones: Auto Karma disabled!.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("ToggleChatterbot"), Summary("Normal Command"), Remarks("Toggles Chatter Bot")]
        public async Task ToggleChatterBotAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.ChatterBot)
            {
                gldConfig.ChatterBot = true;
                await ReplyAsync(":gear: Chatterbot enabled!");
            }
            else
            {
                gldConfig.ChatterBot = false;
                await ReplyAsync(":skull_crossbones: Chatterbot disabled!");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }
    }
}
