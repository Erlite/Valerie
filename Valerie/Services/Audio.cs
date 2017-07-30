using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using YoutubeExplode;
using YoutubeExplode.Models;
using Valerie.Handlers;
using Valerie.Extensions;

namespace Valerie.Services
{
    public class Audio
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels =
            new ConcurrentDictionary<ulong, IAudioClient>();

        public async Task JoinAudioChannelAsync(IGuild Guild, IVoiceChannel VoiceChannel)
        {
            if (ConnectedChannels.TryGetValue(Guild.Id, out IAudioClient Value)) return;
            if (VoiceChannel.Guild.Id != Guild.Id) return;
            var Client = await VoiceChannel.ConnectAsync();
            ConnectedChannels.TryAdd(Guild.Id, Client);
        }

        public async Task LeaveAudioChannelAsync(IGuild Guild)
        {
            if (ConnectedChannels.TryRemove(Guild.Id, out IAudioClient Client))
                await Client.StopAsync();
        }

        public async Task SendAudioAsync(IGuild Guild, IMessageChannel Channel, string Input)
        {
            var Client = new YoutubeClient();
            if (Input.ToLower().Contains("youtube.com"))
                Input = YoutubeClient.ParseVideoId(Input);
            else
                Input = StringExtension.Youtube(Input);

            var VidInfo = await Client.GetVideoInfoAsync(Input);
            var AudioBit = VidInfo.AudioStreams.OrderBy(x => x.Bitrate).Last();
            var Name = new Regex("[^a-zA-Z0-9 -]").Replace(VidInfo.Title, "");
            var VideoPath = $"{MainHandler.CacheFolder}/{Name}.{AudioBit.Container.GetFileExtension()}";
            if (!File.Exists(VideoPath))
            {
                await Channel.SendMessageAsync($"Downloading **{Name}** ...");
                using (var getMedia = await Client.GetMediaStreamAsync(AudioBit))
                using (var save = File.Create(VideoPath))
                    await getMedia.CopyToAsync(save);
            }

            if (ConnectedChannels.TryGetValue(Guild.Id, out IAudioClient Audio))
            {
                var Embed = Vmbed.Embed(VmbedColors.Pastel, $"Now Playing {Name}", ThumbUrl: VidInfo.ImageStandardResUrl);
                Embed.AddInlineField("Author", VidInfo.Author.Name);
                Embed.AddInlineField("Duartion", VidInfo.Duration);
                Embed.AddInlineField("Views", VidInfo.ViewCount);
                Embed.AddInlineField("Likes", VidInfo.LikeCount);
                await Channel.SendMessageAsync("", embed: Embed);
                var Stream = Audio.CreatePCMStream(AudioApplication.Music);
                await CreateStream(VideoPath).StandardOutput.BaseStream.CopyToAsync(Stream);
                await Stream.FlushAsync();
            }
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
