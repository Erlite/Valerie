using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.IO;
using Nygma.Utils;
using Nygma.Handlers;

namespace Nygma.Modules
{
    public class TDLModule : ModuleBase
    {
        [Command("todo")]
        [Remarks("Views your todo list.")]
        [RequireContext(ContextType.Guild)]
        public async Task ViewTodo()
        {
            ChecksHandler.TodoCheck();
            var path = Path.Combine(AppContext.BaseDirectory, $"Extras/TDL.json");
            var filetext = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, $"Extras/TDL.json"));
            var json = JsonConvert.DeserializeObject<List<TodoList>>(filetext);
            try
            {
                if (!json.Any(x => x.Id == Context.User.Id))
                {
                    var newtodo = new TodoList()
                    {
                        Id = Context.User.Id,
                        ListItems = new List<string> { "" }
                    };
                    json.Add(newtodo);
                    var outjson = JsonConvert.SerializeObject(json);
                    File.WriteAllText(path, outjson);
                    await ReplyAsync(":anger: Your todo list is empty.");
                }
                else
                {
                    var ret = json.Find(x => x.Id == Context.User.Id);
                    var list = new StringBuilder();
                    foreach (var item in ret.ListItems)
                    {
                        list.AppendLine($"~ {item}");
                    }
                    await ReplyAsync($@"**>>>>  Your Todo List  <<<<**
```
{list.ToString()}
```");

                }

            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }

        }

        [Command("addtodo")]
        [Name("addtodo `<todoitem>`")]
        [Remarks("Add to your todo list.")]
        [RequireContext(ContextType.Guild)]
        public async Task AddTodo([Remainder] string listitem)
        {
            ChecksHandler.TodoCheck();
            var path = Path.Combine(AppContext.BaseDirectory, $"Extras/TDL.json");
            var filetext = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, $"Extras/TDL.json"));
            var json = JsonConvert.DeserializeObject<List<TodoList>>(filetext);
            try
            {
                if (!json.Any(x => x.Id == Context.User.Id))
                {
                    var newtodo = new TodoList()
                    {
                        Id = Context.User.Id,
                        ListItems = new List<string> { listitem }
                    };
                    json.Add(newtodo);
                    var outjson = JsonConvert.SerializeObject(json);
                    File.WriteAllText(path, outjson);
                    await ReplyAsync(":white_check_mark: Added item to your Todo List.");
                }
                else
                {
                    json.First(x => x.Id == Context.User.Id).ListItems.Add(listitem);
                    var outjson = JsonConvert.SerializeObject(json);
                    File.WriteAllText(path, outjson);
                    await ReplyAsync(":white_check_mark: Added item to your Todo List.");
                }
            }
            catch (Exception e)
            {
                await ReplyAsync($"Exception: {e.Message}");
            }
        }

        [Command("deltodo")]
        [Name("deltodo `<todoitem>`")]
        [Remarks("Deletes an item from your todo list by string.")]
        [RequireContext(ContextType.Guild)]
        public async Task DelTodo([Remainder] string listitem)
        {
            ChecksHandler.TodoCheck();
            var path = Path.Combine(AppContext.BaseDirectory, $"Extras/TDL.json");
            var filetext = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, $"Extras/TDL.json"));
            var json = JsonConvert.DeserializeObject<List<TodoList>>(filetext);
            try
            {
                if (!json.Any(x => x.Id == Context.User.Id))
                {
                    await ReplyAsync(":anger: You dont have a todo list...");
                }
                else
                {
                    if (json.First(x => x.Id == Context.User.Id).ListItems.Contains(listitem))
                    {
                        //Check if list item just contains one word
                        var index = json.FindIndex(x => x.ListItems.Contains(listitem));
                        json.First(x => x.Id == Context.User.Id).ListItems.RemoveAt(index);
                        var outjson = JsonConvert.SerializeObject(json);
                        File.WriteAllText(path, outjson);
                        await ReplyAsync(":white_check_mark: Removed item from your Todo List.");
                    }
                    else
                    {
                        await ReplyAsync(":anger: No list item was found with that keyword.");
                    }
                }
            }
            catch (Exception e)
            {
                await ReplyAsync($"Exception: {e.Message}");
            }
        }

        [Command("deltodo")]
        [Name("deltodo `<index>`")]
        [Remarks("Deletes an item from your todo list by index.")]
        [RequireContext(ContextType.Guild)]
        public async Task DelTodo(int index)
        {
            ChecksHandler.TodoCheck();
            var path = Path.Combine(AppContext.BaseDirectory, $"Extras/TDL.json");
            var filetext = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, $"Extras/TDL.json"));
            var json = JsonConvert.DeserializeObject<List<TodoList>>(filetext);
            try
            {
                index--;
                if (!json.Any(x => x.Id == Context.User.Id))
                {
                    await ReplyAsync(":anger: You dont have a todo list...");
                }
                else
                {
                    if (index < json.First(x => x.Id == Context.User.Id).ListItems.Count)
                    {
                        json.First(x => x.Id == Context.User.Id).ListItems.RemoveAt(index);
                        var outjson = JsonConvert.SerializeObject(json);
                        File.WriteAllText(path, outjson);
                        await ReplyAsync(":white_check_mark: Removed item from your Todo List.");
                    }
                    else
                    {
                        await ReplyAsync(":anger: There is no list item for that index.");
                    }
                }
            }
            catch (Exception e)
            {
                await ReplyAsync($"Exception: {e.Message}");
            }
        }
    }
}