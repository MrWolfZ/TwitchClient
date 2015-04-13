using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using log4net;
using TwitchBot.Entity;
using TwitchBot.IO.WebSockets;
using TwitchBot.Irc;
using TwitchBot.Irc.Messages;
using TwitchBot.Util;

namespace TwitchBot.ChatServer
{
    public class RemoteChatSession : IDisposable
    {
        private readonly Guid sessionId;
        private readonly IReactiveWebSocketSession webSocketSession;
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        private readonly ILog logger;
        private readonly IIrcChannelWriter channelWriter;
        private readonly IServerChannelManager serverChannelManager;
        private readonly IServerUsersetUpdateManager serverUsersetUpdateManager;
        private ImmutableDictionary<string, IDisposable> channelsSubs = ImmutableDictionary<string, IDisposable>.Empty;

        public RemoteChatSession(
            Guid sessionId, 
            IReactiveWebSocketSession webSocketSession, 
            ILoggerFactory loggerFactory, 
            IIrcChannelWriter channelWriter, 
            IServerChannelManager serverChannelManager, 
            IServerUsersetUpdateManager serverUsersetUpdateManager, 
            IObservable<Exception> errors)
        {
            this.sessionId = sessionId;
            this.webSocketSession = webSocketSession;
            this.channelWriter = channelWriter;
            this.serverChannelManager = serverChannelManager;
            this.serverUsersetUpdateManager = serverUsersetUpdateManager;
            this.logger = loggerFactory.GetLogger<RemoteChatSession>();

            this.disposables.Add(this.webSocketSession.Incoming.Subscribe(this.ReceiveMessage, e => this.logger.Error(e.Message, e)));
            this.disposables.Add(errors.Subscribe(this.SendErrorToClient));
        }

        public void Dispose()
        {
            this.webSocketSession.Outgoing.OnCompleted();

            foreach (var disposable in this.channelsSubs.Values)
            {
                disposable.Dispose();
            }

            this.disposables.Dispose();
        }

        private void JoinChannelByName(string channelName)
        {
            this.channelWriter.Join(channelName);

            var usersetUpdates = this.serverUsersetUpdateManager.GetUsersetUpdates(channelName);

            var d = new CompositeDisposable
            {
                this.serverChannelManager.GetChannel(channelName).Subscribe(
                    this.SendMessageToClient, 
                    e => this.logger.Error(e.Message, e), 
                    () => this.logger.DebugFormat("Channel messages for channel #{0} terminated.", channelName)), 
                usersetUpdates.FirstAsync().Subscribe(m => this.SendInitialUsersetToClient(channelName, m), e => this.logger.Error(e.Message, e)), 
                usersetUpdates.Skip(1).Subscribe(m => this.SendUsersetUpdateToClient(channelName, m), e => this.logger.Error(e.Message, e))
            };

            this.channelsSubs = this.channelsSubs.Add(channelName, d);

            this.logger.DebugFormat("[Session {0}] Joined channel #{1}.", this.sessionId, channelName);
        }

        private void PartChannelByName(string channelName)
        {
            IDisposable d;
            if (this.channelsSubs.TryGetValue(channelName, out d))
            {
                this.channelsSubs = this.channelsSubs.Remove(channelName);
                d.Dispose();

                this.logger.DebugFormat("[Session {0}] Parted channel #{1}.", this.sessionId, channelName);
            }
        }

        private void ReceiveMessage(dynamic message)
        {
            var type = (string)message.Type;
            var content = (string)message.Message.Content;
            var channelName = (string)message.Message.ChannelName;
            switch (type)
            {
                case "join":
                    this.JoinChannelByName(channelName);
                    break;
                case "part":
                    this.PartChannelByName(channelName);
                    break;
                case "message":
                    this.SendMessageToServer(channelName, content);
                    break;
            }
        }

        private void SendMessageToClient(UserMessage message)
        {
            var msg = new { message.User, message.ChannelName, message.Content, message.Timestamp };
            this.webSocketSession.Outgoing.OnNext(new { Type = "message", Message = msg });
        }

        private void SendErrorToClient(Exception e)
        {
            var msg = new { Error = e.Message, Timestamp = DateTime.Now };
            this.webSocketSession.Outgoing.OnNext(new { Type = "error", Message = msg });
        }

        private void SendMessageToServer(string channelName, string content)
        {
            IDisposable d;
            if (this.channelsSubs.TryGetValue(channelName, out d))
            {
                this.channelWriter.SendMessage(channelName, content);
            }
            else
            {
                this.logger.WarnFormat("Client tried to send message to unjoined channel #{0}!", channelName);
            }
        }

        private void SendInitialUsersetToClient(string channelName, UsersetUpdate message)
        {
            var msg = new { ChannelName = channelName, Users = message.NewUserSet };
            this.webSocketSession.Outgoing.OnNext(new { Type = "initialUserset", Message = msg });
        }

        private void SendUsersetUpdateToClient(string channelName, UsersetUpdate message)
        {
            var msg = new { ChannelName = channelName, Total = message.NewUserSet.Count, Joins = message.Joins.ToArray(), Parts = message.Parts.ToArray() };
            this.webSocketSession.Outgoing.OnNext(new { Type = "usersetUpdate", Message = msg });
        }
    }
}