using System;
using System.Linq;

namespace TwitchBot.Irc.Messages
{
    public class JoinMessage : AbstractChannelMessage
    {
        public JoinMessage(DateTime timestamp, string user, string channelName) : base(timestamp, user, channelName)
        {
        }
    }
}