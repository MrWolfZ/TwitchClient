using System;
using System.Linq;
using TwitchBot.Irc.Messages;

namespace TwitchBot.Irc
{
    public interface IObservableIrcClient
    {
        IObservable<JoinMessage> JoinMessages { get; }
        IObservable<PartMessage> PartMessages { get; }
        IObservable<PingMessage> PingMessages { get; }
        IObservable<string> UnfilteredMessages { get; }
        IObservable<UserlistEndMessage> UserlistEndMessages { get; }
        IObservable<UsersetMessage> UserlistMessages { get; }
        IObservable<UserMessage> UserMessages { get; }
    }
}