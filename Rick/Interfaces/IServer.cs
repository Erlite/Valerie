using Rick.Wrappers;
using System.Collections.Generic;

namespace Rick.Interfaces
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
    }
}
