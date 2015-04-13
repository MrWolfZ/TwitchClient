using System;
using System.Linq;
using System.Reactive.Linq;

namespace TwitchBot.IO.WebSockets
{
    public class ReactiveWebSocketSession : IReactiveWebSocketSession
    {
        private readonly IObservable<dynamic> incoming;
        private readonly IObserver<dynamic> outgoing;

        public ReactiveWebSocketSession(IObservable<dynamic> incoming, IObserver<dynamic> outgoing)
        {
            var inc = incoming.Publish().RefCount();
            this.incoming = inc;
            this.outgoing = outgoing;
        }

        public IObservable<dynamic> Incoming
        {
            get
            {
                return this.incoming;
            }
        }

        public IObserver<dynamic> Outgoing
        {
            get
            {
                return this.outgoing;
            }
        }
    }
}