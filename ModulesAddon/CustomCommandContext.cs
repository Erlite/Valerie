﻿using Discord;
using Discord.Commands;
using Rick.Handlers;
using Discord.Addons.InteractiveCommands;

namespace Rick.ModulesAddon
{
    public class CustomCommandContext : ICommandContext
    {
        public IDiscordClient Client { get; }
        public IGuild Guild { get; }
        public MainHandler MainHandler { get; }
        public IMessageChannel Channel { get; }
        public IUser User { get; }
        public IUserMessage Message { get; }

        public bool IsPrivate => Channel is IPrivateChannel;

        public CustomCommandContext(IDiscordClient client, MainHandler handler, IUserMessage msg)
        {
            Client = client;
            Guild = (msg.Channel as IGuildChannel)?.Guild;
            Channel = msg.Channel;
            User = msg.Author;
            Message = msg;
            MainHandler = handler;
        }
    }
}
