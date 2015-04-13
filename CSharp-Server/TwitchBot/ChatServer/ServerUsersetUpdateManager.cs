using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using TwitchBot.Entity;
using TwitchBot.Services;

namespace TwitchBot.ChatServer
{
    public class ServerUsersetUpdateManager : IServerUsersetUpdateManager
    {
        private readonly ConcurrentDictionary<string, IObservable<UsersetUpdate>> channels = new ConcurrentDictionary<string, IObservable<UsersetUpdate>>();

        private readonly IPeriodicUsersetUpdateService periodicUsersetUpdateService;
        private readonly TimeSpan updateInterval;

        public ServerUsersetUpdateManager(IPeriodicUsersetUpdateService periodicUsersetUpdateService, TimeSpan updateInterval)
        {
            this.periodicUsersetUpdateService = periodicUsersetUpdateService;
            this.updateInterval = updateInterval;
        }

        public IObservable<UsersetUpdate> GetUsersetUpdates(string channelName)
        {
            return this.channels.GetOrAdd(channelName, this.CreateChannelObservable);
        }

        private IObservable<UsersetUpdate> CreateChannelObservable(string channelName)
        {
            return this.periodicUsersetUpdateService.Subscribe(channelName, ImmutableHashSet<User>.Empty, this.updateInterval);
        }
    }
}