using Rick.Classes;
using System.Collections.Generic;

namespace Rick.Interfaces
{
    public interface IGuildInterface
    {
        string GuildPrefix { get; set; }
        string WelcomeMessage { get; set; }
        ulong ModChannelID { get; set; }
        ulong MuteRoleId { get; set; }
        int CaseNumber { get; set; }
        bool JoinLogs { get; set; }
        bool LeaveLogs { get; set; }
        bool NameChangesLogged { get; set; }
        bool NickChangesLogged { get; set; }
        bool UserBannedLogged { get; set; }
        List<ulong> RequiredRoleIDs { get; set;}
        List<ulong> RequiredChannelIDs { get; set; }
        List<string> RequiredChannelNames { get; set; }
        List<Tags> TagsList { get; set; }
        Dictionary<ulong, string> AfkList { get; set; }
        Dictionary<ulong, int> Karma { get; set; }
    }
}
