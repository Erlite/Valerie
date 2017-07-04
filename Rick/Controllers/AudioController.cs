using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;

namespace Rick.Controllers
{
    class AudioController
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels =
            new ConcurrentDictionary<ulong, IAudioClient>();

        public async Task JoinAudio(IGuild Guild, IVoiceChannel VoiceChannel)
        {
            if (ConnectedChannels.TryGetValue(Guild.Id, out IAudioClient Value)) return;
            if (VoiceChannel.Guild.Id != Guild.Id) return;

            var Client = await VoiceChannel.ConnectAsync();
            ConnectedChannels.TryAdd(Guild.Id, Client);
        }

        public async Task LeaveAudio(IGuild Guild)
        {
            if (ConnectedChannels.TryRemove(Guild.Id, out IAudioClient Client))
                await Client.StopAsync();
        }

        private static Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}
