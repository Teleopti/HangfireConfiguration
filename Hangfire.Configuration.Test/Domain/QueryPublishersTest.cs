using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class QueryPublishersTest
    {
        [Fact]
        public void ShouldQueryPublishers()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true});
            system.PublisherStarter.Start(null, null);

            var storage = system.PublisherQueries.QueryPublishers();

            Assert.NotNull(storage.Single());
        }

        [Fact]
        public void ShouldReturnStorageWithCorrectConnectionString()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true, ConnectionString = ";"});
            system.PublisherStarter.Start(null, null);

            var storage = system.PublisherQueries.QueryPublishers()
                .Single() as FakeJobStorage;

            Assert.Equal(";", storage.ConnectionString);
        }

        [Fact]
        public void ShouldReturnCreatedStorage()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true});
            system.PublisherStarter.Start(null, null);

            var storage = system.PublisherQueries.QueryPublishers().Single();

            Assert.Same(system.Hangfire.CreatedStorages.Single(), storage);
        }

        [Fact]
        public void ShouldReturnTheActiveStorage()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = false});
            system.Repository.Has(new StoredConfiguration {Active = true, ConnectionString = "active"});
            system.Repository.Has(new StoredConfiguration {Active = false});
            system.PublisherStarter.Start(null, null);

            var storage = system.PublisherQueries.QueryPublishers().Single() as FakeJobStorage;

            Assert.Equal("active", storage.ConnectionString);
        }
        
        [Fact]
        public void ShouldReturnTheActiveStorageAfterServerStart()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = false});
            system.Repository.Has(new StoredConfiguration {Active = true, ConnectionString = "string"});
            system.WorkerServerStarter.Start(null, null, null);

            var storage = system.PublisherQueries.QueryPublishers().Single() as FakeJobStorage;

            Assert.Equal("string", storage.ConnectionString);
        }
    }
}