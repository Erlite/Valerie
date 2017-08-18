using System.Collections.Generic;

namespace Valerie.Handlers.GuildHandler.Models
{
    public class GuildModel
    {
        public string Id { get; set; }
        public string Prefix { get; set; } = "<>";
        public List<string> WelcomeMessages { get; set; } = new List<string>();
        public List<string> LeaveMessages { get; set; } = new List<string>();
        public ulong MuteRoleID { get; set; }
        public int ModCases { get; set; }
        public bool AntiAdvertisement { get; set; }
        public Wrapper JoinEvent { get; set; } = new Wrapper();
        public Wrapper LeaveEvent { get; set; } = new Wrapper();
        public Wrapper ModLog { get; set; } = new Wrapper();
        public Wrapper Chatterbot { get; set; } = new Wrapper();
        public Wrapper Starboard { get; set; } = new Wrapper();
        public List<TagsModel> TagsList { get; set; } = new List<TagsModel>();
        public Dictionary<ulong, string> AFKList { get; set; } = new Dictionary<ulong, string>();
        public KarmaModel KarmaHandler { get; set; } = new KarmaModel();
        public List<string> AssignableRoles { get; set; } = new List<string>();
        public List<Starboard> StarredMessages { get; set; } = new List<Starboard>();
    }
}
