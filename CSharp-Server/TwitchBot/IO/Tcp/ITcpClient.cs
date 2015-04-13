using System;
using System.IO;
using System.Linq;

namespace TwitchBot.IO.Tcp
{
    public interface ITcpClient : IDisposable
    {
        TextReader Reader { get; }
        TextWriter Writer { get; }
    }
}