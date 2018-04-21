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

        public async Task<RestWebhook> CreateWebhookAsync(SocketTextChannel Channel, string Name, SettingType Setting)
        {
            var Config = GuildHandler.GetGuild(Channel.Guild.Id);
            var Webhook = await Channel.CreateWebhookAsync(Name, new FileStream("Avatar.jpg", FileMode.Open, FileAccess.Read));
            var Info = new KeyValuePair<ulong, KeyValuePair<ulong, string>>(Channel.Id, new KeyValuePair<ulong, string>(Webhook.Id, Webhook.Token));
            switch (Setting)
            {
                case SettingType.RedditChannel: Config.Reddit.Webhook = Info; break;
                case SettingType.CleverbotChannel: Config.CleverbotWebhook = Info; break;
                case SettingType.JoinChannel: Config.JoinWebhook = Info; break;
                case SettingType.LeaveChannel: Config.LeaveWebhook = Info; break;
            }
            GuildHandler.Save(Config);
            return Webhook;
        }

        public async Task<IUserMessage> SendMessageAsync(WebhookOptions Options)
        {
            var Channel = SocketClient.GetChannel(Options.WebhookInfo.Key) as SocketTextChannel;
            var Get = await GetWebhookAsync(Channel, Options.Name, Options.WebhookInfo.Value.Key, Options.Setting);
            return await WebhookFallbackAsync(WebhookClient(Options.WebhookInfo.Value.Key, Options.WebhookInfo.Value.Value), Channel, Options.Message, Options.Embed);
        }

        public async Task<RestWebhook> GetWebhookAsync(SocketTextChannel Channel, string Name, ulong Id, SettingType Setting)
            => (await Channel.GetWebhooksAsync())?.FirstOrDefault(x => x?.Name == Name || x?.Id == Id) ?? await CreateWebhookAsync(Channel, Name, Setting);

        Task<IUserMessage> WebhookFallbackAsync(DiscordWebhookClient Client, ITextChannel Channel, string Message, Embed GetEmbed)
        {
            if (Client == null) return Channel.SendMessageAsync(Message, embed: GetEmbed);
            Client.SendMessageAsync(Message, embeds: GetEmbed == null ? null : new List<Embed>() { GetEmbed });
            return null;
        }
    }
}