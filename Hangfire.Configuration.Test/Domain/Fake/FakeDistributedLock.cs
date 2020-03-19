using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class FakeDistributedLock : IDistributedLock
    {
        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public IDisposable Take(string resource)
        {
            var @lock = _locks.GetOrAdd(resource, x => new object());
            Monitor.Enter(@lock);
            return new GenericDisposable(() =>
            {
                Monitor.Exit(@lock);
            });
        }
    }
}