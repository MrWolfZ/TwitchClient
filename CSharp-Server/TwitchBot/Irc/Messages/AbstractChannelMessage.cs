using System;
using System.Linq;

namespace TwitchBot.Irc.Messages
{
    public abstract class AbstractChannelMessage
    {
        private readonly string channelName;
        private readonly DateTime timestamp;
        private readonly string user;

        protected AbstractChannelMessage(DateTime timestamp, string user, string channelName)
        {
            this.user = user;
            this.channelName = channelName;
            this.timestamp = timestamp;
        }

        public string ChannelName
        {
            get { return this.channelName; }
        }

        public DateTime Timestamp
        {
            get { return this.timestamp; }
        }

        public string User
        {
            get { return this.user; }
        }
    }
}