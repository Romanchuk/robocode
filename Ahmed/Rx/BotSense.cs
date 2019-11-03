using System;
using System.Collections.Generic;
using Robocode;

namespace Romanchuk
{
    class BotSense : IObservable<Event>
    {
        private readonly IList<IObserver<Event>> _observers = new List<IObserver<Event>>(); 

        public IDisposable Subscribe(IObserver<Event> observer)
        {
            _observers.Add(observer);
            return new UnsubscribeOnDispose(() => _observers.Remove(observer));
        }

        public void PublishEvent(Event evnt)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(evnt);
            }
        }

        class UnsubscribeOnDispose : IDisposable
        {
            private readonly Action _unsubscribe;

            public UnsubscribeOnDispose(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                _unsubscribe();
            }
        }
    }
}