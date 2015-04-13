using System;
using TwitchBot.Irc.Messages;

namespace TwitchBot.ChatServer
{
    public interface IServerChannelManager
    {
        IObservable<UserMessage> GetChannel(string channelName);
    }
}