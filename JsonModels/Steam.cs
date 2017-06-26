using System.Collections.Generic;

namespace Rick.JsonModels
{
    public class Newsitem
    {
        public string title { get; set; }
        public string url { get; set; }
        public string contents { get; set; }
        public string feedlabel { get; set; }
    }

    public class Appnews
    {
        public List<Newsitem> newsitems { get; set; }
        public int count { get; set; }
    }

    public class SteamAppNews
    {
        public Appnews appnews { get; set; }
    }


    // Player Summary
    public class SteamPlayer
    {
        public string personaname { get; set; }
        public int lastlogoff { get; set; }
        public string profileurl { get; set; }
        public string avatarfull { get; set; }
        public int personastate { get; set; }
        public string realname { get; set; }
        public string primaryclanid { get; set; }
        public int timecreated { get; set; }
        public string loccountrycode { get; set; }
        public string locstatecode { get; set; }
    }

    public class SummarResponse
    {
        public List<SteamPlayer> players { get; set; }
    }

    public class PlayerSummary
    {
        public SummarResponse response { get; set; }
    }


    // Get owned games
    public class GamesResponse
    {
        public int game_count { get; set; }
    }

    public class OwnedGames
    {
        public GamesResponse response { get; set; }
    }

    // Get recently played
    public class Game
    {
        public string name { get; set; }
    }

    public class RecentGames
    {
        public int total_count { get; set; }
        public List<Game> games { get; set; }
    }

    public class GetRecent
    {
        public RecentGames response { get; set; }
    }
}
