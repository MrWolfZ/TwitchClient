using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TwitchBot.Test.TestUtils
{
    [ExcludeFromCodeCoverage]
    public static class BlockingCollectionExtensions
    {
        public static T AssertTake<T>(this BlockingCollection<T> col, int milliseconds)
        {
            return col.AssertTake(TimeSpan.FromMilliseconds(milliseconds));
        }

        public static T AssertTake<T>(this BlockingCollection<T> col, TimeSpan timeout)
        {
            T res;
            if (!col.TryTake(out res, timeout))
            {
                Assert.Fail("Timed out!");
            }

            return res;
        }
    }
}