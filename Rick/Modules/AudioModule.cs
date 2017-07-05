using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Rick.Controllers;
using Discord;

namespace Rick.Modules
{
    public class AudioModule : ModuleBase
    {
        private Audio Audio;
        public AudioModule( Audio Ad)
        {
            Audio = Ad;
        }

        [Command("Join", RunMode = RunMode.Async), Summary("Joins voice channel")]
        public async Task JoinAsync()
        {
            await ReplyAsync("Joining Audio Channel ...");
            await Audio.JoinAudioChannelAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("Leave", RunMode = RunMode.Async), Summary("leaves voice channel")]
        public async Task LeaveAsync()
        {
            await Audio.LeaveAudioChannelAsync(Context.Guild);
            await ReplyAsync("Leaving Audio Channel");
        }

        [Command("Play", RunMode = RunMode.Async), Summary("Plays musik!"), Remarks("Play Song Name")]
        public async Task PlayAsync([Remainder]string SongName)
        {
            await Audio.SendAudioAsync(Context.Guild, Context.Channel, SongName);
        }

    }
}
