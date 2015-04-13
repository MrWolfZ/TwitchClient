using System;
using System.Linq;
using log4net;

namespace TwitchBot.Util
{
    public class LoggerFactory : ILoggerFactory
    {
        public ILog GetLogger<T>()
        {
            return LogManager.GetLogger(typeof(T));
        }
    }
}