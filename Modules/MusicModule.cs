using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Audio;
using Rick.Services;

namespace Rick.Modules
{
    public class MusicModule : ModuleBase
    {
        private IVoiceChannel VoiceChannel;
        private IAudioChannel AudioChannel;
        private IAudioClient AudioClient;

        [Command("Join")]
        public async Task JoinAsync()
        {
            await AudioChannel.ConnectAsync();
        }
    }
}
