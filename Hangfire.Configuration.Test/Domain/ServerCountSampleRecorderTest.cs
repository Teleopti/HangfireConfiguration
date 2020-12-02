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

        [Fact]
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

            Assert.Equal(6, system.ServerCountSampleStorage.Samples().Count());
        }
        
        [Fact]
        public void ShouldRemoveOldestAndAddLatest()
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

            Assert.Contains(system.ServerCountSampleStorage.Samples(), x => x.Timestamp == "2020-12-01 13:00".Utc());
            Assert.DoesNotContain(system.ServerCountSampleStorage.Samples(), x => x.Timestamp == "2020-12-01 12:00".Utc() );
        }
        
        [Fact]
        public void ShouldNotRemoveWhenNotRecordable()
        {
            var system = new SystemUnderTest();
            system.WithConfiguration(new StoredConfiguration());
            6.Times(x =>
            {
                var minute = x * 10;
                var time = "2020-12-01 12:00".Utc().AddMinutes(minute);
                system.Now(time);
                system.ServerCountSampleRecorder.Record();
            });
            
            system.ServerCountSampleRecorder.Record();

            Assert.Equal(6, system.ServerCountSampleStorage.Samples().Count());
            Assert.Contains(system.ServerCountSampleStorage.Samples(), x => x.Timestamp == "2020-12-01 12:00".Utc() );
        }
    }
}