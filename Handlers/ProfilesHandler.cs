using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rick.Classes;
using System.IO;
using Discord.WebSocket;
using System;
using System.Linq;

namespace Rick.Handlers
{
    public class ProfilesHandler
    {
        public const string path = "ProfilesDB.json";

        [JsonProperty("ProfilesDatabase")]
        public List<UsersProfile> ProfilesDatabase { get; set; } = new List<UsersProfile>();

        public static async Task<ProfilesHandler> LoadDatabaseAsync()
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<ProfilesHandler>(json);
            }
            var newConfig = await CreateNewDBAsync();
            return newConfig;
        }

        public static async Task<ProfilesHandler> CreateNewDBAsync()
        {
            ProfilesHandler result;
            result = new ProfilesHandler();

            using (var configStream = File.Create(Path.Combine(Directory.GetCurrentDirectory(), path)))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    var save = JsonConvert.SerializeObject(result, Formatting.Indented);
                    await configWriter.WriteAsync(save);
                }
            }
            return result;
        }

        public static async Task SaveAsync(string path, ProfilesHandler DB)
            => File.WriteAllText(path, await Task.Run(() => JsonConvert.SerializeObject(DB, Formatting.Indented)));

        public async Task ProfileKarma(SocketUserMessage message, SocketGuild gld)
        {
            if (message.Author.IsBot) return;

            var hasUser = ProfilesDatabase.FirstOrDefault(x => x.UserID == message.Author.Id);

            if (hasUser == null)
            {
                var Profile = new UsersProfile
                {
                    UserID = message.Author.Id,
                    Karma = 0,
                    ProfileMsg = "Who am I? What am I??? Where am I? WHAT'S GOING ON !??!??!",
                Level = 0,
                };
                ProfilesDatabase.Add(Profile);
            }
            double getKarma = hasUser.Karma;
            var giveKarma = AssignXP(getKarma);

            getKarma += giveKarma;

            hasUser.Karma = getKarma;

            double level = hasUser.Level;
            level = Levels(hasUser.Karma);
            hasUser.Level = level;

            await SaveAsync(path, this);
        }

        private double AssignXP(double XP)
        {
            Random rand = new Random();
            double random = rand.Next(1, 5);
            return 5 * (Math.Pow(2.0, random)) + 50 * random + 100;
        }

        private double Levels(double XP)
        {
            int Level = 0;
            while (XP >= AssignXP(Level))
            {
                XP -= AssignXP(Level);
                Level += 1;
            }
            return Level;
        }
    }
}
