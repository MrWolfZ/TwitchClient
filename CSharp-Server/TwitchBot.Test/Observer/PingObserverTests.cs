using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using TwitchBot.Irc.Messages;
using TwitchBot.Observer;

namespace TwitchBot.Test.Observer
{
    [TestClass]
    public class PingObserverTests
    {
        private const string Server = "testserver.com";
        private PingObserver observer;
        private IObserver<string> writer;

        [TestInitialize]
        public void SetUp()
        {
            this.writer = MockRepository.GenerateStub<IObserver<string>>();
            this.observer = new PingObserver(this.writer);
        }

        [TestMethod]
        public void TestOnNext()
        {
            this.observer.OnNext(new PingMessage(Server));
            this.writer.AssertWasCalled(w => w.OnNext("PONG :" + Server));
        }
    }
}