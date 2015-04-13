using System;
using System.Linq;

namespace TwitchBot.Irc.Messages
{
    public class UserMessage : AbstractChannelMessage
    {
        private readonly string content;

        public UserMessage(DateTime timestamp, string user, string channelName, string content) : base(timestamp, user, channelName)
        {
            this.content = content;
        }

        public string Content
        {
            get { return this.content; }
        }
    }
}