using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TwitchBot.Util;

namespace TwitchBot.IO.Tcp
{
    [ExcludeFromCodeCoverage]
    public class TcpClientFactory : ITcpClientFactory
    {
        private readonly ILoggerFactory loggerFactory;

        public TcpClientFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public ITcpClient CreateClient(string serverAddress, int port)
        {
            return new TcpClientAdapter(serverAddress, port, this.loggerFactory);
        }
    }
}