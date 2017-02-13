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
using Nygma.Services;
using Nygma.Utils;

namespace Nygma.Modules
{
    public class ReminderModule : ModuleBase
    {
        internal static readonly Regex Regex = new Regex(@"(?<digits>\d+)\s+(?<unit>\w+)(?:\s+(?<reason>[\w\d\s':/`\\\.,!?]+))?");

        [Command("reminder")]
        [Alias("remindme")]
        [Summary("Adds an event that will DM you at a specified day/hour/minute/second in the future")]
        public async Task AddReminderEventCommand([Remainder] string message)
        {
            Match matches = Regex.Match(message);
            if (matches.Success)
            {
                var milliseconds = 0;
                switch (matches.Groups["unit"].Value.ToLower()[0])
                {
                    case 's':
                        milliseconds = int.Parse(matches.Groups["digits"].Value) * 1000;
                        break;
                    case 'm':
                        milliseconds = int.Parse(matches.Groups["digits"].Value) * 1000 * 60;
                        break;
                    case 'h':
                        milliseconds = int.Parse(matches.Groups["digits"].Value) * 1000 * 60 * 60;
                        break;
                    case 'd':
                        milliseconds = int.Parse(matches.Groups["digits"].Value) * 1000 * 60 * 60 * 24;
                        break;
                }

                ReminderService.ReminderEvent reminderEvent = new ReminderService.ReminderEvent
                {
                    RequestedTime = DateTime.Now.AddMilliseconds(milliseconds),
                    UserId = Context.User.Id,
                    Reason = matches.Groups["reason"].Success ? matches.Groups["reason"].Value : "No specified reason"
                };

                if (ReminderService.ReminderList.Count == 0)
                {
                    ReminderService.ReminderList.AddFirst(reminderEvent);
                    ReminderService.SetTimer(reminderEvent.RequestedTime);
                }
                else
                {
                    var laternode =
                        ReminderService.ReminderList.EnumerateNodes()
                            .FirstOrDefault(x => x.Value.RequestedTime.CompareTo(reminderEvent.RequestedTime) > 0);
                    if (laternode == null)
                    {
                        ReminderService.ReminderList.AddLast(reminderEvent);
                    }
                    else
                    {
                        ReminderService.ReminderList.AddBefore(laternode, reminderEvent);
                    }
                }
                ReminderService.Save();
                //await ReplyAsync($"Reminder set for {reminderEvent.RequestedTime.ToUniversalTime().ToString("g", new CultureInfo("en-US"))} UTC with reason: {reminderEvent.Reason}");
                await Context.Channel.SendEmbedAsync(EmbedAsync.Reminder("Reminder Set", Context.User, reminderEvent.Reason, reminderEvent.RequestedTime.ToUniversalTime().ToString("g", new CultureInfo("en-US")), Context.User.AvatarUrl));
            }
        }
    }
}
