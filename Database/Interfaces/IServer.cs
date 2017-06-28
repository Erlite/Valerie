using Database.Models;
using Database.Wrappers;
using System.Collections.Generic;

namespace Database.Interfaces
{
    public interface IServer
    {
        char Prefix { get; set; }
        List<string> WelcomeMessages { get; set; }
        ulong MuteRoleID { get; set; }
        int AdminCases { get; set; }
        Wrapper JoinEvent { get; set; }
        Wrapper LeaveEvent { get; set; }
        Wrapper AdminLog { get; set; }
        Wrapper Chatterbot { get; set; }
        List<TagsModel> TagsList { get; set; }
        Dictionary<ulong, string> AFKList { get; set; }
        Dictionary<ulong, int> KarmaList { get; set; }
        List<string> AssignableRoles { get; set; }
    }
}
