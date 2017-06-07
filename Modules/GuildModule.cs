using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;
using Discord.WebSocket;
using Rick.Enums;
using Rick.Attributes;
using System.Text;
using Rick.Extensions;

namespace Rick.Modules
{
    [RequireUserPermission(GuildPermission.Administrator), CheckBlacklist]
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
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync($"Mod Channel has been set to **{channel.Name}**");
        }

        [Command("SetPrefix"), Summary("SetPrefix ?"), Remarks("Sets Guild prefix")]
        public async Task SetPrefixAsync(string prefix)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.GuildPrefix = prefix;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync($"Guild Prefix has been set to: **{prefix}**");
        }

        [Command("WelcomeMsg"), Summary("WelcomeMsg This is a welcome Msg"), Remarks("Sets welcome message for your server")]
        public async Task WelcomeMessageAsync([Remainder]string msg)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.WelcomeMessage = msg;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync($"Guild Welcome Message has been set to:\n```{msg}```");
        }

        [Command("Actions"), Summary("Normal Command"), Remarks("Shows what Actions are being logged")]
        public async Task ListLogActionsAsync()
        {
            var GConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var Joins = GConfig.JoinEvent.IsEnabled ? "Enabled" : "Disabled";
            var Leaves = GConfig.LeaveEvent.IsEnabled ? "Enabled" : "Disabled";
            var Bans = GConfig.UserBanned.IsEnabled ? "Enabled" : "Disabled";
            var Karma = GConfig.ChatKarma ? "Enabled" : "Disabled";
            var Chatterbot = GConfig.ChatterBot ? "Enabled" : "Disabled";
            var SB = new StringBuilder();
            foreach(var Names in GConfig.RequiredChannelNames)
            {
                SB.AppendLine(Names);
            }
            var JoinChannel = (await Context.Guild.GetChannelAsync(GConfig.JoinEvent.TextChannel)) as ITextChannel;
            var LeaveChannel = (await Context.Guild.GetChannelAsync(GConfig.LeaveEvent.TextChannel)) as ITextChannel;
            var BanChannel = (await Context.Guild.GetChannelAsync(GConfig.UserBanned.TextChannel)) as ITextChannel;        

            if (string.IsNullOrWhiteSpace(SB.ToString()))
                SB = SB.AppendLine("No channels found in required channel list.");
            string Description =
                                $"**Guild Prefix:** {GConfig.GuildPrefix}\n" +
                                $"**Server Mod Channel:** {GConfig.ModChannelID}\n" +
                                $"**Mute Role ID:** {GConfig.MuteRoleId}\n" +
                                $"**Welcome Message:** {GConfig.WelcomeMessage}\n" +
                                $"**User Join Logging:** {Joins} ({JoinChannel.Mention})\n" +
                                $"**User Leave Logging:** {Leaves} ({LeaveChannel.Mention})\n" +
                                $"**User Ban Logging:** {Bans} ({BanChannel})\n" +
                                $"**Chat Karma:** {Karma}\n" +
                                $"**Chatter Bot:** {Chatterbot}\n" +
                                $"**Total Bans/Kicks Cases:** {GConfig.CaseNumber}\n" +
                                $"**AFK Members:** {GConfig.AfkList.Count}\n" +
                                $"**Total Tags:** {GConfig.TagsList.Count}\n" +
                                $"**Required Channels for NSFW:** {SB.ToString()}";
            var embed = EmbedExtension.Embed(EmbedColors.Teal, $"{Context.Guild.Name} || {(await Context.Guild.GetOwnerAsync()).Username}", Context.Guild.IconUrl, Description: Description, ThumbUrl: Context.Guild.IconUrl);
            await ReplyAsync("", embed: embed);
        }

        [Command("ToggleJoins"), Summary("ToggleJoins #ChannelName"), Remarks("Toggles Join logging")]
        public async Task ToggleJoinsAsync(ITextChannel Channel = null)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.JoinEvent.IsEnabled)
            {
                gldConfig.JoinEvent.IsEnabled = true;
                gldConfig.JoinEvent.TextChannel = Channel.Id;
                Log.EnableJoinLogging();
                await ReplyAsync(":gear: Joins logging enabled!");
            }
            else
            {
                Log.DisableJoinLogging();
                gldConfig.JoinEvent.IsEnabled = false;
                await ReplyAsync(":skull_crossbones:   No longer logging joins.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("ToggleLeaves"), Summary("ToggleLeaves #ChannelName"), Remarks("Toggle Leaves logging")]
        public async Task ToggleLeavesAsync(ITextChannel Channel = null)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.LeaveEvent.IsEnabled)
            {
                gldConfig.LeaveEvent.IsEnabled = true;
                gldConfig.LeaveEvent.TextChannel = Channel.Id;
                Log.EnableLeaveLogging();
                await ReplyAsync(":gear:   Now logging leaves.");
            }
            else
            {
                gldConfig.LeaveEvent.IsEnabled = false;
                Log.DisableLeaveLogging();
                await ReplyAsync(":skull_crossbones:  No longer logging leaves.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("ToggleBans"), Summary("Normal Command"), Remarks("Toggles ban logging")]
        public async Task ToggleBansAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!gldConfig.UserBanned.IsEnabled)
            {
                gldConfig.UserBanned.IsEnabled = true;
                await ReplyAsync(":gear:   Now logging bans.");
            }
            else
            {
                gldConfig.UserBanned.IsEnabled = false;
                await ReplyAsync(":skull_crossbones:  No longer logging bans.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("Channel"), Summary("Channel Add #ChannelName/Channel AddId #ChannelName"), Remarks("Adds/Removes channel names/ids from the list")]
        public async Task ChannelAsync(GlobalEnums Prop, ITextChannel channel)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            switch (Prop)
            {
                case GlobalEnums.Add:
                    gldConfig.RequiredChannelNames.Add(channel.Name);
                    await ReplyAsync($"Channel **{channel.Name}** has been added to RequiredChannel Attribute.");
                    break;

                case GlobalEnums.Remove:
                    gldConfig.RequiredChannelNames.Remove(channel.Name);
                    await ReplyAsync($"Channel **{channel.Name}** has been removed from RequiredChannel Attribute.");
                    break;

                case GlobalEnums.AddId:
                    gldConfig.RequiredRoleIDs.Add(channel.Id);
                    await ReplyAsync($"Channel **{channel.Id}** has been added to RequiredChannel Attribute.");
                    break;

                case GlobalEnums.RemoveId:
                    gldConfig.RequiredRoleIDs.Remove(channel.Id);
                    await ReplyAsync($"Channel **{channel.Id}** has been removed from RequiredChannel Attribute.");
                    break;
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("Role"), Summary("Role AddId RoleName"), Remarks("Adds/Removes role ids from the list")]
        public async Task RoleAsync(GlobalEnums Prop, IRole Role)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            switch (Prop)
            {
                case GlobalEnums.AddId:
                    gldConfig.RequiredRoleIDs.Add(Role.Id);
                    await ReplyAsync($"Role **{Role.Id}** has been added to RequiredRoleIDs Attribute");
                    break;

                case GlobalEnums.RemoveId:
                    gldConfig.RequiredRoleIDs.Remove(Role.Id);
                    await ReplyAsync($"Role **{Role.Id}** has been removed to RequiredRoleIDs Attribute");
                    break;
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
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
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
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
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }
    }
}
