using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Rick.Handlers;
using CleverbotLib.Models;

namespace Rick.Services
{
    public class MsgsService
    {
        public static double GiveKarma(double karma)
        {
            return (Math.Pow(karma, 2) + 10 * karma) / 5;
        }

        public static int GetLevelFromXP(double karma)
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
            Random rand = new Random();
            double RandomKarma = rand.Next(1, 5);
            RandomKarma = GiveKarma(RandomKarma);
            if (Guilds.ChatKarma)
            {
                var karmalist = Guilds.Karma;
                if (!karmalist.ContainsKey(message.Author.Id))
                    karmalist.Add(message.Author.Id, 1);
                else
                {
                    int getKarma = karmalist[message.Author.Id];
                    getKarma += Convert.ToInt32(RandomKarma);
                    karmalist[message.Author.Id] = getKarma;
                }
                GuildHandler.GuildConfigs[gld.Id] = Guilds;
                await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            }
        }

        public static async Task CleverBotAsync(SocketUserMessage message, SocketGuild gld)
        {
            var IsEnabled = GuildHandler.GuildConfigs[gld.Id].ChatterBot;
            if (message.Author.IsBot || !message.Content.StartsWith(BotHandler.BotConfig.BotName) || !IsEnabled) return;
            CleverbotResponse Response = null;
            Response = CleverbotLib.Core.Talk(message.Content, Response);
            await message.Channel.SendMessageAsync(Response.Output);
        }
    }
}
