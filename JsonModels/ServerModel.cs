using System.Collections.Generic;

namespace Valerie.JsonModels
{
    public class ServerModel
    {
        public string Id { get; set; }
        public string Prefix { get; set; }
        public string JoinChannel { get; set; }
        public string LeaveChannel { get; set; }
        public string ChatterChannel { get; set; }
        public List<string> JoinMessages { get; set; } = new List<string>(5);
        public List<string> LeaveMessages { get; set; } = new List<string>(5);
        public List<ulong> AssignableRoles { get; set; } = new List<ulong>(10);
        public XPWrapper ChatXP { get; set; } = new XPWrapper();
        public ModWrapper ModLog { get; set; } = new ModWrapper();
        public RedditWrapper Reddit { get; set; } = new RedditWrapper();
        public List<TagWrapper> Tags { get; set; } = new List<TagWrapper>();
        public StarboardWrapper Starboard { get; set; } = new StarboardWrapper();
        public Dictionary<ulong, string> AFKUsers { get; set; } = new Dictionary<ulong, string>();
        public Dictionary<ulong, UserProfile> Profiles { get; set; } = new Dictionary<ulong, UserProfile>();
    }
}