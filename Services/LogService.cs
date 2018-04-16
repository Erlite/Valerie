using System;
using System.IO;
using Valerie.Enums;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Valerie.Services
{
    public class LogService
    {
        public void Initialize()
        {
            var LogPath = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");
            if (!File.Exists(LogPath)) File.Create(LogPath);
            PrintApplicationInformation();
        }

        static async Task LogAsync(string Message)
            => await File.AppendAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "log.txt"), Message + Environment.NewLine);

        static void Append(string Text, Color Color)
        {
            Console.ForegroundColor = Color;
            Console.Write(Text);
        }

        public static void Write(LogSource Source, string Text, Color Color)
        {
            Console.Write(Environment.NewLine);
            Append($"-> {DateTime.Now.ToShortTimeString()} ", Color.DarkGray);
            Append($"[{Source}]", Color);
            Append($" {Text}", Color.WhiteSmoke);
            _ = LogAsync($"[{DateTime.Now}] [{Source}] {Text}");
        }

        public void PrintApplicationInformation()
        {
            Console.WriteAscii("VALERIE", Color.Pink);
            Append("-> INFORMATION\n", Color.PaleVioletRed);
            Append(
                "      Author  :  Yucked\n" +
                "      Version :  18.4.X - Rewrite\n" +
                "      Discord :  Discord.me/Glitched\n", Color.Olive);
        }
    }
}