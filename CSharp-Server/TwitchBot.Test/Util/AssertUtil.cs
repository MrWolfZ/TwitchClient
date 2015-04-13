using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TwitchBot.Test.Util
{
    public static class AssertUtil
    {
        public static void AssertThrows<TException>(Action a) where TException : Exception
        {
            try
            {
                a();
                Assert.Fail("Expected exception of type {0} to be thrown!", typeof(TException).Name);
            }
            catch (TException)
            {
            }
        }
    }
}