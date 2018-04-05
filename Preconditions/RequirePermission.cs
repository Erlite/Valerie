﻿using System;
using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Addons;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Valerie.Preconditions
{
    public class RequirePermission : PreconditionAttribute
    {
        AccessLevel AccessLevel { get; }
        public RequirePermission(AccessLevel accessLevel)
        {
            AccessLevel = accessLevel;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo Command, IServiceProvider Provider)
        {
            var Context = context as IContext;
            var GuildUser = Context.User as SocketGuildUser;
            var AdminPerms = Context.GuildHelper.GetProfile(Context.Guild.Id, Context.User.Id).IsAdmin || Context.Guild.OwnerId == Context.User.Id ||
                GuildUser.GuildPermissions.Administrator || GuildUser.GuildPermissions.ManageGuild;
            var ModPerms = new[] { GuildPermission.KickMembers, GuildPermission.BanMembers, GuildPermission.ManageChannels, GuildPermission.ManageMessages, GuildPermission.ManageRoles };
            if (AccessLevel >= AccessLevel.ADMIN && AdminPerms) return Task.FromResult(PreconditionResult.FromSuccess());
            else if (AccessLevel >= AccessLevel.MODERATOR && ModPerms.Any(x => GuildUser.GuildPermissions.Has(x))) return Task.FromResult(PreconditionResult.FromSuccess());
            else return Task.FromResult(PreconditionResult.FromError($"{Command.Name} requires **{AccessLevel}** AccessLevel. To learn more on AccessLevel, use `{Context.Config.Prefix}Info` command."));
        }
    }
}