using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Test.Infrastructure;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Integration;

public class QueuePriorityTest : DatabaseTest
{
	private readonly string[] _queues = {"default", "a-queue-starting-with-a"};
	private IEnumerable<string> queuesAlphabetical => _queues.OrderBy(x => x).ToArray();

	public QueuePriorityTest(string connectionString) : base(connectionString)
	{
	}

	[Test]
	public void ShouldRunJobsInQueueAlphabeticalOrder_SqlServer()
	{
		runJobs(ConnectionStrings.SqlServer);
		FakeJobService.WasRunOn.Should().Have.SameSequenceAs(queuesAlphabetical);
	}

	[Test]
	public void ShouldRunJobsInQueueAlphabeticalOrder_Postgres()
	{
		runJobs(ConnectionStrings.Postgres);
		FakeJobService.WasRunOn.Should().Have.SameSequenceAs(queuesAlphabetical);
	}

	[Test]
	[InstallRedis]
	public void ShouldRunJobsInQueueInputOrder_Redis()
	{
		runJobs("localhost");
		FakeJobService.WasRunOn.Should().Have.SameSequenceAs(_queues);
	}

	private void runJobs(string storageConnectionString)
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions
			{
				ConnectionString = ConnectionString,
				UpdateConfigurations = new[]
				{
					new UpdateStorageConfiguration
					{
						ConnectionString = storageConnectionString,
						Name = DefaultConfigurationName.Name()
					}
				}
			})
			.UseServerOptions(new BackgroundJobServerOptions
			{
				Queues = _queues
			});
		system.UseRealHangfire();
		FakeJobService.Reset();

		var publisher = system.QueryPublishers().Single();
		publisher.BackgroundJobClient.Enqueue(() => FakeJobService.DefaultQueueJob());
		publisher.BackgroundJobClient.Enqueue(() => FakeJobService.AQueueJob());
		WorkerEmulation.SingleIteration(publisher.JobStorage, _queues);
		WorkerEmulation.SingleIteration(publisher.JobStorage, _queues);
	}

	public class FakeJobService
	{
		public static void Reset() => WasRunOn.Clear();
		public static IList<string> WasRunOn { get; } = new List<string>();

		public static void DefaultQueueJob() => WasRunOn.Add("default");

		[Queue("a-queue-starting-with-a")]
		public static void AQueueJob() => WasRunOn.Add("a-queue-starting-with-a");
	}
}