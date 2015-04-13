using System;
using System.Linq;

namespace TwitchBot.Irc
{
    public interface IIrcChannelWriter
    {
        void Join(string channelName);
        void Part(string channelName);
        void SendMessage(string channelName, string message);
    }
}