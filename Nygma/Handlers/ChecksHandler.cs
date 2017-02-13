using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Nygma.Utils;

namespace Nygma.Handlers
{
    public class ChecksHandler
    {
        [JsonIgnore]
        public static readonly string appdir = AppContext.BaseDirectory;

        public static void RepCheck()
        {
            var path = Path.Combine(AppContext.BaseDirectory, $"Extras/Reps.json");
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "Extras")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Extras"));

            if (!File.Exists(path))
            {
                List<Reputation> reps = new List<Reputation>();
                reps.Add(new Reputation()
                {
                    Id = 0,
                    Rep = 0
                });
                var json = JsonConvert.SerializeObject(reps);
                using (var file = new FileStream(path, FileMode.Create)) { }
                File.WriteAllText(path, json);
            }
            else
                return;
        }

        public static void TodoCheck()
        {
            var path = Path.Combine(AppContext.BaseDirectory, $"Extras/TDL.json");
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "Extras")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Extras"));

            if (!File.Exists(path))
            {
                List<TodoList> lists = new List<TodoList>();
                List<string> item = new List<string>();
                item.Add("Default");
                lists.Add(new TodoList()
                {
                    Id = 0,
                    ListItems = item
                });
                var json = JsonConvert.SerializeObject(lists);
                using (var file = new FileStream(path, FileMode.Create)) { }
                File.WriteAllText(path, json);
            }
            else
                return;
        }
    }
}