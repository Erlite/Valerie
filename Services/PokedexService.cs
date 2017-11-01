using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Cookie.Pokedex.Core;
using Cookie.Pokedex.Models;
using Valerie.Extensions;
using Valerie.Handlers.Config;
using Cookie.Pokedex;

namespace Valerie.Services
{
    public class PokedexService
    {
        DiscordSocketClient Client;
        IServiceProvider Provider;
        public TeamCore Team { get; } = new TeamCore();
        public TrainerCore Trainer { get; } = new TrainerCore();
        public PokemonCore Pokemon { get; } = new PokemonCore();
        Random Random = new Random(Guid.NewGuid().GetHashCode());
        Dictionary<ulong, (ulong, PokemonModel)> SpawnedPokemons { get; set; } = new Dictionary<ulong, (ulong, PokemonModel)>();


        public void Initialize(DiscordSocketClient _Client, IServiceProvider Provider)
        {
            new Pokedex(new PokedexConfig
            {
                APIUrl = "http://localhost:5000/api"
            });
            Client = _Client;
            this.Provider = Provider;
        }

        public async Task SpawnPokemonsAsync()
        {
            var Load = await Pokemon.PokemonListAsync();
            if (Load.StatusCode == 7002) return;
            foreach (var PokeGuild in BotConfig.Config.PokeServers.OrderBy(x => Random.Next()).Take(20))
            {
                var Guild = Client.GetGuild(PokeGuild);
                if (Guild == null) return;
                var RandomPoke = Load.Response.ToList()[Random.Next(0, Load.Response.Count)];
                var RandomChannel = Guild.TextChannels.ToList()[Random.Next(0, Guild.TextChannels.Count)];
                var Embed = ValerieEmbed.Embed(EmbedColor.Pastel, ImageUrl: RandomPoke.Avatar, Title: "A New Pokemon Has Appeared!");
                Embed.AddField("Pokemon", RandomPoke.Id, true);
                Embed.AddField("Poke Type", RandomPoke.PokeType, true);
                Embed.AddField("Pokemon Candy", RandomPoke.Candy, true);
                Embed.AddField("Pokemon HP", RandomPoke.MaxHP, true);
                var Msg = await RandomChannel.SendMessageAsync("", embed: Embed.Build());
                var TryAdding = SpawnedPokemons.TryAdd(RandomChannel.Id, (Msg.Id, RandomPoke));
                if (!TryAdding)
                    SpawnedPokemons.Add(RandomChannel.Id, (Msg.Id, RandomPoke));
            }
        }

        public async Task CatchPokemonAsync(IMessageChannel Channel, IUser User)
        {
            if (!SpawnedPokemons.ContainsKey(Channel.Id)) return;
            var GetChannel = SpawnedPokemons[Channel.Id];
            var Msg = await Channel.GetMessageAsync(GetChannel.Item1);
        }
    }
}