using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Nygma.Handlers;
using Discord.Addons.Paginator;
using Nygma.Utils;
using System.IO;
using System;

namespace Nygma
{
    public class Program
    {
        public static void Main(string[] args) => new Core().RunAsync().GetAwaiter().GetResult();
    }
}