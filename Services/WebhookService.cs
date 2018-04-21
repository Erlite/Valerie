using Discord;
using System.IO;
using System.Linq;
using Discord.Rest;
using Valerie.Enums;
using Valerie.Models;
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

        public async Task<WebhookWrapper> CreateWebhookAsync(SocketTextChannel Channel, string Name)
        {
            var Get = await GetWebhookAsync(Channel, new WebhookOptions
            {
                Name = Name
            });
            var Config = GuildHandler.GetGuild(Channel.Guild.Id);
            var Webhook = Get ?? await Channel.CreateWebhookAsync(Name, new FileStream("Avatar.jpg", FileMode.Open, FileAccess.Read));
            return new WebhookWrapper
            {
                TextChannel = Channel.Id,
                WebhookId = Webhook.Id,
                WebhookToken = Webhook.Token
            };
        }

        public async Task SendMessageAsync(WebhookOptions Options)
        {
            var Channel = SocketClient.GetChannel(Options.Webhook.TextChannel) as SocketTextChannel;
            if (Channel == null) return;
            var Get = await GetWebhookAsync(Channel, Options);
            await WebhookFallbackAsync(WebhookClient(Get), Channel, Options);
        }

        public async Task<RestWebhook> GetWebhookAsync(SocketTextChannel Channel, WebhookOptions Options)
            => (await Channel?.GetWebhooksAsync())?.FirstOrDefault(x => x?.Name == Options.Name || x?.Id == Options.Webhook.WebhookId);

        Task WebhookFallbackAsync(DiscordWebhookClient Client, ITextChannel Channel, WebhookOptions Options)
        {
            if (Client == null || Channel == null) return Task.CompletedTask;
            if (Client == null) return Channel.SendMessageAsync(Options.Message, embed: Options.Embed);
            return Client.SendMessageAsync(Options.Message, embeds: Options.Embed == null ? null : new List<Embed>() { Options.Embed });
        }
    }
}