using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using Valerie.Handlers.GuildHandler;
using Valerie.Handlers.GuildHandler.Enum;
using Valerie.Handlers.ConfigHandler;
using Valerie.Extensions;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Cleverbot.Models;

namespace Valerie.Handlers
{
    public class EventsHandler
    {
        static List<ulong> Waitlist = new List<ulong>();
        static Timer timer;

        internal static async Task GuildAvailableAsync(SocketGuild Guild)
        {
            await ServerDB.LoadGuildConfigsAsync(Guild.Id);
        }

        internal static async Task JoinedGuildAsync(SocketGuild Guild)
        {
            await ServerDB.LoadGuildConfigsAsync(Guild.Id);
            var Config = ServerDB.GuildConfig(Guild.Id);
            string JoinMessage = null;
            if (string.IsNullOrWhiteSpace(BotDB.Config.GuildJoinMessage))
                JoinMessage = $"Thank you for inviting me to your guild!\n" +
                    $"Your Guild Prefix: {Config.Prefix}  | Default Prefix: {BotDB.Config.Prefix}";
            else
                JoinMessage = BotDB.Config.GuildJoinMessage;
            await Guild.DefaultChannel.SendMessageAsync(JoinMessage);
        }

        internal static async Task LeftGuildAsync(SocketGuild Guild)
        {
            await ServerDB.DeleteGuildConfigAsync(Guild.Id);
        }

        internal static async Task UserJoinedAsync(SocketGuildUser User)
        {
            var Config = ServerDB.GuildConfig(User.Guild.Id);
            if (!Config.JoinEvent.IsEnabled) return;

            ITextChannel Channel = null;
            string WelcomeMessage = null;
            ulong Id = Convert.ToUInt64(Config.JoinEvent.TextChannel);
            var JoinChannel = User.Guild.GetChannel(Id);

            if (Config.WelcomeMessages.Count <= 0)
                WelcomeMessage = $"{User} just joined {User.Guild.Name}! WELCOME!";
            else
            {
                var ConfigMsg = Config.WelcomeMessages[new Random().Next(0, Config.WelcomeMessages.Count)];
                WelcomeMessage = StringExtension.ReplaceWith(ConfigMsg, User.Mention, User.Guild.Name);
            }

            if (User.Guild.GetChannel(Id) != null)
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
            var Config = ServerDB.GuildConfig(User.Guild.Id);
            if (!Config.LeaveEvent.IsEnabled) return;

            ITextChannel Channel = null;
            string LeaveMessage = null;
            ulong Id = Convert.ToUInt64(Config.LeaveEvent.TextChannel);
            var LeaveChannel = User.Guild.GetChannel(Id);

            if (Config.LeaveMessages.Count <= 0)
                LeaveMessage = $"{User} has left {User.Guild.Name} :wave:";
            else
            {
                var configMsg = Config.LeaveMessages[new Random().Next(0, Config.LeaveMessages.Count)];
                LeaveMessage = StringExtension.ReplaceWith(configMsg, User.Username, User.Guild.Name);
            }

            if (User.Guild.GetChannel(Id) != null)
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

        internal static async Task MessageReceivedAsync(SocketMessage Message)
        {
            await BotDB.UpdateConfigAsync(ConfigHandler.Enum.ConfigValue.MessageReceived);
            await KarmaHandlerAsync(Message.Author as SocketGuildUser, Message.Content.Length);
            await AFKHandlerAsync((Message.Author as SocketGuildUser).Guild, Message);
            await CleanUpAsync(Message.Author as SocketGuildUser);
            await CleverbotHandlerAsync((Message.Author as SocketGuildUser).Guild, Message);
            await AntiAdvertisementAsync((Message.Author as SocketGuildUser).Guild, Message);
        }

        // Not Events
        static void RemoveUser(ulong Id)
        {
            timer = new Timer(_ =>
            {
                Waitlist.Remove(Id);
            },
            null,
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(0));
        }

        static async Task KarmaHandlerAsync(SocketGuildUser User, int Karma)
        {
            RemoveUser(User.Id);
            var GuildID = User.Guild.Id;
            var GuildConfig = ServerDB.GuildConfig(GuildID);
            if (User == null || User.IsBot || !GuildConfig.IsKarmaEnabled ||
                BotDB.Config.Blacklist.ContainsKey(User.Id) || Waitlist.Contains(User.Id)) return;

            var RandomKarma = IntExtension.GiveKarma(Karma);
            if (!GuildConfig.KarmaList.ContainsKey(User.Id))
            {
                await ServerDB.KarmaHandlerAsync(GuildID, ModelEnum.KarmaAdd, User.Id, RandomKarma);
                return;
            }
            await ServerDB.KarmaHandlerAsync(GuildID, ModelEnum.KarmaUpdate, User.Id, RandomKarma);
            Waitlist.Add(User.Id);
        }

        static async Task AFKHandlerAsync(SocketGuild Guild, SocketMessage Message)
        {
            var Config = ServerDB.GuildConfig(Guild.Id);
            string afkReason = null;
            SocketUser gldUser = Message.MentionedUsers.FirstOrDefault(u => Config.AFKList.TryGetValue(u.Id, out afkReason));
            if (gldUser != null)
                await Message.Channel.SendMessageAsync($"**Message left from {gldUser.Username}:** {afkReason}");
        }

        static async Task CleanUpAsync(SocketGuildUser User)
        {
            var GuildConfig = ServerDB.GuildConfig(User.Guild.Id);
            if (GuildConfig.KarmaList.ContainsKey(User.Id))
            {
                await ServerDB.UpdateConfigAsync(User.Guild.Id, ModelEnum.KarmaDelete, $"{User.Id}");
            }
            if (GuildConfig.AFKList.ContainsKey(User.Id))
            {
                await ServerDB.UpdateConfigAsync(User.Guild.Id, ModelEnum.AFKRemove, $"{User.Id}");
            }
            foreach (var tag in GuildConfig.TagsList)
            {
                if (Convert.ToUInt64(tag.Owner) == User.Id)
                {
                    await ServerDB.TagsHandlerAsync(User.Guild.Id, ModelEnum.TagRemove, Owner: $"{User.Id}");
                }
            }
        }

        static async Task CleverbotHandlerAsync(SocketGuild Guild, SocketMessage Message)
        {
            var Config = ServerDB.GuildConfig(Guild.Id);
            var IsEnabled = Config.Chatterbot.IsEnabled;
            var Channel = Guild.GetChannel(Convert.ToUInt64(Config.Chatterbot.TextChannel)) as IMessageChannel;
            if (Message.Author.IsBot || !IsEnabled || !Message.Content.StartsWith("Valerie") || Message.Channel != Channel) return;
            string UserMsg = null;
            if (Message.Content.StartsWith("Valerie"))
            {
                UserMsg = Message.Content.Replace("Valerie", "");
            }
            CleverbotResponse Response = null;
            Response = await Cleverbot.Main.TalkAsync(UserMsg, Response);
            if (Channel != null)
                await Channel.SendMessageAsync(Response.Output);
            else
                await Message.Channel.SendMessageAsync(Response.Output);
        }

        static async Task AntiAdvertisementAsync(SocketGuild Guild, SocketMessage Message)
        {
            var Config = ServerDB.GuildConfig(Guild.Id);
            if (!Config.AntiAdvertisement || Guild == null) return;
            if (BoolExtension.Advertisement(Message.Content))
            {
                await Message.DeleteAsync();
                await Message.Channel.SendMessageAsync($"{Message.Author.Mention}, please don't post invite links.");
            }
        }

        public static async Task LatencyUpdatedAsync(DiscordSocketClient Client, int Older, int Newer)
        {
            if (Client == null) return;

            var Status = (Client.ConnectionState == ConnectionState.Disconnected || Newer > 150) ? UserStatus.DoNotDisturb
                : (Client.ConnectionState == ConnectionState.Connecting || Newer > 100) ? UserStatus.Idle
                : (Client.ConnectionState == ConnectionState.Connected || Newer < 100) ? UserStatus.Online : UserStatus.AFK;

            await Client.SetStatusAsync(Status);
        }

        public static async Task ReadyAsync(DiscordSocketClient Client)
        {
            var Config = BotDB.Config;
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

    }
}
