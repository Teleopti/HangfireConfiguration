using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.PostgreSql;
using Npgsql;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain
{
    public class QueryPublishersPostgresTest
    {
        [Test]
        public void ShouldQueryPublishers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true});
            system.PublisherStarter.Start();

            var storage = system.QueryPublishers();

            Assert.NotNull(storage.Single());
        }

        [Test]
        public void ShouldReturnStorageWithCorrectConnectionString()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Host=loscalhost;Database=fakedb;" });
            system.PublisherStarter.Start();

            var storage = system.QueryPublishers()
                .Single().JobStorage as FakeJobStorage;

            Assert.AreEqual("Host=loscalhost;Database=fakedb;", storage.ConnectionString);
        }

        [Test]
        public void ShouldReturnCreatedStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true});
            system.PublisherStarter.Start();

            var storage = system.QueryPublishers().Single().JobStorage;

            storage.Should().Be.SameInstanceAs(system.Hangfire.CreatedStorages.Single());
        }

        [Test]
        public void ShouldReturnTheActiveStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Host=loscalhost;Database=fakedb;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});
            system.PublisherStarter.Start();

            var storage = system.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.AreEqual("Host=loscalhost;Database=fakedb;", storage.ConnectionString);
        }

        [Test]
        public void ShouldReturnTheActiveStorageAfterServerStart()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Host=loscalhost;Database=fakedb;" });
            system.WorkerServerStarter.Start();

            var storage = system.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.AreEqual("Host=loscalhost;Database=fakedb;", storage.ConnectionString);
        }

        [Test]
        public void ShouldReturnTheChangedActiveStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = "Host=loscalhost;Database=one;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false, ConnectionString = "Host=loscalhost;Database=two;" });
            system.PublisherStarter.Start();
            var activatedId = system.ConfigurationStorage.ReadConfigurations().Single(x => !x.Active.Value).Id.Value;
            var inactivatedId = system.ConfigurationStorage.ReadConfigurations().Single(x => x.Active.Value).Id.Value;
            system.ConfigurationApi().ActivateServer(activatedId);
            system.ConfigurationApi().InactivateServer(inactivatedId);

            var storage = system.QueryPublishers().Single().JobStorage as FakeJobStorage;

            Assert.AreEqual("Host=loscalhost;Database=two;", storage.ConnectionString);
        }

        [Test]
        public void ShouldAutoUpdate()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());

            system.UseOptions(new ConfigurationOptionsForTest
            {
	            UpdateConfigurations = new[]
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new NpgsqlConnectionStringBuilder {Host = "Hangfire"}.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            });
            system.QueryPublishers();

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
				ConnectionString = "Host=loscalhost;Database=fakedb;"
            });

            system.UseStorageOptions(new PostgreSqlStorageOptions {PrepareSchemaIfNecessary = false});
            system.QueryPublishers();

			Assert.False(system.Hangfire.CreatedStorages.Single().PostgresOptions.PrepareSchemaIfNecessary);
        }
        
        [Test]
        public void ShouldReturnTheChangedActiveStorageWhenInactiveWasDeleted()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 1, Active = true, ConnectionString = "Host=loscalhost;Database=1;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 2, Active = false, ConnectionString = "Host=loscalhost;Database=2;" });
            system.QueryPublishers();
            system.ConfigurationStorage.Remove(1);
            system.ConfigurationApi().ActivateServer(2);
            
            var storage = system.QueryPublishers();
            
            Assert.AreEqual("Host=loscalhost;Database=2;", (storage.Single().JobStorage as FakeJobStorage).ConnectionString);
        }
        
        [Test]
        public void ShouldReturnStorageConfigurationId()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Id = 4, Active = true});

            var configurationInfo = system.QueryPublishers().Single();

            Assert.AreEqual(4, configurationInfo.ConfigurationId);
        }

        [Test]
        public void ShouldReturnConfigurationName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Name = "name", Active = true});

            var configurationInfo = system.QueryPublishers().Single();

            Assert.AreEqual("name", configurationInfo.Name);
        }
    }
}