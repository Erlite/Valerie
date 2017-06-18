using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.WebSocket;
using Rick.Handlers;
using CleverbotLib.Models;

namespace Rick.Services
{
    public class MsgsService
    {
        static List<ulong> WaitList = new List<ulong>();

        //public MsgsService()
        //{
        //    _timer = new Timer(x =>
        //    {
        //        WaitList.Remove()
        //    },
        //    null,
        //    TimeSpan.FromSeconds(1),
        //    TimeSpan.FromSeconds(1)
        //    );
        //}

        #region Karma Related Methods
        public static int GiveKarma(int karma)
        {
            return (Convert.ToInt32(Math.Pow(karma, 3)) + 50 * karma) / 5;
        }

        public static int GetLevel(int Karma)
        {
            return 1 + (int)Math.Floor(Math.Pow(Karma, 1 / 3.0));
        }

        public static int GetKarmaForLastLevel(int Level)
        {
            return (Level - 1) * (Level - 1) * (Level - 1);
        }

        public static int GetKarmaForNextLevel(int Level)
        {
            return Level * Level * Level;
        }
        #endregion


        public static async Task AfkAsync(SocketUserMessage message, SocketGuild gld)
        {
            var AfkList = GuildHandler.GuildConfigs[gld.Id].AfkList;
            string afkReason = null;
            SocketUser gldUser = message.MentionedUsers.FirstOrDefault(u => AfkList.TryGetValue(u.Id, out afkReason));
            if (gldUser != null)
                await message.Channel.SendMessageAsync($"**Message left from {gldUser.Username}:** {afkReason}");
        }

        public static async Task ChatKarmaAsync(SocketUserMessage message, SocketGuild gld)
        {
            var Guilds = GuildHandler.GuildConfigs[gld.Id];
            if (message.Author.IsBot || !Guilds.IsKarmaEnabled) return;

            var GetRandom = new Random().Next(1, 10);
            var RandomKarma = GiveKarma(GetRandom);
            var karmalist = Guilds.KarmaList;
            if (!karmalist.ContainsKey(message.Author.Id))
            {
                karmalist.Add(message.Author.Id, RandomKarma);
                return;
            }

            //if (WaitList.Contains(message.Author.Id))
            //    return;

            int getKarma = karmalist[message.Author.Id];
            getKarma += RandomKarma;
            karmalist[message.Author.Id] = getKarma;

            GuildHandler.GuildConfigs[gld.Id] = Guilds;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs).ConfigureAwait(false);
        }

        public static async Task CleverBot(SocketUserMessage message, SocketGuild gld)
        {
            var IsEnabled = GuildHandler.GuildConfigs[gld.Id].IsChatterBotEnabled;
            if (message.Author.IsBot || !IsEnabled || !message.Content.StartsWith(BotHandler.BotConfig.BotName)) return;
            string UserMsg = null;
            if (message.Content.Contains(BotHandler.BotConfig.BotName))
            {
                UserMsg = message.Content.Replace(BotHandler.BotConfig.BotName, "");
            }
            CleverbotResponse Response = null;
            Response = CleverbotLib.Core.Talk(UserMsg, Response);
            await message.Channel.SendMessageAsync(Response.Output);
        }

        public static async Task AddToMessage(SocketUserMessage Message)
        {
            var Config = BotHandler.BotConfig;
            Config.MessagesReceived += 1;
            await BotHandler.SaveAsync(Config);
        }

        public static async Task AddToCommand(SocketUserMessage Message)
        {
            var Config = BotHandler.BotConfig;
            Config.CommandsUsed += 1;
            await BotHandler.SaveAsync(Config);
        }
    }
}
