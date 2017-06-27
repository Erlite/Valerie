namespace Rick.Wrappers
{
    public class APIsWrapper
    {
        public string BingKey { get; set; }
        public string MashapeKey { get; set; }
        public string GoogleKey { get; set; }
        public string SearchEngineID { get; set; }
        public string CleverBotKey { get; set; }
        public string SteamKey { get; set; }
        public string GiphyKey { get; set; } = "dc6zaTOxFJmzC";
        public TwitterWrapper TwitterKeys { get; set; } = new TwitterWrapper();
    }
}
