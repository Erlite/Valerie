using System;
using System.IO;
using Valerie.Enums;
using System.Drawing;
using System.Diagnostics;
using Console = Colorful.Console;

namespace Valerie.Services
{
    public class LogService
    {
        readonly static object Lock = new object();

        static void FileLog(string Message)
        {
            lock (Lock)
                using (var Writer = File.AppendText($"{Directory.GetCurrentDirectory()}/log.txt"))
                    Writer.WriteLine(Message);
        }

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
            FileLog($"[{Date}] [{Source}] {Text}");
        }

        public void PrintApplicationInformation()
        {
            string Arch = Environment.Is64BitOperatingSystem ? "x64" : "x32";
            Console.WriteAscii("VALERIE", Color.Pink);
            Append("-> INFORMATION\n", Color.Crimson);
            Append(
                "      Author   :  Yucked\n" +
                "      Version  :  18.4.X - Stable\n" +
                "      Discord  :  Discord.me/Glitched\n\n", Color.Bisque);
            Append("-> PACKAGES\n", Color.Crimson);
            Append(
                $"      Discord  :  {Discord.DiscordConfig.Version}\n" +
                $"      RavenDB  :  {Raven.Client.Properties.RavenVersionAttribute.Instance.FullVersion}\n" +
                $"      Cookie   :  \n" +
                $"      Colorful :  1.2.6\n\n", Color.Bisque);
            FileLog($"\n\n=================================[ {DateTime.Now} ]=================================\n\n");
        }
    }
}