using Discord.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace Rick.Attributes
{
    public class CooldownAttribute : PreconditionAttribute
    {
        private Timer timer;
        private bool Cooldown;
        private int _cooldown;
        public CooldownAttribute(int cooldown = 60)
        {
            Cooldown = false;
            _cooldown = cooldown;
            timer = new Timer(Timer_Reset, null, Timeout.Infinite, Timeout.Infinite);
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            if (Cooldown)
                return Task.FromResult(PreconditionResult.FromError("This command is under Cooldown. Please wait few seconds before using this command again!"));
            Cooldown = true;
            timer.Change(_cooldown * 1000, Timeout.Infinite);
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        private void Timer_Reset(object state)
        {
            Cooldown = false;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}