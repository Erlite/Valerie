using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Interfaces
{
    public interface IGuildHandler : IHandler
    {
        Task InitializeAsync();
    }
}
