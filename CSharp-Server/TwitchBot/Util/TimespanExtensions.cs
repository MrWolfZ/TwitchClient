using System;
using System.Linq;
using System.Text;

namespace TwitchBot.Util
{
    public static class TimespanExtensions
    {
        public static string Pretty(this TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
            {
                return "0s";
            }

            var stringBuilder = new StringBuilder();
            if (timeSpan.Days > 0)
            {
                stringBuilder.AppendFormat("{0}d ", timeSpan.Days);
            }

            if (timeSpan.Hours > 0)
            {
                stringBuilder.AppendFormat("{0}h ", timeSpan.Hours);
            }

            if (timeSpan.Minutes > 0)
            {
                stringBuilder.AppendFormat("{0}m ", timeSpan.Minutes);
            }

            if (timeSpan.Seconds > 0)
            {
                stringBuilder.AppendFormat("{0}s", timeSpan.Seconds);
            }

            return stringBuilder.ToString();
        }
    }
}