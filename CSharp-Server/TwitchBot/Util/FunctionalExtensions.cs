using System;
using System.Linq;

namespace TwitchBot.Util
{
    public static class FunctionalExtensions
    {
        public static Func<TX, Action<TY>> Curry<TX, TY>(this Action<TX, TY> a)
        {
            return x => y => a(x, y);
        }

        public static Func<TX, Func<TY, TZ>> Curry<TX, TY, TZ>(this Func<TX, TY, TZ> f)
        {
            return x => y => f(x, y);
        }

        public static Func<TY, Action<TX>> Flip<TX, TY>(this Func<TX, Action<TY>> a)
        {
            return y => x => a(x)(y);
        }

        public static Func<TY, Func<TX, TZ>> Flip<TX, TY, TZ>(this Func<TX, Func<TY, TZ>> f)
        {
            return y => x => f(x)(y);
        }
    }
}