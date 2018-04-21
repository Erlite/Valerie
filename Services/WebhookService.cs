using Discord;
using System.IO;
using Discord.Rest;
using Valerie.Helpers;
using Valerie.Handlers;
using System.Net.Http;
using Discord.Webhook;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Valerie.Services
{
    public class WebhookService
    {
        HttpClient HttpClient { get; }
        GuildHandler GuildHandler { get; }
        DiscordSocketClient SocketClient { get; }
        public WebhookService(HttpClient httpClient, GuildHandler guild, DiscordSocketClient client)
        {
            GuildHandler = guild;
            SocketClient = client;
            HttpClient = httpClient;
        }

        public DiscordWebhookClient WebhookClient(ulong Id, string Token)
            => new DiscordWebhookClient(Id, Token);

        public DiscordWebhookClient WebhookClient(RestWebhook Webhook)
            => new DiscordWebhookClient(Webhook);

        public async Task<RestWebhook> CreateWebhookAsync(SocketTextChannel Channel, string Name)
            => await Channel.CreateWebhookAsync(Name, new FileStream("Avatar.jpg", FileMode.Open));

        public async Task<RestWebhook> CreateWebhookAsync(SocketTextChannel Channel, SocketGuildUser User)
            => await Channel.CreateWebhookAsync(User.Username, new FileStream(StringHelper.DownloadUserImageAsync(HttpClient, User).Result, FileMode.Open));

        public async Task TemporaryWebhookAsync(DiscordWebhookClient Client, string Message)
        {
            await Client.SendMessageAsync(Message);
            await Client.DeleteWebhookAsync();
        }

        public async Task WebhookFallbackAsync(DiscordWebhookClient Client, ITextChannel Channel, string Message, Embed GetEmbed)
        {
            if (Client == null) await Channel.SendMessageAsync(Message, embed: GetEmbed);
            await Client.SendMessageAsync(Message, embeds: new List<Embed>() { GetEmbed });
        }

        public Task SendMessageAsync(KeyValuePair<ulong, KeyValuePair<ulong, string>> WebhookInfo, string Message, Embed Embed)
        {
            var Channel = SocketClient.GetChannel(WebhookInfo.Key) as SocketGuildChannel;
            var Webhook = WebhookClient(WebhookInfo.Value.Key, WebhookInfo.Value.Value);
            return WebhookFallbackAsync(Webhook, Channel as ITextChannel, Message, Embed);
        }
    }
}