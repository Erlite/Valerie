using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Nygma.Utils;
using Newtonsoft.Json;
using System.IO;
using Nygma.Handlers;

namespace Nygma.Modules
{
    [Group("Rep")]
    public class RepModule : ModuleBase
    {
        //UNDONE: Leaderboard
        [Command("Leaders")]
        [Remarks("Shows the leading users in reputation.")]
        [RequireContext(ContextType.Guild)]
        public async Task Leaderboard()
        {
            await ReplyAsync(":unamused: Leaderboard not implemented yet. *sorry*");
        }

        [Command("Mine")]
        [Remarks("Views your reputation.")]
        [RequireContext(ContextType.Guild)]
        public async Task MyRep()
        {
            ChecksHandler.RepCheck();
            var path = Path.Combine(AppContext.BaseDirectory, $"Extras/Reps.json");
            var filetext = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, $"Extras/Reps.json"));
            var json = JsonConvert.DeserializeObject<List<Reputation>>(filetext);
            try
            {
                if (!json.Any(x => x.Id == Context.User.Id))
                {
                    var defrep = new Reputation() { Id = Context.User.Id, Rep = 0 };
                    json.Add(defrep);
                    var outjson = JsonConvert.SerializeObject(json);
                    File.WriteAllText(path, outjson);
                }
                var rep = json.First(x => x.Id == Context.User.Id).Rep;
                if (rep < 0)
                    await ReplyAsync($":red_circle: **{Context.User.Username}'s** reputation: {rep}");
                else
                    await ReplyAsync($":white_circle: **{Context.User.Username}'s** reputation: {rep}");
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
        }

        [Command("")]
        [Name("rep `<@user>`")]
        [Remarks("Views a users reputation.")]
        [RequireContext(ContextType.Guild)]
        public async Task CheckRep(IGuildUser user)
        {
            ChecksHandler.RepCheck();
            var path = Path.Combine(AppContext.BaseDirectory, $"Extras/Reps.json");
            var filetext = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, $"Extras/Reps.json"));
            var json = JsonConvert.DeserializeObject<List<Reputation>>(filetext);
            try
            {
                if (!json.Any(x => x.Id == user.Id))
                {
                    var defrep = new Reputation() { Id = user.Id, Rep = 0 };
                    json.Add(defrep);
                    var outjson = JsonConvert.SerializeObject(json);
                    File.WriteAllText(path, outjson);
                }
                var rep = json.First(x => x.Id == user.Id).Rep;
                if (rep < 0)
                    await ReplyAsync($":red_circle: **{user.Username}'s** reputation: {rep}");
                else
                    await ReplyAsync($":white_circle: **{user.Username}'s** reputation: {rep}");
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
        }

        [Command("Add")]
        [Name("addrep `<@user>`")]
        [Remarks("Add reputation to a user.")]
        [RequireContext(ContextType.Guild)]
        public async Task AddRep(IGuildUser user)
        {
            ChecksHandler.RepCheck();
            var path = Path.Combine(AppContext.BaseDirectory, $"Extras/Reps.json");
            var filetext = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, $"Extras/Reps.json"));
            var json = JsonConvert.DeserializeObject<List<Reputation>>(filetext);
            try
            {
                if (!json.Any(x => x.Id == user.Id))
                {
                    var defrep = new Reputation() { Id = user.Id, Rep = 1 };
                    json.Add(defrep);
                    await ReplyAsync($":white_circle: **{user.Username}'s** reputation: 1");
                    var defout = JsonConvert.SerializeObject(json);
                    File.WriteAllText(path, defout);
                }
                else
                {
                    json.First(x => x.Id == user.Id).Rep++;
                    var rep = json.First(x => x.Id == user.Id).Rep;
                    if (rep < 0)
                        await ReplyAsync($":red_circle: **{user.Username}'s** reputation: {rep}");
                    else
                        await ReplyAsync($":white_circle: **{user.Username}'s** reputation: {rep}");
                    var outjson = JsonConvert.SerializeObject(json);
                    File.WriteAllText(path, outjson);
                }
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
        }

        [Command("Del")]
        [Name("delrep `<@user>`")]
        [Remarks("Deletes reputation from a user.")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        public async Task DelRep(IGuildUser user)
        {
            ChecksHandler.RepCheck();
            var path = Path.Combine(AppContext.BaseDirectory, $"Extras/Reps.json");
            var filetext = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, $"Extras/Reps.json"));
            var json = JsonConvert.DeserializeObject<List<Reputation>>(filetext);
            try
            {
                if (!json.Any(x => x.Id == user.Id))
                {
                    var defrep = new Reputation() { Id = user.Id, Rep = -1 };
                    json.Add(defrep);
                    await ReplyAsync($":red_circle: **{user.Username}'s** reputation: -1");
                    var defout = JsonConvert.SerializeObject(json);
                    File.WriteAllText(path, defout);
                }
                else
                {
                    json.First(x => x.Id == user.Id).Rep--;
                    var rep = json.First(x => x.Id == user.Id).Rep;
                    if (rep < 0)
                        await ReplyAsync($":red_circle: **{user.Username}'s** reputation: {rep}");
                    else
                        await ReplyAsync($":white_circle: **{user.Username}'s** reputation: {rep}");
                    var outjson = JsonConvert.SerializeObject(json);
                    File.WriteAllText(path, outjson);
                }
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
        }
    }
}