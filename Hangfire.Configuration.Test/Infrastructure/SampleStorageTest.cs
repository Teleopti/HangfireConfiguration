using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Parallelizable(ParallelScope.None)]
    [CleanDatabase]
    public class SampleStorageTest
    {
        [Test]
        public void ShouldWrite()
        {
            var system = new SystemUnderInfraTest();
            system.BuildOptions().UseOptions(new ConfigurationOptions
                {ConnectionString = ConnectionUtils.GetConnectionString()});

            system.KeyValueStore.ServerCountSamples(new ServerCountSamples
            {Samples = new[] {new ServerCountSample {Timestamp = "2020-12-02 12:00".Utc(), Count = 1}}});

            var sample = system.KeyValueStore.ServerCountSamples().Samples.Single();
            Assert.AreEqual("2020-12-02 12:00".Utc(), sample.Timestamp);
            Assert.AreEqual(1, sample.Count);
        }

        [Test]
        public void ShouldReadEmpty()
        {
            var system = new SystemUnderInfraTest();
            system.WithOptions(new ConfigurationOptions {ConnectionString = ConnectionUtils.GetConnectionString()});

            var sample = system.KeyValueStore.ServerCountSamples();

            Assert.IsEmpty(sample.Samples);
        }

        [Test]
        public void ShouldUpdate()
        {
            var system = new SystemUnderInfraTest();
            system.WithOptions(new ConfigurationOptions {ConnectionString = ConnectionUtils.GetConnectionString()});

            system.KeyValueStore.ServerCountSamples(new ServerCountSamples
            { Samples = new[] {new ServerCountSample {Count = 1} }});
            system.KeyValueStore.ServerCountSamples(new ServerCountSamples
            { Samples = new[] {new ServerCountSample {Count = 2} }});

            var sample = system.KeyValueStore.ServerCountSamples().Samples.Single();
            Assert.AreEqual(2, sample.Count);
        }
    }
}