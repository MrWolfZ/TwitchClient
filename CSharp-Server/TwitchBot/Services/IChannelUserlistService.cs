using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwitchBot.Entity;

namespace TwitchBot.Services
{
    public interface IChannelUserlistService
    {
        Task<IImmutableSet<User>> GetUsersAsync(string channelName, CancellationToken token);
    }
}