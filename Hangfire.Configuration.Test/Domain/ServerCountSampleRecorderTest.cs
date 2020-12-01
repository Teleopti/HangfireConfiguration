using System;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ServerCountSampleRecorderTest
    {
        [Fact]
        public void ShouldStartRecorder()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.WorkerServerStarter.Start(null, null, null);

            Assert.Single(system.Hangfire.StartedServers.Single().backgroundProcesses
                .OfType<ServerCountSampleRecorder>());
        }

        [Fact]
        public void ShouldRecord()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
            system.Monitor.AnnounceServer("runningServer", null);

            system.ServerCountSampleRecorder.Record();

            Assert.Equal(1, system.ServerCountSampleStorage.Samples().Single().Count);
        }

        [Fact]
        public void ShouldRecordBoth()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
            system.Monitor.AnnounceServer("runningServer1", null);
            system.Monitor.AnnounceServer("runningServer2", null);

            system.ServerCountSampleRecorder.Record();

            Assert.Equal(2, system.ServerCountSampleStorage.Samples().Single().Count);
        }

        [Fact]
        public void ShouldNotBoomWithoutConfigurations()
        {
            var system = new SystemUnderTest();

            system.ServerCountSampleRecorder.Record();

            Assert.Empty(system.ServerCountSampleStorage.Samples());
        }

        [Fact]
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

            Assert.Equal(2, system.ServerCountSampleStorage.Samples().Single().Count);
        }

        [Fact]
        public void ShouldRecordWithTimestamp()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithAnnouncedServer("runningServer");

            system.Now("2020-12-01 12:00");
            system.ServerCountSampleRecorder.Record();

            Assert.Equal("2020-12-01 12:00".Utc(), system.ServerCountSampleStorage.Samples().Single().Timestamp);
        }

        [Fact]
        public void ShouldNotRecordDuplicateSample()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:00".Utc(), Count = 2})
                .WithAnnouncedServer("runningServer");

            system.Now("2020-12-01 12:00");
            system.ServerCountSampleRecorder.Record();

            Assert.Equal(2, system.ServerCountSampleStorage.Samples().Single().Count);
        }
        
        [Fact]
        public void ShouldRecordNewSampleAfter10Minutes()
        {
            var system = new SystemUnderTest();
            system
                .WithConfiguration(new StoredConfiguration())
                .WithServerCountSample(new ServerCountSample {Timestamp = "2020-12-01 12:00".Utc(), Count = 2})
                .WithAnnouncedServer("runningServer");

            system.Now("2020-12-01 12:10");
            system.ServerCountSampleRecorder.Record();

            var actual = system.ServerCountSampleStorage.Samples().Single(x => x.Timestamp == "2020-12-01 12:10".Utc());
            Assert.Equal(1, actual.Count);
        }
    }
}