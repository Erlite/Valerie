using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using DiscordBot.Services;

namespace DiscordBot.Handlers
{
    public class GiftsHandler : GiftService
    {
        public ulong UID { get; protected set; }
        public DateTime LastUpdate { get; protected set; } = DateTime.UtcNow;

        public override string FileName => Path.Combine("Config", "Users", $"{UID}.json");

        public Dictionary<ulong, uint> XP { get; set; } = new Dictionary<ulong, uint>();

        public GiftsHandler(ulong UID)
        {
            this.UID = UID;
        }

        public override async Task Behaviour()
        {
            await Task.Run(() =>
            {
                if (!File.Exists(FilePath))
                {
                    LastUpdate = DateTime.UtcNow;
                    this.SaveJson<GiftsHandler>();
                }
            });
        }

        public GiftsHandler GivePoints(ulong GUID, uint points)
        {
            uint givePoints =
                HasPoints(GUID)
                ? points + XP[GUID]
                : points;

            SetPoints(GUID, givePoints);
            return this;
        }

        public GiftsHandler TakePoints(ulong GUID, uint points)
        {
            uint value;
            if (XP.TryGetValue(GUID, out value))
                SetPoints(GUID, value - points);
            return this;
        }

        public GiftsHandler SetPoints(ulong GUID, uint points)
        {
            XP[GUID] = Math.Max(0, points);
            this.SaveJson<GiftsHandler>();
            return this;
        }

        public static Task<IReadOnlyCollection<GiftsHandler>> GetAll()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Config", "users");
            var files = Directory.GetFiles(path, "*.json").ToList();
            var configs = new List<GiftsHandler>();
            files.ForEach(x => configs.Add(Load<GiftsHandler>(File.ReadAllText(x))));
            return Task.FromResult<IReadOnlyCollection<GiftsHandler>>(configs.AsReadOnly());
        }

        public bool HasPoints(ulong GUID) =>
            XP.ContainsKey(GUID) && XP[GUID] > 0;

    }
}