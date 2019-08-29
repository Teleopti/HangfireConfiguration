using System;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class FakeDistributedLock : IDistributedLock, IDisposable
    {
        public IDisposable Take(TimeSpan lockTimeout)
        {
            return this;
        }

        public void Dispose()
        {
        }
    }
}