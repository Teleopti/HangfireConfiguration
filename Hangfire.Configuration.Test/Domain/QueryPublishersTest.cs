using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.SqlServer;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain
{
    public class QueryPublishersTest
    {
        [Test]
        public void ShouldQueryPublishers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true});
            system.PublisherStarter.Start();

            var storage = system.PublisherQueries.QueryPublishers();

            Assert.NotNull(storage.Single());
        }

        [Test]
        public void ShouldReturnStorageWithCorrectConnectionString()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Data Source=.;Initial Catalog=fakedb;" });
            system.PublisherStarter.Start();

            var storage = system.PublisherQueries.QueryPublishers()
                .Single().JobStorage as FakeJobStorage;

            Assert.AreEqual("Data Source=.;Initial Catalog=fakedb;", storage.ConnectionString);
        }

        [Test]
        public void ShouldReturnCreatedStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true});
            system.PublisherStarter.Start();

            var storage = system.PublisherQueries.QueryPublishers().Single().JobStorage;

            storage.Should().Be.SameInstanceAs(system.Hangfire.CreatedStorages.Single());
        }

        [Test]
        public void ShouldReturnTheActiveStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Data Source=.;Initial Catalog=fakedb;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});
            system.PublisherStarter.Start();

            var storage = system.PublisherQueries.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.AreEqual("Data Source=.;Initial Catalog=fakedb;", storage.ConnectionString);
        }

        [Test]
        public void ShouldReturnTheActiveStorageAfterServerStart()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Data Source=.;Initial Catalog=fakedb;" });
            system.WorkerServerStarter.Start();

            var storage = system.PublisherQueries.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.AreEqual("Data Source=.;Initial Catalog=fakedb;", storage.ConnectionString);
        }

        [Test]
        public void ShouldReturnTheChangedActiveStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Data Source=.;Initial Catalog=one;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false, ConnectionString = "Data Source=.;Initial Catalog=two;" });
            system.PublisherStarter.Start();
            var activatedId = system.ConfigurationStorage.ReadConfigurations().Single(x => !x.Active.Value).Id.Value;
            var inactivatedId = system.ConfigurationStorage.ReadConfigurations().Single(x => x.Active.Value).Id.Value;
            system.ConfigurationApi.ActivateServer(activatedId);
            system.ConfigurationApi.InactivateServer(inactivatedId);

            var storage = system.PublisherQueries.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.AreEqual("Data Source=.;Initial Catalog=two;", storage.ConnectionString);
        }

        [Test]
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
                    });

            system.ConfigurationStorage.Data.Single().ConnectionString
	            .Should().Contain("Hangfire");
        }

        [Test]
        public void ShouldQueryPublishersWithDefaultStorageOptions()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true,
				ConnectionString = "Data Source=.;Initial Catalog=fakedb;"
            });

            system.UseStorageOptions(new SqlServerStorageOptions {PrepareSchemaIfNecessary = false});
            system.PublisherQueries.QueryPublishers();

			Assert.False(system.Hangfire.CreatedStorages.Single().SqlServerOptions.PrepareSchemaIfNecessary);
        }
        
        [Test]
        public void ShouldReturnTheChangedActiveStorageWhenInactiveWasDeleted()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 1, Active = true, ConnectionString = "Data Source=.;Initial Catalog=1;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 2, Active = false, ConnectionString = "Data Source=.;Initial Catalog=2;" });
            system.PublisherQueries.QueryPublishers();
            system.ConfigurationStorage.Remove(1);
            system.ConfigurationApi.ActivateServer(2);
            
            var storage = system.PublisherQueries.QueryPublishers();
            
            Assert.AreEqual("Data Source=.;Initial Catalog=2;", (storage.Single().JobStorage as FakeJobStorage).ConnectionString);
        }
        
        [Test]
        public void ShouldReturnStorageConfigurationId()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 4, Active = true});

            var configurationInfo = system.PublisherQueries.QueryPublishers()
                .Single();

            Assert.AreEqual(4, configurationInfo.ConfigurationId);
        }

        [Test]
        public void ShouldReturnConfigurationName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Name = "name", Active = true});

            var configurationInfo = system.PublisherQueries.QueryPublishers()
                .Single();

            Assert.AreEqual("name", configurationInfo.Name);
        }
        
        [Test]
        public void ShouldNotReturnNullActive()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, Id = 1});
	        system.ConfigurationStorage.Has(new StoredConfiguration {Active = null, Id = 2});
	        system.PublisherStarter.Start();

	        system.PublisherQueries.QueryPublishers().Single().ConfigurationId
		        .Should().Be(1);
        }
    }
}