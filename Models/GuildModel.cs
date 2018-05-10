using System.Collections.Generic;

namespace Valerie.Models
{
    public class GuildModel
    {
        public string Id { get; set; }
        public string Prefix { get; set; }
        public bool IsConfigured { get; set; }
        public XPWrapper ChatXP { get; set; } = new XPWrapper();
        public ModWrapper Mod { get; set; } = new ModWrapper();
        public RedditWrapper Reddit { get; set; } = new RedditWrapper();
        public List<string> JoinMessages { get; set; } = new List<string>(5);
        public List<string> LeaveMessages { get; set; } = new List<string>(5);
        public List<TagWrapper> Tags { get; set; } = new List<TagWrapper>();
        public List<ulong> AssignableRoles { get; set; } = new List<ulong>(10);
        public StarboardWrapper Starboard { get; set; } = new StarboardWrapper();
        public WebhookWrapper JoinWebhook { get; set; } = new WebhookWrapper();
        public WebhookWrapper LeaveWebhook { get; set; } = new WebhookWrapper();
        public WebhookWrapper CleverbotWebhook { get; set; } = new WebhookWrapper();
        public Dictionary<ulong, string> AFK { get; set; } = new Dictionary<ulong, string>();
        public List<MessageWrapper> DeletedMessages { get; set; } = new List<MessageWrapper>();
        public Dictionary<ulong, UserProfile> Profiles { get; set; } = new Dictionary<ulong, UserProfile>();
    }
}