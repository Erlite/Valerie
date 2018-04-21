using System.Collections.Generic;

namespace Valerie.Models
{
    public class GuildModel
    {
        public string Id { get; set; }
        public string Prefix { get; set; }
        public bool IsConfigured { get; set; }
        public List<string> JoinMessages { get; set; } = new List<string>(5);
        public List<string> LeaveMessages { get; set; } = new List<string>(5);
        public List<ulong> AssignableRoles { get; set; } = new List<ulong>(10);
        public XPWrapper ChatXP { get; set; } = new XPWrapper();
        public ModWrapper Mod { get; set; } = new ModWrapper();
        public RedditWrapper Reddit { get; set; } = new RedditWrapper();
        public List<TagWrapper> Tags { get; set; } = new List<TagWrapper>();
        public StarboardWrapper Starboard { get; set; } = new StarboardWrapper();
        public Dictionary<ulong, string> AFK { get; set; } = new Dictionary<ulong, string>();
        public KeyValuePair<ulong, KeyValuePair<ulong, string>> JoinWebhook { get; set; }
        public KeyValuePair<ulong, KeyValuePair<ulong, string>> LeaveWebhook { get; set; }
        public KeyValuePair<ulong, KeyValuePair<ulong, string>> CleverbotWebhook { get; set; }
        public List<MessageWrapper> DeletedMessages { get; set; } = new List<MessageWrapper>();
        public Dictionary<ulong, UserProfile> Profiles { get; set; } = new Dictionary<ulong, UserProfile>();
    }
}