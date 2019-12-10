using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Xunit;

namespace Hangfire.Configuration.Test.Api
{
    [Collection("NotParallel")]
    public class CurrentStaticTest
    {
        [Fact]
        public void ShouldQueryPublishersAfterStartingPublishers()
        {
            var system = new SystemUnderTest();
            HangfireConfiguration.UseHangfireConfiguration(null, null, new Dictionary<string, object>() {{"CompositionRoot", system}});
            system.Repository.Has(new StoredConfiguration {Active = true, ConnectionString = "active"});
            system.PublisherStarter.Start(null, null);

            var storage = HangfireConfiguration.Current.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.Equal("active", storage.ConnectionString);
        }
        
        [Fact]
        public void ShouldQueryPublishersAfterStartingServers()
        {
            var system = new SystemUnderTest();
            HangfireConfiguration.UseHangfireConfiguration(null, null, new Dictionary<string, object>() {{"CompositionRoot", system}});
            system.Repository.Has(new StoredConfiguration {Active = true, ConnectionString = "active"});
            system.WorkerServerStarter.Start(null, null, null);

            var storage = HangfireConfiguration.Current.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.Equal("active", storage.ConnectionString);
        }
    }
}