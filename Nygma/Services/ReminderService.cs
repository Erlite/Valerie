using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json;
using Discord.WebSocket;

namespace Nygma.Services
{
    public static class ReminderService
    {
        private static DiscordSocketClient client;
        public static string RootDirectory = Directory.GetCurrentDirectory();
        public static string ReminderPath => RootDirectory + "/Extras/ReminderList.json";

        internal static Timer ReminderTimer;
        internal static readonly LinkedList<ReminderEvent> ReminderList = File.Exists(ReminderPath) ?
                JsonConvert.DeserializeObject<LinkedList<ReminderEvent>>(File.ReadAllText(ReminderPath)) :
                new LinkedList<ReminderEvent>();

        static ReminderService()
        {
            List<ReminderEvent> deleteBuffer = new List<ReminderEvent>(ReminderList.Count);
            foreach (var reminder in ReminderList)
            {
                if (ReminderList.First.Value.RequestedTime <= DateTime.Now)
                {
                    deleteBuffer.Add(reminder);
                }
            }
            DeleteList(deleteBuffer);
            if (ReminderList.Count != 0)
            {
                SetTimer(ReminderList.First.Value.RequestedTime);
            }
        }

        internal static void SetTimer(DateTime newTimer)
        {
            TimeSpan interval = newTimer - DateTime.Now;
            ReminderTimer?.Dispose();
            ReminderTimer = new Timer(CheckReminders, null, interval, TimeSpan.FromMinutes(1));

        }

        private static async Task CheckReminders()
        {
            List<ReminderEvent> deleteBuffer = new List<ReminderEvent>(ReminderList.Count);

            foreach (ReminderEvent reminder in ReminderList)
            {
                if (reminder.RequestedTime.CompareTo(DateTime.Now) <= 0)
                {
                    var channel = await client.GetUser(reminder.UserId).CreateDMChannelAsync();

                    await channel.SendMessageAsync($"Reminder: {reminder.Reason}");

                    deleteBuffer.Add(reminder);
                    if (ReminderList.Count == 0)
                    {
                        ReminderTimer.Dispose();
                        break;
                    }
                }
            }
            DeleteList(deleteBuffer);
            if (ReminderList.Count != 0)
            {
                SetTimer(ReminderList.First.Value.RequestedTime);
            }
            Save();
        }

        private static void DeleteList(List<ReminderEvent> deleteBuffer)
        {
            foreach (var lapsedEvent in deleteBuffer)
            {
                ReminderList.Remove(lapsedEvent);
            }
        }

        private static async void CheckReminders(object sender)
        {
            try
            {
                await CheckReminders();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex + ex.Message);
            }
        }

        internal static void Save()
        {
            File.WriteAllText(ReminderPath, JsonConvert.SerializeObject(ReminderList));
        }

        internal struct ReminderEvent
        {
            public DateTime RequestedTime { get; set; }
            public ulong UserId { get; set; }
            public string Reason { get; set; }
        }

        public static bool Init()
        {
            return true;
        }
    }
    public static class LinkedListExtensions
    {
        public static IEnumerable<LinkedListNode<T>> EnumerateNodes<T>(this LinkedList<T> list)
        {
            var node = list.First;
            while (node != null)
            {
                yield return node;
                node = node.Next;
            }
        }
    }
}