using System.Linq;
using Hangfire.Configuration.Test.Infrastructure;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Integration;

public class QueueWorkTest : DatabaseTest
{
	public QueueWorkTest(string connectionString) : base(connectionString)
	{
	}

	[Test]
	public void ShouldRunEnqueuedJobOnWorker_SqlServerStorage()
	{
		enqueuedJobOnWorker(ConnectionStrings.SqlServer);
	}
	
	[Test]
	[InstallRedis]
	public void ShouldRunEnqueuedJobOnWorker_RedisStorage()
	{
		enqueuedJobOnWorker("localhost");
	}
	
	private void enqueuedJobOnWorker(string storageConnstring)
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = ConnectionString,
			UpdateConfigurations = new[]
			{
				new UpdateStorageConfiguration
				{
					ConnectionString = storageConnstring,
					Name = DefaultConfigurationName.Name()
				}
			}
		});
		system.UseRealHangfire();
		FakeJobService.Reset();

		var publisher = system.QueryPublishers().Single();
		publisher.BackgroundJobClient.Enqueue(() => FakeJobService.RunTheJob());
		WorkerEmulation.SingleIteration(publisher.JobStorage);

		FakeJobService.WasRun.Should().Be.True();
	}

	public class FakeJobService
	{
		public static void Reset() => WasRun = false;
		public static bool WasRun = false;
		public static void RunTheJob() => WasRun = true;
	}
}