using System;
using System.IO;
using Valerie.Enums;
using System.Drawing;
using System.Diagnostics;
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
            string Date = DateTime.Now.ToShortTimeString().Length <= 7
                ? $"0{DateTime.Now.ToShortTimeString()}" : DateTime.Now.ToShortTimeString();
            Console.Write(Environment.NewLine);
            Append($"-> {Date} ", Color.DarkGray);
            Append($"[{Source}]", Color);
            Append($" {Text}", Color.WhiteSmoke);
            _ = LogAsync($"[{DateTime.Now}] [{Source}] {Text}");
        }

        public void PrintApplicationInformation()
        {
            string Arch = Environment.Is64BitOperatingSystem ? "x64" : "x32";
            Console.WriteAscii("VALERIE", Color.Pink);
            Append("-> INFORMATION\n", Color.Crimson);
            Append(
                "      Author   :  Yucked\n" +
                "      Version  :  18.4.X - Stable\n" +
                "      Discord  :  Discord.me/Glitched\n" +
                $"      Arch     : {Arch}\n" +
                $"      PID      : {Process.GetCurrentProcess().Id}\n\n", Color.Bisque);
            Append("-> PACKAGES\n", Color.Crimson);
            Append(
                $"      Discord  :  {Discord.DiscordConfig.Version}\n" +
                $"      RavenDB  :  {Raven.Client.Properties.RavenVersionAttribute.Instance.FullVersion}\n" +
                $"      Cookie   :  \n" +
                $"      Colorful :  1.2.6\n\n", Color.Bisque);
        }
    }
}