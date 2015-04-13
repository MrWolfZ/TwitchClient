using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchBot.Util
{
    public static class TaskExtensions
    {
        public static async Task TimeoutAfter(this Task task, TimeSpan timeout, CancellationToken? token = null)
        {
            if (timeout == TimeSpan.Zero)
            {
                throw new TimeoutException();
            }

            if (task.IsCompleted)
            {
                return;
            }

            var cts = new CancellationTokenSource();
            var t = token ?? cts.Token;
            if (await Task.WhenAny(Task.Delay(timeout, t), task) != task)
            {
                t.ThrowIfCancellationRequested();
                throw new TimeoutException();
            }

            cts.Cancel();
            await task;
        }
    }
}