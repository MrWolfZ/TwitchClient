using System;
using System.Linq;

namespace TwitchBot.Irc.Messages
{
    public class UserlistEndMessage : AbstractChannelMessage
    {
        public UserlistEndMessage(DateTime timestamp, string user, string channelName)
            : base(timestamp, user, channelName)
        {
        }
    }
}