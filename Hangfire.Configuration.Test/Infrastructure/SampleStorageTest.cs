using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("NotParallel")]
    public class SampleStorageTest
    {
        [Fact, CleanDatabase]
        public void ShouldWrite()
        {
            var system = new SystemUnderInfraTest();
            system.BuildOptions().UseOptions(new ConfigurationOptions
                {ConnectionString = ConnectionUtils.GetConnectionString()});

            system.KeyValueStore.Write(new ServerCountSamples
            {Samples = new[] {new ServerCountSample {Timestamp = "2020-12-02 12:00".Utc(), Count = 1}}});

            var sample = system.KeyValueStore.Read().Samples.Single();
            Assert.Equal("2020-12-02 12:00".Utc(), sample.Timestamp);
            Assert.Equal(1, sample.Count);
        }

        [Fact, CleanDatabase]
        public void ShouldReadEmpty()
        {
            var system = new SystemUnderInfraTest();
            system.WithOptions(new ConfigurationOptions {ConnectionString = ConnectionUtils.GetConnectionString()});

            var sample = system.KeyValueStore.Read();

            Assert.Empty(sample.Samples);
        }

        [Fact, CleanDatabase]
        public void ShouldUpdate()
        {
            var system = new SystemUnderInfraTest();
            system.WithOptions(new ConfigurationOptions {ConnectionString = ConnectionUtils.GetConnectionString()});

            system.KeyValueStore.Write(new ServerCountSamples
            { Samples = new[] {new ServerCountSample {Count = 1} }});
            system.KeyValueStore.Write(new ServerCountSamples
            { Samples = new[] {new ServerCountSample {Count = 2} }});

            var sample = system.KeyValueStore.Read().Samples.Single();
            Assert.Equal(2, sample.Count);
        }
    }
}