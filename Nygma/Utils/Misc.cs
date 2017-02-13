using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Nygma.Handlers;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace Nygma.Utils
{
    public static class Misc
    {
        public static Color RandColor()
        {
            Random r = new Random();
            return new Color((byte)r.Next(255), (byte)r.Next(255), (byte)r.Next(255));
        }

        public static string LimitLength(this string str, int maxLengh)
        {
            if (str.Length <= maxLengh) return str;
            return str.Substring(0, maxLengh);
        }

        public static string StripTags(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        public static async Task<bool> ValidateQuery(IMessageChannel ch, string query)
        {
            if (!string.IsNullOrEmpty(query.Trim())) return true;
            await ch.SendMessageAsync("Please specify search parameters.");
            return false;
        }

        public static string TrimTo(this string str, int maxLength, bool hideDots = false)
        {
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength), $"Argument {nameof(maxLength)} can't be negative.");
            if (maxLength == 0)
                return string.Empty;
            if (maxLength <= 3)
                return string.Concat(str.Select(c => '.'));
            if (str.Length < maxLength)
                return str;
            return string.Concat(str.Take(maxLength - 3)) + (hideDots ? "" : "...");
        }
    }

    public static class Wiki
    {
        public static string HttpGet(this HttpClient client, string relativeUrl)
        {
            var response = client.GetAsync(relativeUrl).Result;
            var content = response.Content;
            return content.ReadAsStringAsync().Result;
        }
    }

    public class DefineModel
    {
        public List<Result> Results { get; set; }
    }

    public class Sens
    {
        public object Definition { get; set; }
        public List<Example> Examples { get; set; }
        public GramaticalInfo Gramatical_info { get; set; }
    }

    public class Result
    {
        public string Part_of_speech { get; set; }
        public List<Sens> Senses { get; set; }
        public string Url { get; set; }
    }

    public class Example
    {
        public List<Audio> audio { get; set; }
        public string text { get; set; }
    }

    public class GramaticalInfo
    {
        public string type { get; set; }
    }

    public class Audio
    {
        public string url { get; set; }
    }

    public class Reputation
    {
        public ulong Id { get; set; }
        public int Rep { get; set; }
    }

    public class TodoList
    {
        public ulong Id { get; set; }
        public List<string> ListItems { get; set; }
    }

    public class Message
    {
        public string Author;
        public string Content;
        public DateTimeOffset Timestamp;
    }

}