//using Discord.WebSocket;
//using Nygma.Handlers;
//using System.Threading.Tasks;
//using Discord;
//using Nygma.Utils;

//namespace Nygma.Services
//{
//    public class EventService
//    {
//        private ConfigHandler config;
//        private DiscordSocketClient _client;

//        public EventService(ConfigHandler con)
//        {
//            config = con;
//        }
//        private Dictionary<ulong, GuildHandler> guilds;

//        public async Task UserJoinedAsync(SocketGuildUser user)
//        {

//            if (config.Welcome == true)
//            {
//                var ch = _client.GetGuild(config.LogGuild).GetChannel(config.LogChannel) as ITextChannel;
//                var embed = new EmbedBuilder();
//                embed.Title = "User Joined Event";
//                embed.Description = $"**Username: **{user.Mention}\n**Guild Name: **{user.Guild.Name}\n{config.WelcomeMsg}";
//                embed.Color = Misc.RandColor();
//                await ch.SendMessageAsync("", embed: embed);
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

//        internal async Task UserLeftAsync(SocketGuildUser user)
//        {
//            var ch = _client.GetGuild(config.LogGuild).GetChannel(config.LogChannel) as ITextChannel;
//            var embed = new EmbedBuilder();
//            embed.Title = "User Left Event";
//            embed.Description = $"**Username: **{user.Username}\n**Guild Name: **{user.Guild.Name}";
//            embed.Color = Misc.RandColor();
//            await ch.SendMessageAsync("", embed: embed);
//        }

//        internal async Task GetMsgAsync(SocketMessage message)
//        {
//            if (config.MsgLog == true)
//            {
//                IConsole.Log(LogSeverity.Info, "MESSAGE", "[" + message.Timestamp.UtcDateTime.ToString("dd/MM/yyyy HH:mm:ss") + "]" + message.Author.Username + ": " + message.Content);
//                var chx = message.Channel as SocketGuildChannel as ITextChannel;
//                if (message.Author.Id != _client.CurrentUser.Id)
//                {
//                    var ch = _client.GetGuild(config.LogGuild).GetChannel(config.LogChannel) as ITextChannel;
//                    var embed = new EmbedBuilder();
//                    embed.Title = "Message Received Event";
//                    embed.Description = $"**Guild Name: **{chx.Guild.Name}\n**Channel Name: **{message.Channel.Name}\n**Username: **{message.Author.Username} || **IsBot?** {message.Author.IsBot}\n**Message ID: **{message.Id}\n**Message Content: **{message.Content}\n**Attachments **{message.Attachments.Count}";
//                    embed.Color = Misc.RandColor();
//                    await ch.SendMessageAsync("", embed: embed);
//                }
//                else
//                    IConsole.Log(LogSeverity.Info, "Application Message", "Ignoring this message");
//            }
//        }
//        internal async Task GuildAvailableEvent(SocketGuild guild)
//        {
//            if (guilds.ContainsKey(guild.Id))
//                await guilds[guild.Id].RenewGuildObject(guild);
//            else
//            {
//                GuildHandler gh = new GuildHandler(this, guild);
//                await gh.InitializeAsync();
//                guilds[guild.Id] = gh;
//            }
//        }

//        internal Task LeftGuildEvent(SocketGuild guild)
//        {
//            if (guilds.ContainsKey(guild.Id))
//                guilds[guild.Id].DeleteGuildFolder();
//            return Task.CompletedTask;
//        }
//    }
//}
