using System;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using log4net;
using TwitchBot.Util;
using vtortola.WebSockets;
using vtortola.WebSockets.Rfc6455;

namespace TwitchBot.IO.WebSockets
{
    public class ReactiveWebSocketServer : IDisposable
    {
        private readonly WebSocketListener server;
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        private readonly IConnectableObservable<IReactiveWebSocketSession> sessions;
        private readonly ILog logger;

        public ReactiveWebSocketServer(IPEndPoint endPoint, ILoggerFactory loggerFactory)
        {
            this.server = new WebSocketListener(endPoint);
            this.logger = loggerFactory.GetLogger<ReactiveWebSocketServer>();
            this.disposables.Add(this.server);

            var rfc6455 = new WebSocketFactoryRfc6455(this.server);
            this.server.Standards.RegisterStandard(rfc6455);

            var clientConnections = Observable.FromAsync(this.server.AcceptWebSocketAsync).DoWhile(() => !this.disposables.IsDisposed);

            var s = from c in clientConnections
                    where c != null
                    let incoming = Observable.FromAsync(c.ReadDynamicAsync).Where(msg => msg != null).DoWhile(() => c.IsConnected)
                    let outgoing =
                        System.Reactive.Observer.Synchronize(
                            System.Reactive.Observer.Create<dynamic>(
                                c.WriteDynamic, 
                                () =>
                                {
                                    this.logger.Debug("Outgoing messages terminated.");

                                    if (c.IsConnected)
                                    {
                                        c.Close();
                                        c.Dispose();
                                    }
                                }))
                    select new ReactiveWebSocketSession(incoming, outgoing);

            this.sessions = s.Publish();
            this.disposables.Add(this.sessions.Connect());
        }

        public IObservable<IReactiveWebSocketSession> Sessions
        {
            get
            {
                return this.sessions;
            }
        }

        public void Dispose()
        {
            this.disposables.Dispose();
        }

        public void Start()
        {
            this.server.Start();

            this.logger.Info("Started server. Waiting for client connections...");
        }
    }
}