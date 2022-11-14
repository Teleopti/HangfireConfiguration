using System;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.SqlServer;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
    public class StartPublishersTest
    {
        [Test]
        public void ShouldConfigureAndStartWithAutoUpdatedConnectionString()
        {
            var system = new SystemUnderTest();

            system.UseOptions(new ConfigurationOptionsForTest
            {
	            UpdateConfigurations = new[]
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder {DataSource = "Hangfire"}.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            });
            system.PublisherStarter.Start();

            Assert.NotNull(system.Hangfire.LastCreatedStorage);
        }

        //Should this maybe throw???? 
        [Test]
        public void ShouldNotStart()
        {
            var system = new SystemUnderTest();

            system.PublisherStarter.Start();

            Assert.Null(system.Hangfire.LastCreatedStorage);
        }

        [Test]
        public void ShouldStartWithExistingActiveConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true});

            system.PublisherStarter.Start();

            Assert.NotNull(system.Hangfire.LastCreatedStorage);
        }

        [Test]
        public void ShouldStartWithActiveStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false, ConnectionString = "Data Source=."});
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true, 
	            SchemaName = "ActiveSchema",
	            ConnectionString = "Data Source=."
            });

            system.PublisherStarter.Start();

            Assert.AreEqual("ActiveSchema", system.Hangfire.LastCreatedStorage.SqlServerOptions.SchemaName);
        }

		[Test]
		public void ShouldPassDefaultStorageOptionsToHangfire()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration { Active = true, ConnectionString = "Data Source=."});

			system.PublisherStarter.Start();

			var options = new SqlServerStorageOptions();
			var storage = system.Hangfire.CreatedStorages.Single();
			Assert.AreEqual(options.QueuePollInterval, storage.SqlServerOptions.QueuePollInterval);
			Assert.AreEqual(options.SlidingInvisibilityTimeout, storage.SqlServerOptions.SlidingInvisibilityTimeout);
			Assert.AreEqual(options.JobExpirationCheckInterval, storage.SqlServerOptions.JobExpirationCheckInterval);
			Assert.AreEqual(options.CountersAggregateInterval, storage.SqlServerOptions.CountersAggregateInterval);
			Assert.AreEqual(options.PrepareSchemaIfNecessary, storage.SqlServerOptions.PrepareSchemaIfNecessary);
			Assert.AreEqual(options.DashboardJobListLimit, storage.SqlServerOptions.DashboardJobListLimit);
			Assert.AreEqual(options.TransactionTimeout, storage.SqlServerOptions.TransactionTimeout);
			Assert.AreEqual(options.DisableGlobalLocks, storage.SqlServerOptions.DisableGlobalLocks);
			Assert.AreEqual(options.UsePageLocksOnDequeue, storage.SqlServerOptions.UsePageLocksOnDequeue);
		}

		[Test]
		public void ShouldUseStorageOptions()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Active = true,
				ConnectionString = "Data Source=."
			});
			var options = new SqlServerStorageOptions
			{
				QueuePollInterval = TimeSpan.FromSeconds(1.0),
				SlidingInvisibilityTimeout = TimeSpan.FromSeconds(2),
				JobExpirationCheckInterval = TimeSpan.FromMinutes(4),
				CountersAggregateInterval = TimeSpan.FromMinutes(5.0),
				PrepareSchemaIfNecessary = !new SqlServerStorageOptions().PrepareSchemaIfNecessary,
				DashboardJobListLimit = 6,
				TransactionTimeout = TimeSpan.FromMinutes(7.0),
				DisableGlobalLocks = !new SqlServerStorageOptions().DisableGlobalLocks,
				UsePageLocksOnDequeue = !new SqlServerStorageOptions().UsePageLocksOnDequeue
			};

			system.UseStorageOptions(options);
			system.PublisherStarter.Start();

			var storage = system.Hangfire.CreatedStorages.Single();
			Assert.AreEqual(options.QueuePollInterval, storage.SqlServerOptions.QueuePollInterval);
			Assert.AreEqual(options.SlidingInvisibilityTimeout, storage.SqlServerOptions.SlidingInvisibilityTimeout);
			Assert.AreEqual(options.JobExpirationCheckInterval, storage.SqlServerOptions.JobExpirationCheckInterval);
			Assert.AreEqual(options.CountersAggregateInterval, storage.SqlServerOptions.CountersAggregateInterval);
			Assert.AreEqual(options.PrepareSchemaIfNecessary, storage.SqlServerOptions.PrepareSchemaIfNecessary);
			Assert.AreEqual(options.DashboardJobListLimit, storage.SqlServerOptions.DashboardJobListLimit);
			Assert.AreEqual(options.TransactionTimeout, storage.SqlServerOptions.TransactionTimeout);
			Assert.AreEqual(options.DisableGlobalLocks, storage.SqlServerOptions.DisableGlobalLocks);
			Assert.AreEqual(options.UsePageLocksOnDequeue, storage.SqlServerOptions.UsePageLocksOnDequeue);
		}

		[Test]
        public void ShouldUseSchemaNameFromConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true, 
	            SchemaName = "SchemaName",
	            ConnectionString = "Data Source=."
            });

            system.PublisherStarter.Start();

            Assert.AreEqual("SchemaName", system.Hangfire.CreatedStorages.Single().SqlServerOptions.SchemaName);
        }

        [Test]
        public void ShouldUseSchemaNameFromTwoConfigurations()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true, 
	            SchemaName = "SchemaName1",
	            ConnectionString = "Data Source=."
            });
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true, 
	            SchemaName = "SchemaName2",
	            ConnectionString = "Data Source=."
            });

            system.PublisherStarter.Start();

            var storages = system.Hangfire.CreatedStorages;
            Assert.AreEqual("SchemaName1", storages.First().SqlServerOptions.SchemaName);
            Assert.AreEqual("SchemaName2", storages.Last().SqlServerOptions.SchemaName);
        }

        [Test]
        public void ShouldNotCreateInactiveStorages()
        {
            var system = new SystemUnderTest();

            const string connectionString = "Data Source=fake;Integrated Security=SSPI;Initial Catalog=db;";
			system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = connectionString });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});

            system.PublisherStarter.Start();

            Assert.AreEqual(connectionString, system.Hangfire.CreatedStorages.Single().ConnectionString);
        }
    }
}