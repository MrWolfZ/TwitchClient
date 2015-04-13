using System;
using System.Linq;
using TwitchBot.Irc.Messages;

namespace TwitchBot.Observer
{
    public class PingObserver : AbstractObserver<PingMessage>
    {
        private readonly IObserver<string> writer;

        public PingObserver(IObserver<string> writer)
        {
            this.writer = writer;
        }

        public override void OnNext(PingMessage value)
        {
            this.writer.OnNext(string.Format("PONG :{0}", value.Server));
        }
    }
}