using System;
using System.Linq;
using System.Reactive.Subjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwitchBot.Entity;
using TwitchBot.Irc;
using TwitchBot.Test.TestUtils;

namespace TwitchBot.Test.Irc
{
    [TestClass]
    public class ObservableIrcClientTests
    {
        private ObservableIrcClient client;
        private Subject<string> messages;

        [TestInitialize]
        public void SetUp()
        {
            this.messages = new Subject<string>();
            this.client = new ObservableIrcClient(this.messages);
        }

        [TestMethod]
        public void TestJoinMessage()
        {
            var cache = this.client.JoinMessages.Cache();
            this.messages.OnNext(":testuser!testuser@testuser.tmi.twitch.tv JOIN #testchannel");
            var msg = cache.AssertTake(10);
            Assert.AreEqual("testuser", msg.User);
            Assert.AreEqual("testchannel", msg.ChannelName);
        }

        [TestMethod]
        public void TestPartMessage()
        {
            var cache = this.client.PartMessages.Cache();
            this.messages.OnNext(":testuser!testuser@testuser.tmi.twitch.tv PART #testchannel");
            var msg = cache.AssertTake(10);
            Assert.AreEqual("testuser", msg.User);
            Assert.AreEqual("testchannel", msg.ChannelName);
        }

        [TestMethod]
        public void TestPing()
        {
            var cache = this.client.PingMessages.Cache();
            this.messages.OnNext("PING :tmi.twitch.tv");
            var ping = cache.AssertTake(10);
            Assert.AreEqual("tmi.twitch.tv", ping.Server);
        }

        [TestMethod]
        public void TestUnfilteredMessage()
        {
            var cache = this.client.UnfilteredMessages.Cache();
            this.messages.OnNext(":testuser!testuser@testuser.tmi.twitch.tv PRIVMSG #testchannel :test");
            var msg = cache.AssertTake(10);
            Assert.AreEqual(":testuser!testuser@testuser.tmi.twitch.tv PRIVMSG #testchannel :test", msg);
        }

        [TestMethod]
        public void TestUserlistEndMessage()
        {
            var cache = this.client.UserlistEndMessages.Cache();
            this.messages.OnNext(":testuser.tmi.twitch.tv 366 testuser #testchannel :End of /NAMES list");
            var msg = cache.AssertTake(10);
            Assert.AreEqual("testuser", msg.User);
            Assert.AreEqual("testchannel", msg.ChannelName);
        }

        [TestMethod]
        public void TestUserlistMessage()
        {
            var cache = this.client.UserlistMessages.Cache();
            this.messages.OnNext(":testuser.tmi.twitch.tv 353 testuser = #testchannel :testuser testuser2");
            var msg = cache.AssertTake(10);
            Assert.AreEqual("testuser", msg.User);
            Assert.AreEqual("testchannel", msg.ChannelName);
            Assert.IsTrue(new[] { new User("testuser"), new User("testuser2") }.SequenceEqual(msg.Users.OrderBy(s => s)));
        }

        [TestMethod]
        public void TestUserMessage()
        {
            var cache = this.client.UserMessages.Cache();
            this.messages.OnNext(":testuser!testuser@testuser.tmi.twitch.tv PRIVMSG #testchannel :test");
            var msg = cache.AssertTake(10);
            Assert.AreEqual("testuser", msg.User);
            Assert.AreEqual("testchannel", msg.ChannelName);
            Assert.AreEqual("test", msg.Content);
        }
    }
}