using System.Collections.Generic;

namespace Rick.Interfaces
{
    public interface IGuildInterface
    {
        string GuildPrefix { get; set; }
        string WelcomeMessage { get; set; }
        ulong ModChannelID { get; set; }
        bool JoinLogs { get; set; }
        bool LeaveLogs { get; set; }
        bool NameChangesLogged { get; set; }
        bool NickChangesLogged { get; set; }
        bool UserBannedLogged { get; set; }
        bool AutoRespond { get; set; }
        IEnumerable<ulong> RequiredRoleIDs { get; set;}
        IEnumerable<ulong> RequiredChannelIDs { get; set; }
        IEnumerable<string> RequiredChannelNames { get; set; }
        Dictionary<string, string> Tags { get; set; }
        Dictionary<string, string> Responses { get; set; }
    }
}
