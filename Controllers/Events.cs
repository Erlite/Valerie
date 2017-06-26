using System;
using System.Threading.Tasks;
using System.Linq;
using Rick.Models;
using Rick.Handlers;
using Discord;
using Discord.WebSocket;
using Library.Cleverbot.Models;
using Rick.Functions;

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
            var GuildConfig = GuildHandler.GuildConfigs[Guild.Id];
            var BotConfig = ConfigHandler.IConfig;

            BotConfig.MessagesReceived += 1;

            var AFKList = GuildConfig.AFKList;
            var KarmaList = GuildConfig.KarmaList;

            if (Message.Author.IsBot || !GuildConfig.Chatterbot.IsEnabled || !GuildConfig.IsKarmaEnabled) return;

            string Msg = null;

            #region Guild AFK Tasks
            SocketUser usr = Message.MentionedUsers.FirstOrDefault(User => AFKList.TryGetValue(User.Id, out Msg));
            await Message.Channel.SendMessageAsync($"**Message left from {usr.Username}:** {Msg}");
            #endregion

            #region Cleverbot Tasks
            if (Message.Content.StartsWith(ConfigHandler.IConfig.Prefix))
            {
                Msg = Message.Content.Replace(ConfigHandler.IConfig.Prefix, "");
            }

            CleverbotResponse Response = null;
            Response = Library.Cleverbot.Core.Talk(Msg, Response);
            await Message.Channel.SendMessageAsync(Response.Output);
            #endregion

            #region Karma Tasks
            var GetRandom = new Random().Next(1, 10);
            var RandomKarma = Fomulas.GiveKarma(GetRandom);
            var karmalist = GuildConfig.KarmaList;
            if (!karmalist.ContainsKey(Message.Author.Id))
            {
                karmalist.Add(Message.Author.Id, RandomKarma);
                return;
            }

            int getKarma = karmalist[Message.Author.Id];
            getKarma += RandomKarma;
            karmalist[Message.Author.Id] = getKarma;
            #endregion

            GuildHandler.GuildConfigs[Guild.Id] = GuildConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs).ConfigureAwait(false);
            await ConfigHandler.SaveAsync();
        }

        internal static async Task LatencyAsync(int Older, int Newer)
        {

        }
    }
}
