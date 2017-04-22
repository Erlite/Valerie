﻿using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using Rick.Models;

namespace Rick.Attributes
{
    public class RequiredRoleAttribute : PreconditionAttribute
    {
        private GuildModel Model;

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var user = context.User as IGuildUser;
            var role = user.RoleIds.Intersect(GuildModel.GuildConfigs[context.Guild.Id].RequiredRoleID).Any();
            return await Task.FromResult(role) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"{Format.Bold("ERROR: ")}Role is missing! Please get the appropriate role for this command!");
        }
    }
}