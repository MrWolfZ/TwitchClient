using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwitchBot.Util;

namespace TwitchBot.Test.Util
{
    [TestClass]
    public class TimespanExtensionsTests
    {
        [TestMethod]
        public void TestPretty()
        {
            Assert.AreEqual("0s", TimeSpan.Zero.Pretty());
            Assert.AreEqual("7s", TimeSpan.FromSeconds(7).Pretty());
            Assert.AreEqual("1m 7s", TimeSpan.FromSeconds(67).Pretty());
            Assert.AreEqual("1h 1m 7s", TimeSpan.FromSeconds(3667).Pretty());
            Assert.AreEqual("1d 1h 1m 7s", (TimeSpan.FromDays(1) + TimeSpan.FromSeconds(3667)).Pretty());
        }
    }
}