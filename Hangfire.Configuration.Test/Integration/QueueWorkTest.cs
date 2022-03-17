using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hangfire.Configuration.Test.Infrastructure;
using Hangfire.Server;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Integration;

public class QueueWorkTest : DatabaseTestBase
{
	public class FakeJobService
	{
		public static void Reset() => WasRun = false;
		public static bool WasRun = false;
		public static void RunTheJob() => WasRun = true;
	}

	public QueueWorkTest(string connectionString) : base(connectionString)
	{
	}
	
	[Test]
	public void ShouldRunEnqueuedJobOnWorker()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = ConnectionString,
			UpdateConfigurations = new[]
			{
				new UpdateStorageConfiguration
				{
					ConnectionString = ConnectionString,
					Name = DefaultConfigurationName.Name()
				}
			}
		});
		system.UseRealHangfire();
		FakeJobService.Reset();

		var storage = system.BuildPublishersQuerier()
			.QueryPublishers().Single().JobStorage;
		var client = new BackgroundJobClient(storage);
		client.Enqueue(() => FakeJobService.RunTheJob());
		emulateWorkerIteration(storage);

		FakeJobService.WasRun.Should().Be.True();
	}

	private static void emulateWorkerIteration(JobStorage storage)
	{
		// will hang if nothing to work with
		// if (NumberOfEnqueuedJobs() > 0)
		new Worker().Execute(
			new BackgroundProcessContext(
				"fake server",
				storage,
				new Dictionary<string, object>(),
				Guid.NewGuid(),
				new CancellationToken(),
				new CancellationToken(),
				new CancellationToken()
			));
	}

}