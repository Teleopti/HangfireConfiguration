using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.SqlServer;
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

            var storage = system.PublisherQueries.QueryPublishers(null, null);

            Assert.NotNull(storage.Single());
        }

        [Fact]
        public void ShouldReturnStorageWithCorrectConnectionString()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true, ConnectionString = ";"});
            system.PublisherStarter.Start(null, null);

            var storage = system.PublisherQueries.QueryPublishers(null, null)
                .Single().JobStorage as FakeJobStorage;

            Assert.Equal(";", storage.ConnectionString);
        }

        [Fact]
        public void ShouldReturnCreatedStorage()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true});
            system.PublisherStarter.Start(null, null);

            var storage = system.PublisherQueries.QueryPublishers(null, null).Single().JobStorage;

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

            var storage = system.PublisherQueries.QueryPublishers(null, null).Single().JobStorage as FakeJobStorage;

            Assert.Equal("active", storage.ConnectionString);
        }

        [Fact]
        public void ShouldReturnTheActiveStorageAfterServerStart()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = false});
            system.Repository.Has(new StoredConfiguration {Active = true, ConnectionString = "string"});
            system.WorkerServerStarter.Start(null, null, null);

            var storage = system.PublisherQueries.QueryPublishers(null, null).Single().JobStorage as FakeJobStorage;

            Assert.Equal("string", storage.ConnectionString);
        }

        [Fact]
        public void ShouldReturnTheChangedActiveStorage()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true, ConnectionString = "one"});
            system.Repository.Has(new StoredConfiguration {Active = false, ConnectionString = "two"});
            system.PublisherStarter.Start(null, null);
            var configurationId = system.Repository.ReadConfigurations().Single(x => !x.Active.Value).Id.Value;
            system.ConfigurationApi.ActivateServer(configurationId);

            var storage = system.PublisherQueries.QueryPublishers(null, null).Single().JobStorage as FakeJobStorage;

            Assert.Equal("two", storage.ConnectionString);
        }

        [Fact]
        public void ShouldAutoUpdate()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration());

            system.PublisherQueries
                .QueryPublishers(
                    new ConfigurationOptions
                    {
                        AutoUpdatedHangfireConnectionString = new SqlConnectionStringBuilder {DataSource = "Hangfire"}.ToString()
                    }, null);

            Assert.Contains("Hangfire", system.Repository.Data.Single().ConnectionString);
        }

        [Fact]
        public void ShouldQueryPublishersWithDefaultSqlStorageOptions()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Active = true});

            system.PublisherQueries.QueryPublishers(null, new SqlServerStorageOptions {PrepareSchemaIfNecessary = false});

            Assert.False(system.Hangfire.CreatedStorages.Single().Options.PrepareSchemaIfNecessary);
        }
        
        [Fact]
        public void ShouldReturnTheChangedActiveStorageWhenInactiveWasDeleted()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Id = 1, Active = true, ConnectionString = "one"});
            system.Repository.Has(new StoredConfiguration {Id = 2, Active = false, ConnectionString = "two"});
            system.PublisherQueries.QueryPublishers(null, null);
            system.Repository.Data = system.Repository.Data.Where(x => x.Id == 2).ToArray();
            system.ConfigurationApi.ActivateServer(2);
            
            var storage = system.PublisherQueries.QueryPublishers(null, null);
            
            Assert.Equal("two", (storage.Single().JobStorage as FakeJobStorage).ConnectionString);
        }
        
        [Fact]
        public void ShouldReturnStorageConfigurationId()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration {Id = 4, Active = true});

            var configurationInfo = system.PublisherQueries.QueryPublishers(null, null)
                .Single();

            Assert.Equal(4, configurationInfo.ConfigurationId);
        }
    }
}