using System;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace TwitchBot.Util
{
    public static class StringObservableExtensions
    {
        public static IObservable<string[]> FilterAndExtract(this IObservable<string> source, Regex filter)
        {
            return from s in source
                   let match = filter.Match(s)
                   where match.Success
                   let values = match.Groups.Cast<Group>().Skip(1).Select(g => g.Value).ToArray()
                   select values;
        }
    }
}