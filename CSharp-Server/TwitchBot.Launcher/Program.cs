using System;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using log4net;
using TwitchBot.ChatServer;
using TwitchBot.IO.Tcp;
using TwitchBot.IO.WebSockets;
using TwitchBot.Irc;
using TwitchBot.Observer;
using TwitchBot.Services;
using TwitchBot.Util;

namespace TwitchBot.Launcher
{
    public class Program
    {
        private const string ServerAddress = "irc.twitch.tv";
        private const int Port = 6667;

        private static readonly char[] BeepChars = { '\x7', '\a' };

        private readonly LoggerFactory loggerFactory;
        private readonly PeriodicUsersetUpdateService usersetUpdateService;
        private readonly ITcpClientFactory tcpClientFactory;
        private readonly ILog logger;
        private readonly ISubject<Exception> errors = new ReplaySubject<Exception>();
        private IServerChannelManager serverChannelManager;
        private IServerUsersetUpdateManager serverUsersetUpdateManager;

        public Program()
        {
            this.loggerFactory = new LoggerFactory();
            this.logger = this.loggerFactory.GetLogger<Program>();
            var channelUserlistService = new ChannelUserlistService(TimeSpan.FromSeconds(30), this.loggerFactory);
            this.usersetUpdateService = new PeriodicUsersetUpdateService(channelUserlistService, this.loggerFactory);
            this.tcpClientFactory = new TcpClientFactory(this.loggerFactory);
        }

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: twitchbot <usernick> <oauth-token>");
            }

            var nick = args[0];
            var pass = args[1];

            new Program().Launch(nick, pass);
        }

        private async void CreateChatSession(IReactiveWebSocketSession webSocketSession)
        {
            var sessionId = Guid.NewGuid();
            this.logger.InfoFormat("[Session {0}] Receiving web socket session...", sessionId);

            var tcpClient = this.tcpClientFactory.CreateClient(ServerAddress, Port);
            var writer = new Subject<string>();
            var nonEmptyWriter = writer.Where(s => !string.IsNullOrEmpty(s));
            var reactiveClient = new ReactiveTcpClient(tcpClient, nonEmptyWriter, this.loggerFactory, TimeSpan.FromMinutes(10));

            var debug = nonEmptyWriter.Subscribe(s => this.logger.DebugFormat("[Session {0}] > {1}", sessionId, s));

            var init = await webSocketSession.Incoming.FirstAsync();
            var username = (string)init.Username;
            var authToken = (string)init.AuthToken;
            var description = ((string)init.Description) ?? string.Empty;

            this.logger.InfoFormat(
                "[Session {0}] User authenticated with: Username={1}, authToken={2}, description={3}", 
                sessionId, 
                username, 
                authToken, 
                description);

            writer.OnNext(string.Format("PASS {0}", authToken));
            writer.OnNext(string.Format("NICK {0}", username));
            writer.OnNext(string.Format("USER {0} 0 * :{1}", username, description));

            var messages = reactiveClient.Reader.Where(s => !string.IsNullOrEmpty(s));
            var obsTwitchClient = new ObservableIrcClient(messages);
            var pongs = obsTwitchClient.PingMessages.Subscribe(new PingObserver(writer));

            var ircChannelWriter = new IrcChannelWriter(writer);
            var session = new RemoteChatSession(
                sessionId, 
                webSocketSession, 
                this.loggerFactory, 
                ircChannelWriter, 
                this.serverChannelManager, 
                this.serverUsersetUpdateManager, 
                this.errors);

            webSocketSession.Incoming.LastAsync().Amb(this.errors.FirstAsync()).Finally(
                () =>
                {
                    this.logger.InfoFormat("[Session {0}] Closing web socket session for user {1}...", sessionId, username);
                    pongs.Dispose();
                    session.Dispose();
                    debug.Dispose();
                }).Subscribe();
        }

        private void Launch(string username, string authToken)
        {
            var tcpClient = this.tcpClientFactory.CreateClient(ServerAddress, Port);
            var writer = new Subject<string>();
            var nonEmptyWriter = writer.Where(s => !string.IsNullOrEmpty(s));
            var reactiveClient = new ReactiveTcpClient(tcpClient, nonEmptyWriter, this.loggerFactory, TimeSpan.FromMinutes(60));
            var messages = reactiveClient.Reader.Where(s => !string.IsNullOrEmpty(s));
            var obsTwitchClient = new ObservableIrcClient(messages);

            writer.OnNext(string.Format("PASS {0}", authToken));
            writer.OnNext(string.Format("NICK {0}", username));
            writer.OnNext(string.Format("USER {0} 0 * :{1}", username, string.Empty));

            var ircChannelManager = new IrcChannelWriter(writer);
            this.serverChannelManager = new ServerChannelManager(obsTwitchClient, ircChannelManager);
            this.serverUsersetUpdateManager = new ServerUsersetUpdateManager(this.usersetUpdateService, TimeSpan.FromSeconds(10));

            var serverEndpoint = new IPEndPoint(IPAddress.Any, 8006);
            using (obsTwitchClient.UnfilteredMessages.Subscribe(this.PrintMessage, this.HandleError, this.OnCompleted))
            using (var chatServer = new ReactiveWebSocketServer(serverEndpoint, this.loggerFactory))
            using (chatServer.Sessions.Subscribe(this.CreateChatSession))
            {
                chatServer.Start();

                Console.WriteLine("Press any key to shut down the server...");
                Console.ReadKey();
            }
        }

        private void PrintMessage(string content)
        {
            content = BeepChars.Aggregate(content, (s, c) => s.Replace(c, ' '));
            this.logger.DebugFormat("< {0}", content);
        }

        private void HandleError(Exception e)
        {
            this.logger.Error(e.Message, e);
            this.errors.OnNext(e);
        }

        private void OnCompleted()
        {
            this.logger.Error("Server session ended!");
        }
    }
}