using System;
using System.Linq;

namespace TwitchBot.IO.Tcp
{
    public interface ITcpClientFactory
    {
        ITcpClient CreateClient(string serverAddress, int port);
    }
}