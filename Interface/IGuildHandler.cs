using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Meeseeks.Interfaces
{
    public interface IGuildHandler : IHandler
    {
        Task InitializeAsync();
    }
}
