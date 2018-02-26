using System;
using System.Collections.Generic;

namespace Valerie.JsonModels
{
    public class XPWrapper
    {
        public bool IsEnabled { get; set; }
        public string LevelMessage { get; set; }
        public List<ulong> ForbiddenRoles { get; set; } = new List<ulong>(20);
        public Dictionary<ulong, int> LevelRoles { get; set; } = new Dictionary<ulong, int>(20);
        public Dictionary<ulong, int> Rankings { get; set; } = new Dictionary<ulong, int>();
    }

    public class StarboardWrapper
    {
        public string TextChannel { get; set; }
        public List<StarboardMessages> StarboardMessages { get; set; } = new List<StarboardMessages>();
    }

    public class MemoryWrapper
    {
        public string Id { get; set; }
        public float Byte { get; set; }
        public DateTime DailyReward { get; set; }
    }

    public enum Memory
    {
        Byte,
        Kilobyte,
        Megabyte,
        Gigabyte,
        Terabyte,
        Petabyte,
        Exabyte,
        Zettabyte,
        Yottabyte,
        Hellabyte
    }

    public class StarboardMessages
    {
        public int Stars { get; set; }
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
        public Dictionary<ulong, int> Warnings { get; set; } = new Dictionary<ulong, int>();
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

    public class ApplicationKeys
    {
        public string GiphyKey { get; set; } = "dc6zaTOxFJmzC";
        public string GoogleKey { get; set; }
        public string SteamKey { get; set; }
        public string ImgurKey { get; set; }
        public string CleverBotKey { get; set; }
    }

    public class RedditWrapper
    {
        public bool IsEnabled { get; set; }
        public string TextChannel { get; set; }
        public List<string> Subreddits { get; set; } = new List<string>(3);
    }
}