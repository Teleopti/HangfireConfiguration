using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Server;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeBackgroundProcessingServer : IBackgroundProcessingServer
{
	public bool StopSent { get; private set; }
	public bool Disposed { get; private set; }
	public void SendStop() => StopSent = true;
	public void Dispose() => Disposed = true;
	public bool WaitForShutdown(TimeSpan timeout) => true;
	public Task WaitForShutdownAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
