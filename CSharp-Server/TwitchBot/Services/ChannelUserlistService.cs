using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using log4net;
using TwitchBot.Entity;
using TwitchBot.Util;

namespace TwitchBot.Services
{
    public class ChannelUserlistService : IChannelUserlistService
    {
        private const string UrlPattern = "http://tmi.twitch.tv/group/user/{0}/chatters";

        private readonly TimeSpan timeout;
        private readonly ILog logger;

        public ChannelUserlistService(TimeSpan timeout, ILoggerFactory loggerFactory)
        {
            this.timeout = timeout;
            this.logger = loggerFactory.GetLogger<ChannelUserlistService>();
        }

        public async Task<IImmutableSet<User>> GetUsersAsync(string channelName, CancellationToken token)
        {
            var uri = new Uri(string.Format(UrlPattern, channelName));
            var t = new HttpClient().GetAsync(uri, token);
            await t.TimeoutAfter(this.timeout);
            var result = await t;

            var body = await result.Content.ReadAsStringAsync();

            try
            {
                var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(body), new XmlDictionaryReaderQuotas());
                var root = XElement.Load(jsonReader);
                var viewers = from n in root.Elements("chatters")
                              from u in n.Elements()
                              from v in u.Elements()
                              select v.Value;

                return ImmutableHashSet.Create(viewers.Select(n => new User(n)).ToArray());
            }
            catch (Exception)
            {
                this.logger.WarnFormat("Problem while parsing body: {0}!", body);
                throw;
            }
        }
    }
}