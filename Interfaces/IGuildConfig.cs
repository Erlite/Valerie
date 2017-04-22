using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rick.Interfaces
{
    public interface IGuildConfig
    {
        string GuildPrefix { get; set; }
        string WelcomeMessage { get; set; }
        ulong ModChannelID { get; set; }
        bool JoinLogs { get; set; }
        bool LeaveLogs { get; set; }
        bool NameChangesLogged { get; set; }
        bool NickChangesLogged { get; set; }
        bool UserBannedLogged { get; set; }
        bool MessageRecieve { get; set; }
        ulong[] RequiredRoleID { get; set;}
        ulong[] RequiredChannelIDs { get; set; }
        string[] RequiredChannelNames { get; set; }
        Dictionary<string, string> Tags { get; set; }
        Dictionary<string, string> Responses { get; set; }
    }
}
