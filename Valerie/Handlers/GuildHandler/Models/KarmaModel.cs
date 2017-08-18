using System.Collections.Generic;

namespace Valerie.Handlers.GuildHandler.Models
{
    public class EridiumModel
    {
        public bool IsEridiumEnabled { get; set; }
        public int MaxRoleLevel { get; set; } = 50;
        public List<string> BlacklistRoles { get; set; } = new List<string>();
        public Dictionary<ulong, int> LevelUpRoles { get; set; } = new Dictionary<ulong, int>();
        public Dictionary<ulong, int> UsersList { get; set; } = new Dictionary<ulong, int>();
    }
}
