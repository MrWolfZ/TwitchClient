using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using TwitchBot.Entity;
using TwitchBot.Irc.Messages;
using TwitchBot.Util;

namespace TwitchBot.Irc
{
    public class ObservableIrcClient : IObservableIrcClient
    {
        private static readonly Regex PingFilter = new Regex(@"^PING :(.*)$");
        private static readonly Regex UserMessageFilter = new Regex(@"^:([^!]*)!.*PRIVMSG #([\S]*) :(.*)$");
        private static readonly Regex JoinFilter = new Regex(@"^:([^!]*)!.*JOIN #(.*)$");
        private static readonly Regex PartFilter = new Regex(@"^:([^!]*)!.*PART #(.*)$");
        private static readonly Regex UserlistFilter = new Regex(@"^:\S* \d+ ([\S]*) = #([\S]*) :(.*)$");
        private static readonly Regex UserlistEndFilter = new Regex(@"^:\S* \d+ ([\S]*) #([\S]*) :End of /NAMES list$");
        private readonly IObservable<string> messages;

        public ObservableIrcClient(IObservable<string> messages)
        {
            this.messages = messages;
        }

        public IObservable<JoinMessage> JoinMessages
        {
            get
            {
                return from values in this.messages.FilterAndExtract(JoinFilter)
                       select new JoinMessage(DateTime.Now, user: values[0], channelName: values[1]);
            }
        }

        public IObservable<PartMessage> PartMessages
        {
            get
            {
                return from values in this.messages.FilterAndExtract(PartFilter)
                       select new PartMessage(DateTime.Now, user: values[0], channelName: values[1]);
            }
        }

        public IObservable<PingMessage> PingMessages
        {
            get
            {
                return from values in this.messages.FilterAndExtract(PingFilter)
                       select new PingMessage(server: values[0]);
            }
        }

        public IObservable<string> UnfilteredMessages
        {
            get { return this.messages; }
        }

        public IObservable<UserlistEndMessage> UserlistEndMessages
        {
            get
            {
                return from values in this.messages.FilterAndExtract(UserlistEndFilter)
                       select new UserlistEndMessage(DateTime.Now, user: values[0], channelName: values[1]);
            }
        }

        public IObservable<UsersetMessage> UserlistMessages
        {
            get
            {
                return from values in this.messages.FilterAndExtract(UserlistFilter)
                       let userlistStr = values[2]
                       let users = ImmutableHashSet.Create(userlistStr.Split(' ').Select(n => new User(n)).ToArray())
                       select new UsersetMessage(DateTime.Now, user: values[0], channelName: values[1], users: users);
            }
        }

        public IObservable<UserMessage> UserMessages
        {
            get
            {
                return from values in this.messages.FilterAndExtract(UserMessageFilter)
                       select new UserMessage(DateTime.Now, user: values[0], channelName: values[1], content: values[2]);
            }
        }
    }
}