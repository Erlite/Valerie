using System.Text;

namespace Rick.Extensions
{
    public class StringExtension
    {
        public static string ReplaceWith(string Msg, string Username, string GuildName)
        {
            StringBuilder sb = new StringBuilder(Msg);
            sb.Replace("{user}", Username);
            sb.Replace("{guild}", GuildName);
            return sb.ToString();
        }
    }
}
