using System;
using System.Collections.Concurrent;
using System.Linq;

namespace TwitchBot.Test.TestUtils
{
    public static class TestObservableExtensions
    {
        public static BlockingCollection<T> Cache<T>(this IObservable<T> source)
        {
            var col = new BlockingCollection<T>();
            source.Subscribe(col.Add);
            return col;
        }
    }
}