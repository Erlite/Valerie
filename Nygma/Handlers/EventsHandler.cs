//using System.Threading.Tasks;
//using Discord;
//using Discord.WebSocket;
//using Discord.Commands;
//using Nygma.Handlers;
//using Discord.Addons.Paginator;
//using Nygma.Utils;
//using System.IO;
//using System;

//namespace Nygma.Handlers
//{
//    public static class EventsHandler
//    {
//        private static DiscordSocketClient _client;
//        private static ConfigHandler config;

//        public EventsHandler(ConfigHandler con, DiscordSocketClient client)
//        {
//            config = con;
//            _client = client;
//        }

//        public static async Task UserJoinedAsync(SocketGuildUser user)
//        {
//            if (_client.GetGuild(config.LogGuild).Id == config.LogGuild)
//            {
//                if (config.Welcome == true)
//                {
//                    var ch = _client.GetGuild(config.LogGuild).GetChannel(config.LogChannel) as ITextChannel;
//                    var embed = new EmbedBuilder();
//                    embed.Title = "User Joined Event";
//                    embed.Description = $"**Username: **{user.Mention}\n**Guild Name: **{user.Guild.Name}\n{config.WelcomeMsg}";
//                    embed.Color = Misc.RandColor();
//                    await ch.SendMessageAsync("", embed: embed);
//                }
//            }
//            else
//            {
//                var ch = _client.GetGuild(config.LogGuild).GetChannel(config.LogChannel) as ITextChannel;
//                var embed = new EmbedBuilder();
//                embed.Title = "User Joined Event";
//                embed.Description = $"**Username: **{user.Username}\n**Guild Name: **{user.Guild.Name}";
//                embed.Color = Misc.RandColor();
//                await ch.SendMessageAsync("", embed: embed);
//            }
//        }

//        public static async Task UserLeftAsync(SocketGuildUser user)
//        {
//            var ch = _client.GetGuild(config.LogGuild).GetChannel(config.LogChannel) as ITextChannel;
//            var embed = new EmbedBuilder();
//            embed.Title = "User Left Event";
//            embed.Description = $"**Username: **{user.Username}\n**Guild Name: **{user.Guild.Name}";
//            embed.Color = Misc.RandColor();
//            await ch.SendMessageAsync("", embed: embed);
//        }

//        public static async Task MessageReceivedAsync(SocketMessage message)
//        {
//            IConsole.Log(LogSeverity.Info, "MESSAGE", "[" + message.Timestamp.UtcDateTime.ToString("dd/MM/yyyy HH:mm:ss") + "]" + message.Author.Username + ": " + message.Content);
//            var ch = _client.GetGuild(config.LogGuild).GetChannel(config.LogChannel) as ITextChannel;
//            var embed = new EmbedBuilder();
//            embed.Title = "Message Received Event";
//            embed.Description = $"**Username: **{message.Author.Username}\n**Message ID: **{message.Id}\n**Channel Name: **{message.Content}\n**Message Content: **{message.Channel.Name}";
//            embed.Color = Misc.RandColor();
//            await ch.SendMessageAsync("", embed: embed);
//        }

//    }
//}
