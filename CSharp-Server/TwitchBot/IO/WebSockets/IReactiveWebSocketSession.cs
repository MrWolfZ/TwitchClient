using System;

namespace TwitchBot.IO.WebSockets
{
    public interface IReactiveWebSocketSession
    {
        IObservable<dynamic> Incoming { get; }
        IObserver<dynamic> Outgoing { get; }
    }
}