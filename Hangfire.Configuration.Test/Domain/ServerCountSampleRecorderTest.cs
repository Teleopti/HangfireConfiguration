using System;
using System.Linq;
using Hangfire.Common;
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
    }
}