using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain
{
    public class ServerCountSampleRecorderTest
    {
        [Test]
        public void ShouldStartRecorder()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start();

            system.Hangfire.StartedServers.Single().backgroundProcesses
	            .OfType<ServerCountSampleRecorder>()
	            .Should().Have.Count.EqualTo(1);
        }

        [Test]
        public void ShouldNotStartRecorder()
        {
	        var options = new ConfigurationOptions();
	        options.WorkerDeterminerOptions.UseServerCountSampling = false;
	        var system = new SystemUnderTest();
	        system.UseOptions(options);
	        system.WithConfiguration(new StoredConfiguration());

	        system.WorkerServerStarter.Start();

	        Assert.IsEmpty(system.Hangfire.StartedServers.Single().backgroundProcesses
		        .OfType<ServerCountSampleRecorder>());
        }
        
        [Test]
        public void ShouldRecord()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
            system.Monitor.AnnounceServer("runningServer");

            system.ServerCountSampleRecorder.Record();

            Assert.AreEqual(1, system.KeyValueStore.Samples().Single().Count);
        }

        [Test]
        public void ShouldRecordBoth()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
            system.Monitor.AnnounceServer("runningServer1");
            system.Monitor.AnnounceServer("runningServer2");

            system.ServerCountSampleRecorder.Record();

            Assert.AreEqual(2, system.KeyValueStore.Samples().Single().Count);
        }

        [Test]
        public void ShouldNotBoomWithoutConfigurations()
        {
            var system = new SystemUnderTest();

            system.ServerCountSampleRecorder.Record();

            Assert.IsEmpty(system.KeyValueStore.Samples());
        }

        [Test]
        public void ShouldNotBoomWith2Configurations()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithConfiguration(new StoredConfiguration())
                .WithAnnouncedServer("runningServer1")
                .WithAnnouncedServer("runningServer2")
                ;

            system.ServerCountSampleRecorder.Record();

            Assert.AreEqual(2, system.KeyValueStore.Samples().Single().Count);
        }

        [Test]
        public void ShouldRecordWithTimestamp()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithAnnouncedServer("runningServer");

            system.Now("2020-12-01 12:00");
            system.ServerCountSampleRecorder.Record();

            Assert.AreEqual("2020-12-01 12:00".Utc(), system.KeyValueStore.Samples().Single().Timestamp);
        }

        [Test]
        public void ShouldNotRecordDuplicateSample()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:00".Utc(), Count = 2})
                .WithAnnouncedServer("runningServer");

            system.Now("2020-12-01 12:00");
            system.ServerCountSampleRecorder.Record();

            Assert.AreEqual(2, system.KeyValueStore.Samples().Single().Count);
        }

        [Test]
        public void ShouldRecordNewSampleAfter10Minutes()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:00".Utc(), Count = 2})
                .WithAnnouncedServer("runningServer");

            system.Now("2020-12-01 12:10");
            system.ServerCountSampleRecorder.Record();

            var actual = system.KeyValueStore.Samples().Single(x => x.Timestamp == "2020-12-01 12:10".Utc());
            Assert.AreEqual(1, actual.Count);
        }

        [Test]
        public void ShouldKeep6Samples()
        {
            var system = new SystemUnderTest();
            system.WithConfiguration(new StoredConfiguration());

            7.Times(x =>
            {
                var minute = x * 10;
                var time = "2020-12-01 12:00".Utc().AddMinutes(minute);
                system.Now(time);
                system.ServerCountSampleRecorder.Record();
            });

            Assert.AreEqual(6, system.KeyValueStore.Samples().Count());
        }

        [Test]
        public void ShouldRemoveOldestAndAddLatest()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:00".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:10".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:20".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:30".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:40".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:50".Utc()})
                ;

            system.Now("2020-12-01 13:00");
            system.ServerCountSampleRecorder.Record();

            system.KeyValueStore.Samples()
	            .FirstOrDefault(x => x.Timestamp == "2020-12-01 13:00".Utc())
	            .Should().Not.Be.Null();
            system.KeyValueStore.Samples()
	            .FirstOrDefault(x => x.Timestamp == "2020-12-01 12:00".Utc())
	            .Should().Be.Null();
        }

        [Test]
        public void ShouldNotRemoveWhenNotRecordable()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:00".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:10".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:20".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:30".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:40".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:50".Utc()})
                ;

            system.Now("2020-12-01 12:50");
            system.ServerCountSampleRecorder.Record();

            Assert.AreEqual(6, system.KeyValueStore.Samples().Count());
            system.KeyValueStore.Samples()
	            .FirstOrDefault(x => x.Timestamp == "2020-12-01 12:00".Utc())
	            .Should().Not.Be.Null();
        }

        [Test]
        public void ShouldKeep6Samples2()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:00".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:10".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:20".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:30".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:40".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:50".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 13:00".Utc()})
                ;

            system.Now("2020-12-01 13:10".Utc());
            system.ServerCountSampleRecorder.Record();

            Assert.AreEqual(6, system.KeyValueStore.Samples().Count());
        }
        
        [Test]
        public void ShouldKeepTheCorrect6Samples()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:00".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:10".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:20".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:30".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:40".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:50".Utc()})
                ;

            system.Now("2020-12-01 13:00");
            system.ServerCountSampleRecorder.Record();
            system.Now("2020-12-01 13:10");
            system.ServerCountSampleRecorder.Record();
            
            var actual = system.KeyValueStore
	            .Samples()
                .Select(x => x.Timestamp)
                .ToArray();
            Assert.AreEqual(new[] {
                "2020-12-01 12:20".Utc(),
                "2020-12-01 12:30".Utc(),
                "2020-12-01 12:40".Utc(),
                "2020-12-01 12:50".Utc(),
                "2020-12-01 13:00".Utc(),
                "2020-12-01 13:10".Utc(),
                }, actual);
        }
        
        [Test]
        public void ShouldKeepTheLatest6Samples()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:50".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:40".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:30".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:20".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:10".Utc()})
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:00".Utc()})
                ;

            system.Now("2020-12-01 13:00");
            system.ServerCountSampleRecorder.Record();
            system.Now("2020-12-01 13:10");
            system.ServerCountSampleRecorder.Record();
            
            var actual = system.KeyValueStore
	            .Samples()
                .Select(x => x.Timestamp)
                .ToArray();
            Assert.AreEqual(new[] {
                "2020-12-01 12:20".Utc(),
                "2020-12-01 12:30".Utc(),
                "2020-12-01 12:40".Utc(),
                "2020-12-01 12:50".Utc(),
                "2020-12-01 13:00".Utc(),
                "2020-12-01 13:10".Utc(),
            }, actual);
        }
    }
}