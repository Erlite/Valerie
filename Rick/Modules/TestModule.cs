using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Rick.Controllers;
using Rick.Handlers;
using Rick.Functions;
using Discord.WebSocket;
using System.Linq;
using Discord.Audio;
using Discord;

namespace Rick.Modules
{
    public class TestModule : ModuleBase
    {
        private readonly Audio _service;

        public TestModule(Audio audio)
        {
            _service = audio;
        }
        [Command("Test", RunMode = RunMode.Async)]
        public async Task TestAsync(string Discrim)
        {
            await _service.JoinAudioChannelAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
            
        }
    }
}
