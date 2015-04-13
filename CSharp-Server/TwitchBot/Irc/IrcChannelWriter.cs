using System;
using System.Linq;

namespace TwitchBot.Irc
{
    public class IrcChannelWriter : IIrcChannelWriter
    {
        private readonly IObserver<string> writer;

        public IrcChannelWriter(IObserver<string> writer)
        {
            this.writer = writer;
        }

        public void Join(string channelName)
        {
            this.writer.OnNext(string.Format("JOIN #{0}", channelName));
        }

        public void Part(string channelName)
        {
            this.writer.OnNext(string.Format("PART #{0}", channelName));
        }

        public void SendMessage(string channelName, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            this.writer.OnNext(string.Format("PRIVMSG #{0} :{1}", channelName, message));
        }
    }
}