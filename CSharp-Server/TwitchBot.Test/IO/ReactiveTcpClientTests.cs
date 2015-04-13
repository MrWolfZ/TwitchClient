using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using TwitchBot.IO.Tcp;
using TwitchBot.Test.TestUtils;
using TwitchBot.Util;

namespace TwitchBot.Test.IO
{
    [TestClass]
    public class ReactiveTcpClientTests
    {
        private ReactiveTcpClient client;
        private ITcpClient wrapped;
        private StringWriter writer;
        private Subject<string> writerSubject;

        [TestInitialize]
        public void SetUp()
        {
            this.writerSubject = new Subject<string>();
            this.writer = new StringWriter();
            this.wrapped = MockRepository.GenerateStub<ITcpClient>();
            this.wrapped.Stub(c => c.Writer).Return(this.writer);
            this.client = new ReactiveTcpClient(this.wrapped, this.writerSubject, new LoggerFactory(), TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        public void Test()
        {
            var l = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "Int", 2 }, 
                    {
                        "Nested", 
                        new Dictionary<string, object>
                        {
                            { "String", null }
                        }
                    }
                }
            };

            var s = new DataContractJsonSerializer(l.GetType(), new[] { typeof(Dictionary<string, object>) });
            using (var stream = new MemoryStream())
            {
                s.WriteObject(stream, l);
                Console.WriteLine(stream.Length);
            }
        }

        [TestMethod]
        public void TestDispose()
        {
            this.client.Dispose();
            this.wrapped.AssertWasCalled(w => w.Dispose());
        }

        [TestMethod]
        public void TestReadMessage()
        {
            this.wrapped.Stub(c => c.Reader).Return(new StringReader("test" + this.writer.NewLine));
            this.client = new ReactiveTcpClient(this.wrapped, this.writerSubject, new LoggerFactory(), TimeSpan.FromSeconds(10));
            var cache = this.client.Reader.Cache();
            Assert.AreEqual("test", cache.AssertTake(20));
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void TestReadMessageThrowIOException()
        {
            this.wrapped.Stub(c => c.Reader).Throw(new IOException());
            this.client = new ReactiveTcpClient(this.wrapped, this.writerSubject, new LoggerFactory(), TimeSpan.FromSeconds(10));
            this.client.Reader.Wait();
        }

        [TestMethod]
        public void TestWriteMessage()
        {
            this.writerSubject.OnNext("test");
            Assert.AreEqual("test" + this.writer.NewLine, this.writer.GetStringBuilder().ToString());
        }
    }
}