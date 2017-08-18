using System.Collections.Generic;

namespace Valerie.Handlers.GuildHandler.Models
{
    public class KarmaModel
    {
        public bool IsKarmaEnabled { get; set; }
        public int MaxRolesLevel { get; set; }
        public List<string> BlacklistRoles { get; set; } = new List<string>();
        public Dictionary<ulong, int> LevelUpRoles { get; set; } = new Dictionary<ulong, int>();
        public Dictionary<ulong, int> UsersList { get; set; } = new Dictionary<ulong, int>();
    }
}
