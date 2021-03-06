﻿using Discord;
using System.IO;
using System.Linq;
using Discord.Rest;
using Valerie.Models;
using Valerie.Helpers;
using System.Net.Http;
using Valerie.Handlers;
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
        FileStream AvatarStream()
        {
            if (File.Exists("Avatar.jpg"))
                return new FileStream("Avatar.jpg", FileMode.Open, FileAccess.Read);
            else
                return new FileStream(
                    StringHelper.DownloadImageAsync(HttpClient, SocketClient.CurrentUser.GetAvatarUrl()).GetAwaiter().GetResult(),
                    FileMode.Open, FileAccess.Read);
        }

        public WebhookService(HttpClient httpClient, GuildHandler guild, DiscordSocketClient client)
        {
            GuildHandler = guild;
            SocketClient = client;
            HttpClient = httpClient;
        }

        public DiscordWebhookClient WebhookClient(ulong Id, string Token)
        {
            try
            {
                return new DiscordWebhookClient(Id, Token);
            }
            catch
            {
                LogService.Write(Enums.LogSource.DSD, $"Webhook {Id} Failed.", System.Drawing.Color.Crimson);
                return null;
            }
        }

        public async Task SendMessageAsync(WebhookOptions Options)
        {
            if (!(SocketClient.GetChannel(Options.Webhook.TextChannel) is SocketTextChannel Channel)) return;
            var Client = WebhookClient(Options.Webhook.WebhookId, Options.Webhook.WebhookToken);
            await WebhookFallbackAsync(Client, Channel, Options);
        }

        public async Task<WebhookWrapper> CreateWebhookAsync(SocketTextChannel Channel, string Name)
        {
            var Get = await GetWebhookAsync(Channel, new WebhookOptions
            {
                Name = Name
            });
            var Webhook = Get ?? await Channel.CreateWebhookAsync(Name, AvatarStream());
            return new WebhookWrapper
            {
                TextChannel = Channel.Id,
                WebhookId = Webhook.Id,
                WebhookToken = Webhook.Token
            };
        }

        public async Task<RestWebhook> GetWebhookAsync(SocketGuild Guild, WebhookOptions Options)
            => (await Guild?.GetWebhooksAsync())?.FirstOrDefault(x => x?.Name == Options.Name || x?.Id == Options.Webhook.WebhookId);

        public async Task<RestWebhook> GetWebhookAsync(SocketTextChannel Channel, WebhookOptions Options)
            => (await Channel?.GetWebhooksAsync())?.FirstOrDefault(x => x?.Name == Options.Name || x?.Id == Options.Webhook.WebhookId);

        public Task WebhookFallbackAsync(DiscordWebhookClient Client, ITextChannel Channel, WebhookOptions Options)
        {
            if (Client == null && Channel != null)
            {
                LogService.Write(Enums.LogSource.DSD, $"Falling back to Channel: {Channel.Name}", System.Drawing.Color.Yellow);
                return Channel.SendMessageAsync(Options.Message, embed: Options.Embed);
            }
            return Client.SendMessageAsync(Options.Message, embeds: Options.Embed == null ? null : new List<Embed>() { Options.Embed });
        }

        public async Task<WebhookWrapper> UpdateWebhookAsync(SocketTextChannel Channel, WebhookWrapper Old, WebhookOptions Options)
        {
            var Hook = !(SocketClient.GetChannel(Old.TextChannel) is SocketTextChannel GetChannel) ?
                await GetWebhookAsync(Channel.Guild, new WebhookOptions { Webhook = Old }) :
                await GetWebhookAsync(GetChannel, new WebhookOptions { Webhook = Old });
            if (Channel.Id == Old.TextChannel && Hook != null) return Old;
            else if (Hook != null) await Hook.DeleteAsync();
            var New = await Channel.CreateWebhookAsync(Options.Name, AvatarStream());
            return new WebhookWrapper
            {
                TextChannel = Channel.Id,
                WebhookId = New.Id,
                WebhookToken = New.Token
            };
        }
    }
}