using System;
using System.IO;
using Valerie.Enums;
using System.Drawing;
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
            Console.WriteAscii("VALERIE", Color.Pink);
            Append("-> INFORMATION\n", Color.Crimson);
            Append("    Author: Yucked | Discord: Https://Discord.me/Glitched\n", Color.Bisque);
            Append($"\n=======================[ {DateTime.UtcNow} ]=======================\n", Color.Crimson);
            FileLog($"\n\n=================================[ {DateTime.Now} ]=================================\n\n");
        }
    }
}