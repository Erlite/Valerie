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
        public List<ulong> Admins { get; set; } = new List<ulong>(10);
        public List<string> JoinMessages { get; set; } = new List<string>(5);
        public List<string> LeaveMessages { get; set; } = new List<string>(5);
        public List<ulong> AssignableRoles { get; set; } = new List<ulong>(10);
        public List<ulong> BlacklistedUsers { get; set; } = new List<ulong>(50);
        public XPWrapper ChatXP { get; set; } = new XPWrapper();
        public ModWrapper ModLog { get; set; } = new ModWrapper();
        public RedditWrapper Reddit { get; set; } = new RedditWrapper();
        public List<TagWrapper> Tags { get; set; } = new List<TagWrapper>();
        public List<UserProfile> Profiles { get; set; } = new List<UserProfile>();
        public StarboardWrapper Starboard { get; set; } = new StarboardWrapper();
        public Dictionary<ulong, string> AFKUsers { get; set; } = new Dictionary<ulong, string>();
    }
}