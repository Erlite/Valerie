using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Rick.Controllers;
using Discord;
using System.Linq;
using Rick.Attributes;

namespace Rick.Modules
{
    [CheckBlacklist, RequireBotPermission(GuildPermission.SendMessages)]
    public class AudioModule : ModuleBase
    {
        public static Dictionary<ulong, List<string>> Queue = new Dictionary<ulong, List<string>>();

        private Audio Audio;
        public AudioModule(Audio Ad)
        {
            Audio = Ad;
        }

        [Command("Join", RunMode = RunMode.Async), Summary("Joins voice channel")]
        public async Task JoinAsync()
        {
            if ((Context.User as IVoiceState).VoiceChannel == null)
            {
                await ReplyAsync("You are not in a voice channel! Please join a voice channel.");
                return;
            }
            await ReplyAsync($"Joining {(Context.User as IVoiceState).VoiceChannel.Name} channel.");
            await Audio.JoinAudioChannelAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("Leave", RunMode = RunMode.Async), Summary("leaves voice channel")]
        public async Task LeaveAsync()
        {
            await Audio.LeaveAudioChannelAsync(Context.Guild);
            await ReplyAsync("Leaving Audio Channel.");
        }

        [Command("Play", RunMode = RunMode.Async), Summary("Plays musik!"), Remarks("Play Song Name")]
        public async Task PlayAsync()
        {
            List<string> list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
            {
                Queue.TryGetValue(Context.Guild.Id, out list);
            }
            else
            {
                await ReplyAsync("Guild's queue list is empty! Please add some songs first by using the `QAdd` command.");
                return;
            }

            while (list.Count > 0)
            {
                await Audio.LeaveAudioChannelAsync(Context.Guild);
                await Audio.JoinAudioChannelAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
                await Audio.SendAudioAsync(Context.Guild, Context.Channel, list.First());
                list.RemoveAt(0);
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
                Queue.TryGetValue(Context.Guild.Id, out list);
            }

            if (list.Count == 0)
                await Audio.LeaveAudioChannelAsync(Context.Guild);
        }

        [Command("Skip", RunMode = RunMode.Async), Summary("Skips a song")]
        public async Task SkipAsync()
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);

            if (list.Count > 0)
            {
                list.RemoveAt(0);
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
            }
            await PlayAsync();
        }

        [Command("Queue", RunMode = RunMode.Async), Summary("Lists all the songs in a queue.")]
        public async Task QueueList(int page = 0)
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);
            var songlist = new List<string>();
            if (list.Count > 0)
            {
                var i = 0;
                foreach (var item in list)
                {
                    songlist.Add($"`{i}` - {item}");
                    i++;
                }
                if (page <= 0)
                {
                    if (i > 10)
                        await ReplyAsync(
                            $"**Page 0**\nHere are the first 10 songs in your playlist (total = {i}):\n{string.Join("\n", songlist.Take(10).ToArray())}");
                    else
                        await ReplyAsync(string.Join("\n", songlist.ToArray()));
                }
                else
                {
                    var response = string.Join("\n", songlist.Skip(page * 10).Take(10).ToArray());
                    if (response == "")
                        await ReplyAsync($"**Page {page}** of your playlist:\nEmpty");
                    await ReplyAsync($"**Page {page}** of your playlist:\n{response}");
                }
            }
            else
            {
                await ReplyAsync("The Queue is empty :(");
            }
        }

        [Command("QAdd", RunMode = RunMode.Async), Summary("Adds a song to the queue list.")]
        public async Task QueueSongAsync([Remainder] string linkOrSearchTerm)
        {
            if (string.IsNullOrWhiteSpace(linkOrSearchTerm))
                return;

            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);
            list.Add(linkOrSearchTerm);

            Queue.Remove(Context.Guild.Id);
            Queue.Add(Context.Guild.Id, list);
            await ReplyAsync(
                $"**{linkOrSearchTerm}** has been added to the end of the queue. \n" +
                $"Queue Length: **{list.Count}**");
        }

        [Command("QClear", RunMode = RunMode.Async), Summary("Clears the queue.")]
        public async Task ClearQue()
        {
            var list = new List<string>();
            if (Queue.ContainsKey(Context.Guild.Id))
                Queue.TryGetValue(Context.Guild.Id, out list);
            if (list.Count > 0)
            {
                list.Clear();
                await ReplyAsync("Queue has been cleared.");
                Queue.Remove(Context.Guild.Id);
                Queue.Add(Context.Guild.Id, list);
            }
        }
    }
}
