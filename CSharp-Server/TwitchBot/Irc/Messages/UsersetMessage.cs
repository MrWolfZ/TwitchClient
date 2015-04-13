using System;
using System.Collections.Immutable;
using System.Linq;
using TwitchBot.Entity;

namespace TwitchBot.Irc.Messages
{
    public class UsersetMessage : AbstractChannelMessage
    {
        private readonly IImmutableSet<User> users;

        public UsersetMessage(DateTime timestamp, string user, string channelName, IImmutableSet<User> users)
            : base(timestamp, user, channelName)
        {
            this.users = users;
        }

        public IImmutableSet<User> Users
        {
            get { return this.users; }
        }
    }
}