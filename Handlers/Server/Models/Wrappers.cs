using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Valerie.Handlers.Server.Models
{
    public class DefaultWrapper
    {
        public bool IsEnabled { get; set; }
        public string TextChannel { get; set; }
    }

    public class EridiumWrapper
    {
        public bool IsEridiumEnabled { get; set; }
        public bool IsDMEnabled { get; set; }
        public int MaxRoleLevel { get; set; } = 50;
        public string LevelUpMessage { get; set; }
        public IList<string> BlacklistedRoles { get; set; } = new List<string>();
        public ConcurrentDictionary<ulong, int> LevelUpRoles { get; set; } = new ConcurrentDictionary<ulong, int>();
        public ConcurrentDictionary<ulong, int> UsersList { get; set; } = new ConcurrentDictionary<ulong, int>();
    }

    public class StarboardWrapper
    {
        public bool IsEnabled { get; set; }
        public string TextChannel { get; set; }
        public IList<StarboardMessages> StarboardMessages { get; set; } = new List<StarboardMessages>();
    }

    public class StarboardMessages
    {
        public string MessageId { get; set; }
        public string ChannelId { get; set; }
        public string StarboardMessageId { get; set; }
        public int Stars { get; set; }
    }

    public class TagWrapper
    {
        public string Name { get; set; }
        public string Response { get; set; }
        public string CreationDate { get; set; }
        public string Owner { get; set; }
        public int Uses { get; set; }
    }

    public class ModWrapper
    {
        public int Cases { get; set; }
        public string MuteRole { get; set; }
        public bool AntiAdvertisement { get; set; }
        public bool IsAutoModEnabled { get; set; }
        public bool IsEnabled { get; set; }
        public string TextChannel { get; set; }
        public ConcurrentDictionary<ulong, int> Warnings { get; set; } = new ConcurrentDictionary<ulong, int>();
    }
}
