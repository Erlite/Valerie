using System;
using System.Threading.Tasks;
using System.Linq;
using Rick.Models;
using Rick.Handlers;
using Discord;
using Discord.WebSocket;
using Rick.Functions;
using Rick.Enums;
using Cleverbot.Models;
using Rick.Extensions;
using System.Threading;
using System.Collections.Generic;

namespace Rick.Controllers
{
    public class Events
    {
        static List<ulong> Waitlist = new List<ulong>();
        static Timer timer;

        internal static async Task UserJoinedAsync(SocketGuildUser User)
        {
            var Config = GuildHandler.GuildConfigs[User.Guild.Id];
            if (!Config.JoinEvent.IsEnabled) return;

            ITextChannel Channel = null;
            string WelcomeMessage = null;

            var JoinChannel = User.Guild.GetChannel(Config.JoinEvent.TextChannel);

            if (Config.WelcomeMessages.Count <= 0)
                WelcomeMessage = $"{User.Mention} just joined {User.Guild.Name}! WELCOME!";
            else
            {
                var ConfigMsg = Config.WelcomeMessages[new Random().Next(0, Config.WelcomeMessages.Count)];
                WelcomeMessage = StringExtension.ReplaceWith(ConfigMsg, User.Mention, User.Guild.Name);
            }

            if (User.Guild.GetChannel(Config.JoinEvent.TextChannel) != null)
            {
                Channel = JoinChannel as ITextChannel;
                await Channel.SendMessageAsync(WelcomeMessage);
            }
            else
            {
                Channel = User.Guild.DefaultChannel as ITextChannel;
                await Channel.SendMessageAsync(WelcomeMessage);
            }
        }

        internal static async Task UserLeftAsync(SocketGuildUser User)
        {
            await CleanUpAsync(User);

            var Config = GuildHandler.GuildConfigs[User.Guild.Id];
            if (!Config.LeaveEvent.IsEnabled) return;

            ITextChannel Channel = null;
            string LeaveMessage = null;

            var LeaveChannel = User.Guild.GetChannel(Config.LeaveEvent.TextChannel);

            if (Config.LeaveMessages.Count <= 0)
                LeaveMessage = $"{User.Mention} has left {User.Guild.Name} :wave:";
            else
            {
                var configMsg = Config.LeaveMessages[new Random().Next(0, Config.LeaveMessages.Count)];
                LeaveMessage = StringExtension.ReplaceWith(configMsg, User.Username, User.Guild.Name);
            }

            if (User.Guild.GetChannel(Config.LeaveEvent.TextChannel) != null)
            {
                Channel = LeaveChannel as ITextChannel;
                await Channel.SendMessageAsync(LeaveMessage);
            }
            else
            {
                Channel = User.Guild.DefaultChannel as ITextChannel;
                await Channel.SendMessageAsync(LeaveMessage);
            }
        }

        internal static async Task DeleteGuildConfig(SocketGuild Guild)
        {
            if (GuildHandler.GuildConfigs.ContainsKey(Guild.Id))
            {
                GuildHandler.GuildConfigs.Remove(Guild.Id);
            }
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        internal static async Task HandleGuildConfigAsync(SocketGuild Guild)
        {
            var CreateConfig = new GuildModel();
            if (!GuildHandler.GuildConfigs.ContainsKey(Guild.Id))
            {
                GuildHandler.GuildConfigs.Add(Guild.Id, CreateConfig);
            }
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        internal static async Task HandleGuildMessagesAsync(SocketMessage Message)
        {
            var Guild = (Message.Channel as SocketGuildChannel).Guild;

            await KarmaHandlerAsync(Message.Author as SocketGuildUser);
            await AFKHandlerAsync(Guild, Message);
            await CleverbotHandlerAsync(Guild, Message);
            await AntiAdvertisementAsync(Guild, Message);
            await AddToMessageAsync(Message);
        }

        internal static async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> CacheMsgs, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            var GuildConfig = GuildHandler.GuildConfigs[(Channel as SocketGuildChannel).Guild.Id];
            if (!GuildConfig.Starboard.IsEnabled && Reaction.Emote.Name != "⭐") return;
            await CacheMsgs.GetOrDownloadAsync();
            if (Reaction.Message.IsSpecified && CacheMsgs.Value.Id == Reaction.Message.Value.Id)
            {
                var embed = EmbedExtension.Embed(EmbedColors.Gold,
                    Description: $":star:{Reaction.Message.Value.Reactions.Count} {(Reaction.Channel as ITextChannel).Mention}\n{Reaction.Message.Value.Content}");
                await CacheMsgs.Value.ModifyAsync(x =>
                {
                    x.Embed = new Optional<Embed>(embed);
                });
            }
            else
            {
                var SbChannel = (Channel as SocketGuildChannel).Guild.GetChannel(GuildConfig.Starboard.TextChannel) as IMessageChannel;
                var embed = EmbedExtension.Embed(EmbedColors.Gold,
                    Description: $":star:{Reaction.Message.Value.Reactions.Count} {(Reaction.Channel as ITextChannel).Mention}\n{Reaction.Message.Value.Content}");
                await SbChannel.SendMessageAsync("", embed: embed);
            }
        }

        internal static async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> CacheMsgs, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            var Guild = (Channel as SocketGuildChannel).Guild;
            var GuildConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!GuildConfig.Starboard.IsEnabled && Reaction.Emote.Name != "⭐") return;
            await CacheMsgs.GetOrDownloadAsync();
            if (Reaction.Message.IsSpecified && CacheMsgs.Value.Id == Reaction.Message.Value.Id)
            {
                var embed = EmbedExtension.Embed(EmbedColors.Gold,
                    Description: $":star:{Reaction.Message.Value.Reactions.Count} {(Reaction.Channel as ITextChannel).Mention}\n{Reaction.Message.Value.Content}");
                await CacheMsgs.Value.ModifyAsync(x =>
                {
                    x.Embed = new Optional<Embed>(embed);
                });
            }
            else if (Reaction.Message.Value.Reactions.Count <= 0)
            {
                await Reaction.Message.Value.DeleteAsync();
            }
        }

        #region Event Methods
        static async Task CleanUpAsync(SocketGuildUser User)
        {
            var GuildConfig = GuildHandler.GuildConfigs[User.Guild.Id];
            if (GuildConfig.KarmaList.ContainsKey(User.Id))
            {
                GuildConfig.KarmaList.Remove(User.Id);
            }
            if (GuildConfig.AFKList.ContainsKey(User.Id))
            {
                GuildConfig.AFKList.Remove(User.Id);
            }
            foreach (var tag in GuildConfig.TagsList)
            {
                if (tag.Owner == User.Id)
                {
                    GuildConfig.TagsList.Remove(tag);
                }
            }

            GuildHandler.GuildConfigs[User.Guild.Id] = GuildConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        static async Task KarmaHandlerAsync(SocketGuildUser User)
        {
            RemoveUser(User.Id);
            var GuildID = User.Guild.Id;
            var GuildConfig = GuildHandler.GuildConfigs[GuildID];
            if (User == null ||                 
                User.IsBot ||
                ConfigHandler.IConfig.Blacklist.ContainsKey(User.Id) ||
                !GuildConfig.IsKarmaEnabled ||
                Waitlist.Contains(User.Id)) return;

            var GetRandom = new Random().Next(1, 5);
            var RandomKarma = Fomulas.GiveKarma(GetRandom);
            var karmalist = GuildConfig.KarmaList;
            if (!karmalist.ContainsKey(User.Id))
            {
                karmalist.Add(User.Id, RandomKarma);
                return;
            }
                int getKarma = karmalist[User.Id];
                getKarma += RandomKarma;
                karmalist[User.Id] = getKarma;

                GuildHandler.GuildConfigs[GuildID] = GuildConfig;
                await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
                Waitlist.Add(User.Id);
        }

        static async Task AFKHandlerAsync(SocketGuild Guild, SocketMessage Message)
        {
            var AfkList = GuildHandler.GuildConfigs[Guild.Id].AFKList;
            string afkReason = null;
            SocketUser gldUser = Message.MentionedUsers.FirstOrDefault(u => AfkList.TryGetValue(u.Id, out afkReason));
            if (gldUser != null)
                await Message.Channel.SendMessageAsync($"**Message left from {gldUser.Username}:** {afkReason}");
        }

        static async Task CleverbotHandlerAsync(SocketGuild Guild, SocketMessage Message)
        {
            var GC = GuildHandler.GuildConfigs[Guild.Id];
            var IsEnabled = GC.Chatterbot.IsEnabled;
            var Channel = Guild.GetChannel(GC.Chatterbot.TextChannel) as IMessageChannel;
            if (Message.Author.IsBot || !IsEnabled || !Message.Content.StartsWith("Rick") || Message.Channel != Channel) return;
            string UserMsg = null;
            if (Message.Content.StartsWith("Rick"))
            {
                UserMsg = Message.Content.Replace("Rick", "");
            }
            CleverbotResponse Response = null;
            Response = Cleverbot.Main.Talk(UserMsg, Response);
            if (Channel != null)
                await Channel.SendMessageAsync(Response.Output);
            else
                await Message.Channel.SendMessageAsync(Response.Output);
        }

        static async Task AddToMessageAsync(SocketMessage Message)
        {
            var Config = ConfigHandler.IConfig;
            Config.MessagesReceived += 1;
            await ConfigHandler.SaveAsync();
        }

        static async Task AntiAdvertisementAsync(SocketGuild Guild, SocketMessage Message)
        {
            var Config = GuildHandler.GuildConfigs[Guild.Id];
            if (!Config.NoInvites || Guild == null) return;
            if (Function.Advertisement(Message.Content))
            {
                await Message.DeleteAsync();
                await Message.Channel.SendMessageAsync($"{Message.Author.Mention}, please don't post invite links.");
            }
        }

        public static async Task OnReadyAsync(DiscordSocketClient Client)
        {
            var Config = ConfigHandler.IConfig;
            var GetGame = Config.Games[new Random().Next(Config.Games.Count)];

            if (Client == null) return;

            if (Config.Games.Count <= 0)
            {
                await Client.SetGameAsync(Config.Prefix + "Cmds");
                return;
            }
            else
                await Client.SetGameAsync(GetGame);
        }

        public static async Task LatencyHandlerAsync(DiscordSocketClient Client, int Older, int Newer)
        {
            if (Client == null) return;

            var Status = (Client.ConnectionState == ConnectionState.Disconnected || Newer > 150) ? UserStatus.DoNotDisturb
                : (Client.ConnectionState == ConnectionState.Connecting || Newer > 100) ? UserStatus.Idle
                : (Client.ConnectionState == ConnectionState.Connected || Newer < 100) ? UserStatus.Online : UserStatus.AFK;

            await Client.SetStatusAsync(Status);
        }

        public static async Task AddToCommand(SocketMessage Message)
        {
            var Config = ConfigHandler.IConfig;
            Config.CommandsUsed += 1;
            await ConfigHandler.SaveAsync();
        }

        public static async Task JoinedGuildAsync(SocketGuild Guild)
        {
            var Prefix = ConfigHandler.IConfig.Prefix;
            var gld = GuildHandler.GuildConfigs[Guild.Id];
            string Message = $"HENLO! I'm Ricky R-r-r-r-RICK! c: :eggplant:" +
                $"I'm better than SIRI and you! Yea! You heard that right! :100:\n" +
                $"Default Prefix is: {Prefix}\n" +
                $"Your Guild Prefix is: {gld.Prefix}\n" +
                $"To setup your guild read the `Guild Commands` section on command list. Use `{Prefix}Settings` to preview your guild settings.";
            string Misc = $"[My Website](https://Rickbot.cf) | [Command List](https://Rickbot.cf/Pages/Commands.html) | " +
                $"[My Support Server](https://discord.gg/S5CnhVY) | [Follow Me](https://twitter.com/Vuxey) | " +
                $"[Invite Me](https://discordapp.com/oauth2/authorize?client_id=261561347966238721&scope=bot&permissions=2146946175)";
            await Guild.DefaultChannel.SendMessageAsync($"{Message}\n{Misc}");

            var CreateConfig = new GuildModel();
            if (!GuildHandler.GuildConfigs.ContainsKey(Guild.Id))
            {
                GuildHandler.GuildConfigs.Add(Guild.Id, CreateConfig);
            }
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        static void RemoveUser(ulong Id)
        {
            timer = new Timer(_ =>
            {
                Waitlist.Remove(Id);
            },
            null,
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(60));
        }
        #endregion
    }
}