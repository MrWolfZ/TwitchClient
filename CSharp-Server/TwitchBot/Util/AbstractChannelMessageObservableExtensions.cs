using System;
using System.Linq;
using System.Reactive.Linq;
using TwitchBot.Irc.Messages;

namespace TwitchBot.Util
{
    public static class AbstractChannelMessageObservableExtensions
    {
        public static IObservable<T> FilterByChannel<T>(this IObservable<T> source, string channelName) where T : AbstractChannelMessage
        {
            return source.Where(a => a.ChannelName == channelName);
        }

        public static IObservable<T> FilterByUser<T>(this IObservable<T> source, string user) where T : AbstractChannelMessage
        {
            return source.Where(a => a.User == user);
        }
    }
}