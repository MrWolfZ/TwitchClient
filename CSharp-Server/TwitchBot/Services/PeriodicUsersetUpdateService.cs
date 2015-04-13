using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using log4net;
using TwitchBot.Entity;
using TwitchBot.Util;

namespace TwitchBot.Services
{
    public class PeriodicUsersetUpdateService : IPeriodicUsersetUpdateService
    {
        private readonly IChannelUserlistService userlistService;
        private readonly ILog logger;

        public PeriodicUsersetUpdateService(IChannelUserlistService userlistService, ILoggerFactory loggerFactory)
        {
            this.userlistService = userlistService;
            this.logger = loggerFactory.GetLogger<PeriodicUsersetUpdateService>();
        }

        public IObservable<UsersetUpdate> Subscribe(string channelName, IImmutableSet<User> set, TimeSpan interval)
        {
            return Observable.Create<UsersetUpdate>(
                async (obs, token) =>
                {
                    this.logger.DebugFormat("Creating userlist observable for channel {0}!", channelName);
                    token.Register(() => this.logger.DebugFormat("Cancelling userlist observable for channel {0}!", channelName));

                    await Task.Yield();

                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            var sleep = Task.Delay(interval, token);
                            var newSet = await this.userlistService.GetUsersAsync(channelName, token);

                            if (!newSet.SetEquals(set))
                            {
                                var joins = newSet.Except(set);
                                var parts = set.Except(newSet);
                                var update = new UsersetUpdate(set, newSet, joins, parts);
                                set = newSet;
                                
                                this.logger.InfoFormat(
                                    "[#{0}] Updated user list (Total: {1}, Joins: {2}, Parts: {3})",
                                    channelName,
                                    update.NewUserSet.Count,
                                    update.Joins.Count,
                                    update.Parts.Count);

                                obs.OnNext(update);
                            }

                            await sleep;
                        }
                        catch (OperationCanceledException)
                        {
                            obs.OnCompleted();
                            return;
                        }
                        catch (Exception e)
                        {
                            obs.OnError(e);
                            return;
                        }
                    }

                    obs.OnCompleted();
                }).Publish().RefCount();
        }
    }
}