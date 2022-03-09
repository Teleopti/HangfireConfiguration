using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Api
{
    [Parallelizable(ParallelScope.None)]
    public class CurrentStaticTest
    {
        [Test]
        public void ShouldQueryPublishersAfterStartingPublishers()
        {
            var system = new SystemUnderTest();
            HangfireConfiguration.UseHangfireConfiguration(null, null, new Dictionary<string, object>() {{"CompositionRoot", system}});
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Data Source=.;Initial Catalog=fakedb;"});
            system.PublisherStarter.Start();

            var storage = HangfireConfiguration.Current.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.AreEqual("Data Source=.;Initial Catalog=fakedb;", storage.ConnectionString);
        }
        
        [Test]
        public void ShouldQueryPublishersAfterStartingServers()
        {
            var system = new SystemUnderTest();
            HangfireConfiguration.UseHangfireConfiguration(null, null, new Dictionary<string, object>() {{"CompositionRoot", system}});
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Data Source=.;Initial Catalog=fakedb;" });
            system.WorkerServerStarter.Start();

            var storage = HangfireConfiguration.Current.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.AreEqual("Data Source=.;Initial Catalog=fakedb;", storage.ConnectionString);
        }
    }
}