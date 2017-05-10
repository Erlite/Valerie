﻿using Rick.Classes;
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
        //bool JoinLogs { get; set; }
        List<EventWrapper> JoinLogs { get; set; }
        //bool LeaveLogs { get; set; }
        List<EventWrapper> LeaveLogs { get; set; }
        //bool NameChangesLogged { get; set; }
        List<EventWrapper> NameChangesLogged { get; set; }
        //bool NickChangesLogged { get; set; }
        List<EventWrapper> NickChangesLogged { get; set; }
        //bool UserBannedLogged { get; set; }
        List<EventWrapper> UserBannedLogged { get; set; }
        bool ChatKarma { get; set; }
        bool ChatterBot { get; set;  }
        List<ulong> RequiredRoleIDs { get; set;}
        List<string> RequiredChannelNames { get; set; }
        List<Tags> TagsList { get; set; }
        Dictionary<ulong, string> AfkList { get; set; }
        Dictionary<ulong, int> Karma { get; set; }
    }
}
