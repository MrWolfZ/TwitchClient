using System;
using System.Linq;
using System.Reactive.Subjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwitchBot.Irc;
using TwitchBot.Test.TestUtils;

namespace TwitchBot.Test.Irc
{
    [TestClass]
    public class IrcChannelWriterTests
    {
        public const string ChannelName = "testChannel";

        private IIrcChannelWriter channelWriter;
        private Subject<string> writer;

        [TestInitialize]
        public void SetUp()
        {
            this.writer = new Subject<string>();
            this.channelWriter = new IrcChannelWriter(this.writer);
        }

        [TestMethod]
        public void TestJoinReturnsCorrectUserset()
        {
            var written = this.writer.Cache();
            this.channelWriter.Join(ChannelName);
            Assert.AreEqual("JOIN #" + ChannelName, written.AssertTake(10));
        }

        [TestMethod]
        public void TestPart()
        {
            var written = this.writer.Cache();
            this.channelWriter.Part(ChannelName);
            Assert.AreEqual("PART #" + ChannelName, written.AssertTake(10));
        }
    }
}