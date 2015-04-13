using System;
using System.Linq;
using TwitchBot.Entity;

namespace TwitchBot.ChatServer
{
    public interface IServerUsersetUpdateManager
    {
        IObservable<UsersetUpdate> GetUsersetUpdates(string channelName);
    }
}