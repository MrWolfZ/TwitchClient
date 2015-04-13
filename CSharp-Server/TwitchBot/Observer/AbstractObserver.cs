using System;
using System.Linq;

namespace TwitchBot.Observer
{
    public abstract class AbstractObserver<T> : IObserver<T>
    {
        public virtual void OnNext(T value)
        {
        }

        public virtual void OnError(Exception error)
        {
        }

        public virtual void OnCompleted()
        {
        }
    }
}