using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.SqlServer;
using Xunit;

namespace Hangfire.Configuration.Test.Domain.SqlServer
{
    public class QueryPublishersTest
    {
        [Fact]
        public void ShouldQueryPublishers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true});
            system.PublisherStarter.Start();

            var storage = system.PublisherQueries.QueryPublishers();

            Assert.NotNull(storage.Single());
        }

        [Fact]
        public void ShouldReturnStorageWithCorrectConnectionString()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Data Source=.;Initial Catalog=fakedb;" });
            system.PublisherStarter.Start();

            var storage = system.PublisherQueries.QueryPublishers()
                .Single().JobStorage as FakeJobStorage;

            Assert.Equal("Data Source=.;Initial Catalog=fakedb;", storage.ConnectionString);
        }

        [Fact]
        public void ShouldReturnCreatedStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true});
            system.PublisherStarter.Start();

            var storage = system.PublisherQueries.QueryPublishers().Single().JobStorage;

            Assert.Same(system.Hangfire.CreatedStorages.Single(), storage);
        }

        [Fact]
        public void ShouldReturnTheActiveStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Data Source=.;Initial Catalog=fakedb;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});
            system.PublisherStarter.Start();

            var storage = system.PublisherQueries.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.Equal("Data Source=.;Initial Catalog=fakedb;", storage.ConnectionString);
        }

        [Fact]
        public void ShouldReturnTheActiveStorageAfterServerStart()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Data Source=.;Initial Catalog=fakedb;" });
            system.WorkerServerStarter.Start();

            var storage = system.PublisherQueries.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.Equal("Data Source=.;Initial Catalog=fakedb;", storage.ConnectionString);
        }

        [Fact]
        public void ShouldReturnTheChangedActiveStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Data Source=.;Initial Catalog=one;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false, ConnectionString = "Data Source=.;Initial Catalog=two;" });
            system.PublisherStarter.Start();
            var configurationId = system.ConfigurationStorage.ReadConfigurations().Single(x => !x.Active.Value).Id.Value;
            system.ConfigurationApi.ActivateServer(configurationId);

            var storage = system.PublisherQueries.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.Equal("Data Source=.;Initial Catalog=two;", storage.ConnectionString);
        }

        [Fact]
        public void ShouldAutoUpdate()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.PublisherQueries
                .QueryPublishers(
                    new ConfigurationOptions
                    {
	                    UpdateConfigurations = new []
	                    {
		                    new UpdateStorageConfiguration
		                    {
			                    ConnectionString = new SqlConnectionStringBuilder{ DataSource = "Hangfire" }.ToString(),
			                    Name = DefaultConfigurationName.Name()
		                    }
	                    }
                    }, new SqlServerStorageOptions());

            Assert.Contains("Hangfire", system.ConfigurationStorage.Data.Single().ConnectionString);
        }

        [Fact]
        public void ShouldQueryPublishersWithDefaultStorageOptions()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true,
				ConnectionString = "Data Source=.;Initial Catalog=fakedb;"
            });

            system.PublisherQueries.QueryPublishers(null, new SqlServerStorageOptions {PrepareSchemaIfNecessary = false});

			Assert.False(system.Hangfire.CreatedStorages.Single().SqlServerOptions.PrepareSchemaIfNecessary);
        }

        
        [Fact]
        public void ShouldReturnTheChangedActiveStorageWhenInactiveWasDeleted()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 1, Active = true, ConnectionString = "Data Source=.;Initial Catalog=1;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 2, Active = false, ConnectionString = "Data Source=.;Initial Catalog=2;" });
            system.PublisherQueries.QueryPublishers();
            system.ConfigurationStorage.Data = system.ConfigurationStorage.Data.Where(x => x.Id == 2).ToArray();
            system.ConfigurationApi.ActivateServer(2);
            
            var storage = system.PublisherQueries.QueryPublishers();
            
            Assert.Equal("Data Source=.;Initial Catalog=2;", (storage.Single().JobStorage as FakeJobStorage).ConnectionString);
        }
        
        [Fact]
        public void ShouldReturnStorageConfigurationId()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 4, Active = true});

            var configurationInfo = system.PublisherQueries.QueryPublishers()
                .Single();

            Assert.Equal(4, configurationInfo.ConfigurationId);
        }

        [Fact]
        public void ShouldReturnConfigurationName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Name = "name", Active = true});

            var configurationInfo = system.PublisherQueries.QueryPublishers()
                .Single();

            Assert.Equal("name", configurationInfo.Name);
        }
    }
}