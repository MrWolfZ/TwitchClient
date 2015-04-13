using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using TwitchBot.Irc;
using TwitchBot.Irc.Messages;

namespace TwitchBot.ChatServer
{
    public class ServerChannelManager : IServerChannelManager
    {
        private readonly ConcurrentDictionary<string, IObservable<UserMessage>> channels = new ConcurrentDictionary<string, IObservable<UserMessage>>();
        private readonly IObservableIrcClient observableIrcClient;
        private readonly IIrcChannelWriter channelWriter;

        public ServerChannelManager(IObservableIrcClient observableIrcClient, IIrcChannelWriter channelWriter)
        {
            this.observableIrcClient = observableIrcClient;
            this.channelWriter = channelWriter;
        }

        public IObservable<UserMessage> GetChannel(string channelName)
        {
            return this.channels.GetOrAdd(channelName, this.CreateChannelObservable);
        }

        private IObservable<UserMessage> CreateChannelObservable(string channelName)
        {
            return Observable.Create<UserMessage>(obs => this.ReadMessages(channelName, obs)).Publish().RefCount();
        }

        private IDisposable ReadMessages(string channelName, IObserver<UserMessage> obs)
        {
            this.channelWriter.Join(channelName);
            return new CompositeDisposable
            {
                this.observableIrcClient.UserMessages.Where(m => m.ChannelName == channelName).Subscribe(obs), 
                Disposable.Create(() => this.channelWriter.Part(channelName))
            };
        }
    }
}