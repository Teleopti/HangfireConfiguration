using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

[Ignore("WIP")]
public class ServerCountSampleRecorderContainerQueuesTest
{
	[Test]
	public void ShouldRecordSampleWithQueues()
	{
		var system = new SystemUnderTest();
		system
			.WithConfiguration(new StoredConfiguration())
			.WithAnnouncedServer("server1", queues: ["default"]);

		system.ServerCountSampleRecorder.Record();

		var sample = system.KeyValueStore.Samples().Single();
		sample.Count.Should().Be(1);
		sample.Queues.Should().Have.SameSequenceAs(["default"]);
	}

	[Test]
	public void ShouldRecordSeparateSamplesForDifferentQueues()
	{
		var system = new SystemUnderTest();
		system
			.WithConfiguration(new StoredConfiguration())
			.WithAnnouncedServer("defaultServer", queues: ["default"])
			.WithAnnouncedServer("reportsServer", queues: ["reports"]);

		system.ServerCountSampleRecorder.Record();

		var samples = system.KeyValueStore.Samples().ToArray();
		samples.Length.Should().Be(2);
	}

	[Test]
	public void ShouldCountServersWithSameQueues()
	{
		var system = new SystemUnderTest();
		system
			.WithConfiguration(new StoredConfiguration())
			.WithAnnouncedServer("server1", queues: ["default", "email"])
			.WithAnnouncedServer("server2", queues: ["default", "email"]);

		system.ServerCountSampleRecorder.Record();

		var sample = system.KeyValueStore.Samples().Single();
		sample.Count.Should().Be(2);
		sample.Queues.Should().Have.SameSequenceAs(["default", "email"]);
	}

	[Test]
	public void ShouldGroupByExactQueueSet()
	{
		var system = new SystemUnderTest();
		system
			.WithConfiguration(new StoredConfiguration())
			.WithAnnouncedServer("server1", queues: ["default", "email"])
			.WithAnnouncedServer("server2", queues: ["default", "email"])
			.WithAnnouncedServer("server3", queues: ["reports"]);

		system.ServerCountSampleRecorder.Record();

		var samples = system.KeyValueStore.Samples().ToArray();
		samples.Length.Should().Be(2);

		var defaultSample = samples.Single(s => s.Queues.SequenceEqual(["default", "email"]));
		defaultSample.Count.Should().Be(2);

		var reportsSample = samples.Single(s => s.Queues.SequenceEqual(["reports"]));
		reportsSample.Count.Should().Be(1);
	}

	[Test]
	public void ShouldKeep6SamplesPerQueueGroup()
	{
		var system = new SystemUnderTest();
		system
			.WithConfiguration(new StoredConfiguration())
			.WithAnnouncedServer("defaultServer", queues: ["default"])
			.WithAnnouncedServer("reportsServer", queues: ["reports"]);

		7.Times(x =>
		{
			var minute = x * 10;
			var time = "2020-12-01 12:00".Utc().AddMinutes(minute);
			system.Now(time);
			system.ServerCountSampleRecorder.Record();
		});

		system.KeyValueStore.Samples().Count().Should().Be(12);
	}
}
