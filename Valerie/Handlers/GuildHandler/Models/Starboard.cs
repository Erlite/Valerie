namespace Valerie.Handlers.GuildHandler.Models
{
    public class Starboard
    {
        public string MessageId { get; set; }
        public string ChannelId { get; set; }
        public string StarboardMessageId { get; set; }
        public int Stars { get; set; }
    }
}
