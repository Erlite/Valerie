using System;
using System.Collections.Generic;

namespace Valerie.Models
{
    public class XPWrapper
    {
        public bool IsEnabled { get; set; }
        public string LevelMessage { get; set; }
        public List<ulong> ForbiddenRoles { get; set; } = new List<ulong>(20);
        public Dictionary<ulong, int> LevelRoles { get; set; } = new Dictionary<ulong, int>(20);
    }

    public class StarboardWrapper
    {
        public string TextChannel { get; set; }
        public List<StarboardMessage> StarboardMessages { get; set; } = new List<StarboardMessage>();
    }

    public class StarboardMessage
    {
        public int Stars { get; set; }
        public string AuthorId { get; set; }
        public string ChannelId { get; set; }
        public string MessageId { get; set; }
        public string StarboardMessageId { get; set; }
    }

    public class TagWrapper
    {
        public int Uses { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Response { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class ModWrapper
    {
        public string MuteRole { get; set; }
        public int MaxWarnings { get; set; }
        public string TextChannel { get; set; }
        public string AutoAssignRole { get; set; }
        public bool IsAutoModEnabled { get; set; }
        public List<string> BadWords { get; set; } = new List<string>(50);
        public List<string> BlockedUrls { get; set; } = new List<string>(50);
        public List<CaseWrapper> Cases { get; set; } = new List<CaseWrapper>();
    }

    public class CaseWrapper
    {
        public string Reason { get; set; }
        public int CaseNumber { get; set; }
        public string UserInfo { get; set; }
        public string MessageId { get; set; }
        public CaseType CaseType { get; set; }
        public string ResponsibleMod { get; set; }
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
        public string TextChannel { get; set; }
        public List<string> Subreddits { get; set; } = new List<string>(5);
    }

    public class UserProfile
    {
        public int Bytes { get; set; }
        public int ChatXP { get; set; }
        public int Warnings { get; set; }
        public bool IsAdmin { get; set; }
        public int DailyStreak { get; set; }
        public bool IsBlacklisted { get; set; }
        public DateTime? DailyReward { get; set; }
        public Dictionary<string, int> Commands { get; set; } = new Dictionary<string, int>();
    }
}