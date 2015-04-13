using System;
using System.Collections.Immutable;
using System.Linq;
using TwitchBot.Entity;

namespace TwitchBot.Services
{
    public interface IPeriodicUsersetUpdateService
    {
        IObservable<UsersetUpdate> Subscribe(string channelName, IImmutableSet<User> initialSet, TimeSpan interval);
    }
}