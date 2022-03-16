using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hangfire.Server;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Integration;

[Parallelizable(ParallelScope.None)]
public class DoStuffTest
{
	public string ConnectionString => ConnectionStrings.SqlServer;

	public static bool JobWasRun = false;
	public static void TheJob() => JobWasRun = true;

	[Test]
	public void DoTheFoo()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionString);
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
		var storage = system.BuildPublishersQuerier()
			.QueryPublishers().Single().JobStorage;
		var client = new BackgroundJobClient(storage);
		client.Enqueue(() => TheJob());
		JobWasRun.Should().Be.False();

		emulateWorkerIteration(storage);

		JobWasRun.Should().Be.True();
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