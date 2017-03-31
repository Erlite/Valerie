using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Handlers;

namespace DiscordBot.Attributes
{
    public class RequiredAttribute : ParameterPreconditionAttribute
    {
        public string Text { get; private set; }
        public RequiredAttribute(string text = null)
        {
            Text = text;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, ParameterInfo parameter, object value, IDependencyMap map)
        {
            if (!(context is CommandContext))
                return Task.FromResult(PreconditionResult.FromSuccess());
            if (value != null && !(value is object[]) || (value is object[] && (value as object[]).Count() != 0))
                return Task.FromResult(PreconditionResult.FromSuccess());

            CommandContext con = context as CommandContext;
            string cmdline;
            if (Text == null)
                cmdline = $"**Usage**: {con.MainHandler.GetCommandPrefix(con.Channel)}{parameter.Command.Aliases.First()} [{String.Join("] [", parameter.Command.Parameters.Select(x => x.Name))}]";
            else
                cmdline = $"**Usage**: {con.MainHandler.GetCommandPrefix(con.Channel)}{parameter.Command.Aliases.First()} [{Text}]";
            return Task.FromResult(PreconditionResult.FromError(cmdline));
        }
    }
}