using System;
using System.Linq;

namespace TwitchBot.Irc.Messages
{
    public class PartMessage : AbstractChannelMessage
    {
        public PartMessage(DateTime timestamp, string user, string channelName)
            : base(timestamp, user, channelName)
        {
        }
    }
}