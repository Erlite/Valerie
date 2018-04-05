using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Valerie.Models
{
    public class GuildModel
    {
        public string Id { get; set; }
        public string Prefix { get; set; }
        public bool IsConfigured { get; set; }
        public string _JoinChannel { get; set; }
        public string _LeaveChannel { get; set; }
        public string _ChatterChannel { get; set; }
        public List<string> JoinMessages { get; set; } = new List<string>(5);
        public List<string> LeaveMessages { get; set; } = new List<string>(5);
        public List<ulong> AssignableRoles { get; set; } = new List<ulong>(10);
        public XPWrapper ChatXP { get; set; } = new XPWrapper();
        public ModWrapper Mod { get; set; } = new ModWrapper();
        public RedditWrapper Reddit { get; set; } = new RedditWrapper();
        public List<TagWrapper> Tags { get; set; } = new List<TagWrapper>();
        public StarboardWrapper Starboard { get; set; } = new StarboardWrapper();
        public Dictionary<ulong, string> AFK { get; set; } = new Dictionary<ulong, string>();
        public Dictionary<ulong, UserProfile> Profiles { get; set; } = new Dictionary<ulong, UserProfile>();
        [JsonIgnore]
        public ulong JoinChannel { get => UInt64.TryParse(_JoinChannel, out ulong Id) ? Id : 0; set => _JoinChannel = $"{value}"; }
        [JsonIgnore]
        public ulong LeaveChannel { get => UInt64.TryParse(_LeaveChannel, out ulong Id) ? Id : 0; set => _LeaveChannel = $"{value}"; }
        [JsonIgnore]
        public ulong ChatterChannel { get => UInt64.TryParse(_ChatterChannel, out ulong Id) ? Id : 0 set => _ChatterChannel = $"{value}"; }
    }
}