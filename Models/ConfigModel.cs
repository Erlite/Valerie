using System.Collections.Generic;

namespace Valerie.Models
{
    public class ConfigModel
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string JoinMessage { get; set; }
        public List<string> Games { get; set; } = new List<string>();
        public List<ulong> Blacklist { get; set; } = new List<ulong>();
        public List<string> Namespaces { get; set; } = new List<string>();
        public WebhookWrapper ReportWebhook { get; set; } = new WebhookWrapper();
        public Dictionary<string, string> APIKeys { get; set; } = new Dictionary<string, string>()
        { {"Giphy", "dc6zaTOxFJmzC" }, {"Google", "" }, {"Steam", "" }, {"Imgur", "" }, {"Cleverbot", "" } };
    }
}