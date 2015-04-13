using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using TwitchBot.Util;

namespace TwitchBot.IO.Tcp
{
    public class ReactiveTcpClient : IDisposable
    {
        private readonly CompositeDisposable disposables;
        private readonly ILog logger;
        private readonly IObservable<string> reader;
        private readonly ITcpClient wrapped;
        private readonly IObservable<string> written;
        private readonly TimeSpan timeout;

        public ReactiveTcpClient(ITcpClient wrapped, IObservable<string> written, ILoggerFactory loggerFactory, TimeSpan timeout)
        {
            this.wrapped = wrapped;
            this.logger = loggerFactory.GetLogger<ReactiveTcpClient>();
            this.disposables = new CompositeDisposable(wrapped);
            this.written = written;
            this.timeout = timeout;
            this.reader = this.CreateReadHandler();
            this.disposables.Add(this.written.Synchronize().Subscribe(msg => wrapped.Writer.WriteLine(msg)));
        }

        public IObservable<string> Reader
        {
            get
            {
                return this.reader;
            }
        }

        public IObservable<string> Written
        {
            get
            {
                return this.written;
            }
        }

        public void Dispose()
        {
            this.disposables.Dispose();
        }

        private IObservable<string> CreateReadHandler()
        {
            return Observable.Create<string>((obs, token) => this.CreateObservingTask(obs, token)).Publish().RefCount();
        }

        private async Task<Action> CreateObservingTask(IObserver<string> obs, CancellationToken token)
        {
            this.logger.Debug("Starting task to read from network stream...");
            await this.LoopRead(obs, token);

            return () => this.logger.Debug("Cancelling task reading from network stream...");
        }

        private async Task LoopRead(IObserver<string> obs, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var t = this.wrapped.Reader.ReadLineAsync();
                    await t.TimeoutAfter(this.timeout, token);

                    var line = await t;

                    if (token.IsCancellationRequested)
                    {
                        obs.OnCompleted();
                        return;
                    }

                    obs.OnNext(line);
                }
                catch (TimeoutException)
                {
                    obs.OnError(new TimeoutException(string.Format("Did not receive message within {0}.", this.timeout)));
                    return;
                }
                catch (ObjectDisposedException)
                {
                    obs.OnCompleted();
                    return;
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
        }
    }
}