using System;
using System.Threading.Tasks;
using Discord.Commands;
using Cookie.Pokedex.Models;
using Valerie.Handlers;
using Valerie.Services;
using Valerie.Attributes;

namespace Valerie.Modules
{
    public class PokemonModule : ValerieBase<ValerieContext>
    {
        PokedexService Pokedex { get; } = new PokedexService();

        [Command("RegisterPokemon"), Alias("RP"), Summary("Registers a new Pokemon to Pokedex."), ServerLock]
        public async Task RegisterPokemonAsync(string Name, string Avatar, PokeType Poketype)
        {
            int HP = Context.Random.Next(20, 200);
            Name = Name.Substring(0, 1).ToUpper() + Name.Substring(1);

            var Check = await Pokedex.Pokemon.GetPokemonAsync(Name);
            if (Check.StatusCode == 7003) return;
            var Send = await Pokedex.Pokemon.AddPokemonAsync(new PokemonModel
            {
                Id = Name,
                Avatar = Avatar,
                PokeType = Poketype,
                Nickname = Name,
                MaxHP = HP,
                CurrentHP = HP,
                Candy = Context.Random.Next(1, 5)
            });
            if (!Send.IsSuccessStatusCode)
            {
                await ReplyAsync($"Something went wrong when regiseting: {Name}");
                return;
            }
            await ReplyAsync($"Registered {Name} to Pokedex.");
        }

        [Command("RegisterTrainer"), Alias("RT"), Summary("Registers a new Pokemon to Pokedex."), ServerLock]
        public async Task RegisterTrainerAsync()
        {
            var Check = await Pokedex.Trainer.GetTrainer($"{Context.User.Id}");
            if (Check.StatusCode == 8002) return;
            var Send = await Pokedex.Trainer.AddTrainer(new TrainerModel
            {
                Id = $"{Context.User.Id}",
                JoinDate = DateTime.Now,
                Username = Context.User.Username,
                Coins = DateTime.Now.Second
            });
            if (!Send.IsSuccessStatusCode)
            {
                await ReplyAsync("There was an error registering you as a trainer.");
                return;
            }
            await ReplyAsync("You have been registered as a trainer.");
        }
    }
}