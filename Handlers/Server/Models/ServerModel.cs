﻿using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Valerie.Handlers.Server.Models
{
    public class ServerModel
    {
        public string Id { get; set; }
        public string Prefix { get; set; }
        public string JoinChannel { get; set; }
        public string LeaveChannel { get; set; }
        public string ChatterChannel { get; set; }
        public IList<string> WelcomeMessages { get; set; } = new List<string>();
        public IList<string> LeaveMessages { get; set; } = new List<string>();
        public ConcurrentDictionary<ulong, string> AFKList { get; set; } = new ConcurrentDictionary<ulong, string>();
        public ModWrapper ModLog { get; set; } = new ModWrapper();
        public List<TagWrapper> TagsList { get; set; } = new List<TagWrapper>();
        public EridiumWrapper EridiumHandler { get; set; } = new EridiumWrapper();
        public StarboardWrapper Starboard { get; set; } = new StarboardWrapper();
        public IList<string> AssignableRoles { get; set; } = new List<string>();
        public ConcurrentDictionary<ulong, ConcurrentDictionary<int, string>> ToDo { get; set; } =
            new ConcurrentDictionary<ulong, ConcurrentDictionary<int, string>>();
    }
}