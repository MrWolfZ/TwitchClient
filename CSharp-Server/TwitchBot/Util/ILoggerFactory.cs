using System;
using System.Linq;
using log4net;

namespace TwitchBot.Util
{
    public interface ILoggerFactory
    {
        ILog GetLogger<T>();
    }
}