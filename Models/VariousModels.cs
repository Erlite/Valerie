using System;
using Discord;
using Valerie.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Valerie.Models
{
    public class XPWrapper
    {
        public bool IsEnabled { get; set; }
        public string LevelMessage { get; set; }
        public List<string> ForbiddenRoles { get; set; } = new List<string>(20);
        public Dictionary<ulong, int> LevelRoles { get; set; } = new Dictionary<ulong, int>();
    }

    public class StarboardWrapper
    {
        public ulong TextChannel { get; set; }
        public List<StarboardMessage> StarboardMessages { get; set; } = new List<StarboardMessage>();        
    }

    public class StarboardMessage
    {
        public int Stars { get; set; }
        public string _AuthorId { get; set; }
        public string _ChannelId { get; set; }
        public string _MessageId { get; set; }
        public string _StarboardMessageId { get; set; }
        [JsonIgnore]
        public ulong AuthorId { get => UInt64.TryParse(_AuthorId, out ulong authorId) ? authorId : 0; set => _AuthorId = $"{value}"; }
        [JsonIgnore]
        public ulong ChannelId { get => UInt64.TryParse(_ChannelId, out ulong channelId) ? channelId : 0; set => _ChannelId = $"{value}"; }
        [JsonIgnore]
        public ulong MessageId { get => UInt64.TryParse(_MessageId, out ulong messageId) ? messageId : 0; set => _MessageId = $"{value}"; }
        [JsonIgnore]
        public ulong StarboardMessageId { get => UInt64.TryParse(_StarboardMessageId, out ulong starboardId) ? starboardId : 0; set => _StarboardMessageId = $"{value}"; }
    }

    public class TagWrapper
    {
        public int Uses { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string _Owner { get; set; }
        public bool AutoRespond { get; set; }
        public DateTime CreationDate { get; set; }
        [JsonIgnore]
        public ulong Owner { get => UInt64.TryParse(_Owner, out ulong Id) ? Id : 0; set => _Owner = $"{value}"; }
    }

    public class ModWrapper
    {
        public bool AutoMod { get; set; }
        public bool AntiInvite { get; set; }
        public string _JoinRole { get; set; }
        public int MaxWarnings { get; set; }
        public string _MuteRole { get; set; }
        public ulong TextChannel { get; set; }
        public bool AntiProfanity { get; set; }
        public bool LogDeletedMessages { get; set; }
        public List<CaseWrapper> Cases { get; set; } = new List<CaseWrapper>();
        [JsonIgnore]
        public ulong JoinRole { get => UInt64.TryParse(_JoinRole, out ulong Id) ? Id : 0; set => _JoinRole = $"{value}"; }
        [JsonIgnore]
        public ulong MuteRole { get => UInt64.TryParse(_MuteRole, out ulong Id) ? Id : 0; set => _MuteRole = $"{value}"; }
    }

    public class CaseWrapper
    {
        public string Reason { get; set; }        
        public string _ModId { get; set; }
        public string _UserId { get; set; }
        public int CaseNumber { get; set; }
        public string _MessageId { get; set; }
        public CaseType CaseType { get; set; }
        [JsonIgnore]
        public ulong ModId { get => UInt64.TryParse(_ModId, out ulong Id) ? Id : 0; set => _ModId = $"{value}"; }
        [JsonIgnore]
        public ulong UserId { get => UInt64.TryParse(_UserId, out ulong Id) ? Id : 0; set => _UserId = $"{value}"; }
        [JsonIgnore]
        public ulong MessageId { get => UInt64.TryParse(_MessageId, out ulong Id) ? Id : 0; set => _MessageId = $"{value}"; }
    }

    public enum CaseType
    {
        Ban,
        Kick,
        AutoMod
    }

    public class RedditWrapper
    {
        public bool IsEnabled { get; set; }
        public List<string> Subreddits { get; set; } = new List<string>(5);
        public KeyValuePair<ulong, KeyValuePair<ulong, string>> Webhook { get; set; }
    }

    public class UserProfile
    {
        public int ChatXP { get; set; }
        public int Crystals { get; set; }
        public int Warnings { get; set; }
        public int DailyStreak { get; set; }
        public bool IsBlacklisted { get; set; }
        public DateTime? DailyReward { get; set; }
        public Dictionary<string, int> Commands { get; set; } = new Dictionary<string, int>();
    }

    public class MessageWrapper
    {
        public string Content { get; set; }
        public string _AuthorId { get; set; }
        public string _ChannelId { get; set; }
        public string _MessageId { get; set; }
        public DateTime DateTime { get; set; }
        [JsonIgnore]
        public ulong AuthorId { get => UInt64.TryParse(_AuthorId, out ulong Id) ? Id : 0; set => _AuthorId = $"{value}"; }
        [JsonIgnore]
        public ulong ChannelId { get => UInt64.TryParse(_ChannelId, out ulong Id) ? Id : 0; set => _ChannelId = $"{value}"; }
        [JsonIgnore]
        public ulong MessageId { get => UInt64.TryParse(_MessageId, out ulong Id) ? Id : 0; set => _MessageId = $"{value}"; }
    }

    public class WebhookOptions
    {
        public string Name { get; set; }
        public Embed Embed { get; set; }
        public string Message { get; set; }
        public SettingType Setting { get; set; }
        public KeyValuePair<ulong, KeyValuePair<ulong, string>> WebhookInfo { get; set; }
    }
}