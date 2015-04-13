using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwitchBot.Observer;

namespace TwitchBot.Test.Observer
{
    [TestClass]
    public class AbstractObserverTests
    {
        [TestMethod]
        public void TestMethodsDontDoAnything()
        {
            var impl = new ObserverImpl();
            impl.OnNext(null);
            impl.OnError(new Exception());
            impl.OnCompleted();
        }

        private sealed class ObserverImpl : AbstractObserver<object>
        {
        }
    }
}