using Rick.Models;
using System.Collections.Generic;

namespace Rick.Interfaces
{
    public interface IServer
    {
        string GuildPrefix { get; set; }
        string WelcomeMessage { get; set; }
        ulong ModChannelID { get; set; }
        ulong MuteRoleId { get; set; }
        int CaseNumber { get; set; }
        bool ChatKarma { get; set; }
        bool ChatterBot { get; set; }
        EventWrapper JoinEvent { get; set; }
        EventWrapper LeaveEvent { get; set; }
        EventWrapper UserBanned { get; set; }
        List<ulong> RequiredRoleIDs { get; set;}
        List<string> RequiredChannelNames { get; set; }
        List<TagsModel> TagsList { get; set; }
        Dictionary<ulong, string> AfkList { get; set; }
        Dictionary<ulong, int> Karma { get; set; }
    }
}
