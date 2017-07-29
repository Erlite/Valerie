namespace Rick.Handlers.ConfigHandler.Models
{
    public class KeysModel
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

    public class TwitterWrapper
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }
}
