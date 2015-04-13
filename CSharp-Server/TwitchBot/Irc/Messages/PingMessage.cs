using System;
using System.Linq;

namespace TwitchBot.Irc.Messages
{
    public class PingMessage
    {
        private readonly string server;

        public PingMessage(string server)
        {
            this.server = server;
        }

        public string Server
        {
            get { return this.server; }
        }
    }
}