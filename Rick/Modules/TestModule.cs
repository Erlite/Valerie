//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using Discord.Commands;
//using Rick.Controllers;
//using Rick.Handlers;
//using Rick.Functions;
//using Discord.WebSocket;
//using System.Linq;
//using Discord.Audio;
//using Discord;
//using System.Threading;
//using System.Net.Http;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;

//namespace Rick.Modules
//{
//    private readonly Timer _timer;

//    public TestModule(DiscordSocketClient Client)
//    {
//        _timer = new Timer(async _ =>
//        {
//            var chn = Client.GetChannel(232558894554152960) as IMessageChannel;
//            await chn.SendMessageAsync("test");
//        },
//        null,
//        TimeSpan.FromSeconds(20),
//        TimeSpan.FromSeconds(5));
//    }
//    [Command("test")]
//    public async Task test()
//    {
//    }
//    public class TestModule : ModuleBase
//    {
//        [Command("test")]
//        public async Task Testasync([Remainder]string msg)
//        {

//        }
//    }
//}
