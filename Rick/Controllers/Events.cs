﻿using System;
using System.Threading.Tasks;
using System.Linq;
using Rick.Models;
using Rick.Handlers;
using Discord;
using Discord.WebSocket;
using Rick.Functions;
using Rick.Enums;
using Cleverbot.Models;
using Database;

namespace Rick.Controllers
{
    public class Events
    {
        internal static async Task UserJoinedAsync(SocketGuildUser User)
        {
            var Config = GuildHandler.GuildConfigs[User.Guild.Id];
            ITextChannel Channel = null;
            string WelcomeMessage = null;

            if (!Config.JoinEvent.IsEnabled) return;

            var JoinChannel = User.Guild.GetChannel(Config.JoinEvent.TextChannel);

            if (Config.WelcomeMessages.Count != 0 || Config.WelcomeMessages.Count <= 0)
            {
                WelcomeMessage = Config.WelcomeMessages[new Random().Next(0, Config.WelcomeMessages.Count)];
            }
            else
            {
                WelcomeMessage = $"Welcome {User.Username} to {User.Guild.Name}!";
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
            var Config = GuildHandler.GuildConfigs[User.Guild.Id];
            CleanUpAsync(User);
            if (!Config.LeaveEvent.IsEnabled) return;
            var LeaveChannel = User.Guild.GetChannel(Config.LeaveEvent.TextChannel);
            ITextChannel Channel = null;
            if (User.Guild.GetChannel(Config.LeaveEvent.TextChannel) != null)
            {
                Channel = LeaveChannel as ITextChannel;
                await Channel.SendMessageAsync($"{User.Username} has left {User.Guild.Name}");
            }
            else
            {
                Channel = User.Guild.DefaultChannel as ITextChannel;
                await Channel.SendMessageAsync($"{User.Username} has left {User.Guild.Name}");
            }
        }

        internal static async Task JoinedGuildAsync(SocketGuild Guild)
        {
            var Prefix = ConfigHandler.IConfig.Prefix;
            string Message = $"HELLO! I'm Rick! Thank you for inviting me to your server :eggplant:\n" +
                $"Default Prefix: {Prefix}" +
                $"**Website:** https://Rickbot.cf \n" +
                $"**Command List:** https://Rickbot.cf/Pages/Commands.html \n" +
                $"**Support Server:** https://discord.gg/S5CnhVY \n" +
                $"**Twitter:** https://twitter.com/Vuxey";
            await Guild.DefaultChannel.SendMessageAsync(Message);
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
            GuildHandler.GuildConfigs.Add(Guild.Id, CreateConfig);
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        internal static async Task HandleGuildMessagesAsync(SocketMessage Message)
        {
            var Guild = (Message.Channel as SocketGuildChannel).Guild;
            var User = Message.Author as SocketGuildUser;

            KarmaHandlerAsync(User);
            AFKHandlerAsync(Guild, Message);
            CleverbotHandlerAsync(Guild, Message);

            await AddToMessageAsync(Message);
        }

        #region Event Methods
        static async void CleanUpAsync(SocketGuildUser User)
        {
            var GuildConfig = GuildHandler.GuildConfigs[User.Guild.Id];
            if (GuildConfig.KarmaList.ContainsKey(User.Id))
            {
                GuildConfig.KarmaList.Remove(User.Id);
                Logger.Log(LogType.Warning, LogSource.Configuration, $"{User.Username} removed from {User.Guild.Name}'s Karma List.");
            }
            foreach (var tag in GuildConfig.TagsList)
            {
                if (tag.Owner == User.Id)
                {
                    GuildConfig.TagsList.Remove(tag);
                    Logger.Log(LogType.Warning, LogSource.Configuration, $"Removed {tag.Name} by {User.Username}.");
                }
            }

            GuildHandler.GuildConfigs[User.Guild.Id] = GuildConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        static async void KarmaHandlerAsync(SocketGuildUser User)
        {
            var GuildConfig = GuildHandler.GuildConfigs[User.Guild.Id];

            if (User.IsBot || !GuildConfig.IsKarmaEnabled) return;

            var GetRandom = new Random().Next(1, 10);
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

            GuildHandler.GuildConfigs[User.Guild.Id] = GuildConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        static async void AFKHandlerAsync(SocketGuild Guild, SocketMessage Message)
        {
            var AfkList = GuildHandler.GuildConfigs[Guild.Id].AFKList;
            string afkReason = null;
            SocketUser gldUser = Message.MentionedUsers.FirstOrDefault(u => AfkList.TryGetValue(u.Id, out afkReason));
            if (gldUser != null)
                await Message.Channel.SendMessageAsync($"**Message left from {gldUser.Username}:** {afkReason}");
        }

        static async void CleverbotHandlerAsync(SocketGuild Guild, SocketMessage Message)
        {
            var IsEnabled = GuildHandler.GuildConfigs[Guild.Id].Chatterbot.IsEnabled;
            if (Message.Author.IsBot || !IsEnabled || !Message.Content.StartsWith("Rick")) return;
            string UserMsg = null;
            if (Message.Content.StartsWith("Rick"))
            {
                UserMsg = Message.Content.Replace("Rick", "");
            }
            CleverbotResponse Response = null;
            Response = Cleverbot.Main.Talk(UserMsg, Response);
            await Message.Channel.SendMessageAsync(Response.Output);
        }

        static async Task AddToMessageAsync(SocketMessage Message)
        {
            var Config = ConfigHandler.IConfig;
            Config.MessagesReceived += 1;
            await ConfigHandler.SaveAsync();
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
        #endregion
    }
}
