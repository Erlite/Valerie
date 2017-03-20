using GPB.Services;

namespace GPB.Handlers
{
    public class LogHandler
    {
        public ulong ServerLog { get; set; }
        public ulong ModLog { get; set; }
        public bool JoinsLogged { get; set; }
        public bool LeavesLogged { get; set; }
        public bool NameChangesLogged { get; set; }
        public bool NickChangesLogged { get; set; }
        public bool UserBannedLogged { get; set; }
        public bool ClientLatench { get; set; }

        public LogHandler()
        {

        }

        public LogHandler(LogService s)
        {
            ServerLog = s.ServerLogChannelId;
            ModLog = s.ModLogChannelId;
            JoinsLogged = s.JoinsLogged;
            LeavesLogged = s.LeavesLogged;
            NameChangesLogged = s.NameChangesLogged;
            NickChangesLogged = s.NickChangesLogged;
            UserBannedLogged = s.UserBannedLogged;
            ClientLatench = s.ClientLatency;
        }
    }
}