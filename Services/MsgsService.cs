using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Timer = System.Timers.Timer;
using Discord.WebSocket;
using Rick.Handlers;
using CleverbotLib.Models;

namespace Rick.Services
{
    public class MsgsService
    {
        static List<ulong> WaitList = new List<ulong>();

        public static double GiveKarma(double karma)
        {
            return (Math.Pow(karma, 2) + 50 * karma) / 5;
        }

        public static int GetLevelFromKarma(double karma)
        {
            return Convert.ToInt32(Math.Sqrt(karma) / 5);
        }

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
            if (message.Author.IsBot || !Guilds.ChatKarma) return;

            //Timer MinuteTimer = new Timer(60000);
            //MinuteTimer.AutoReset = true;
            //MinuteTimer.Elapsed += ElapsedTime;
            //MinuteTimer.Start();

            Random rand = new Random();
            double RandomKarma = rand.Next(1, 10);
            RandomKarma = GiveKarma(RandomKarma);
            if (Guilds.ChatKarma)
            {
                var karmalist = Guilds.Karma;
                if (!karmalist.ContainsKey(message.Author.Id))
                {
                    karmalist.Add(message.Author.Id, Convert.ToInt32(RandomKarma));
                }
                else
                {
          //          if (WaitList.Contains(message.Author.Id))
           //             return;
              //      else
             //       {
                        int getKarma = karmalist[message.Author.Id];
                        getKarma += Convert.ToInt32(RandomKarma);
                        karmalist[message.Author.Id] = getKarma;
                        //WaitList.Add(message.Author.Id);
                        //ConsoleService.Log(Enums.LogType.Received, Enums.LogSource.Client, $"Added {message.Author.Username} to Waitlist!");
                    //}
                }
                GuildHandler.GuildConfigs[gld.Id] = Guilds;
                await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            }
        }

        //private static void ElapsedTime(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    if (WaitList.Contains(User.Id))
        //        WaitList.Remove(User.Id);
        //}

        public static async Task CleverBot(SocketUserMessage message, SocketGuild gld)
        {
            var IsEnabled = GuildHandler.GuildConfigs[gld.Id].ChatterBot;
            if (message.Author.IsBot ||  !IsEnabled || !message.Content.StartsWith(BotHandler.BotConfig.BotName)) return;
            string UserMsg = null;
            if (message.Content.Contains(BotHandler.BotConfig.BotName))
            {
                UserMsg = message.Content.Replace(BotHandler.BotConfig.BotName, "");
            }
            CleverbotResponse Response = null;
            Response = CleverbotLib.Core.Talk(UserMsg, Response);
            await message.Channel.SendMessageAsync(Response.Output);
        }
    }
}
