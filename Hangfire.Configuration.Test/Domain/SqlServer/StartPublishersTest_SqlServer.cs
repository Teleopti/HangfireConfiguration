using System;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Hangfire.Configuration.Test.Domain.SqlServer
{
    public class StartPublishersTest : XunitContextBase
    {
        public StartPublishersTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void ShouldConfigureAndStartWithAutoUpdatedConnectionString()
        {
            var system = new SystemUnderTest();

            system.PublisherStarter.Start(
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
                }, (SqlServerStorageOptions)null);

            Assert.NotNull(system.Hangfire.LastCreatedStorage);
        }

        //Should this maybe throw???? 
        [Fact]
        public void ShouldNotStart()
        {
            var system = new SystemUnderTest();

            system.PublisherStarter.Start();

            Assert.Null(system.Hangfire.LastCreatedStorage);
        }

        [Fact]
        public void ShouldStartWithExistingActiveConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true});

            system.PublisherStarter.Start();

            Assert.NotNull(system.Hangfire.LastCreatedStorage);
        }

        [Fact]
        public void ShouldStartWithActiveStorage()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration());
            system.ConfigurationStorage.Has(new StoredConfiguration() {Active = true, SchemaName = "ActiveSchema"});

            system.PublisherStarter.Start();

            Assert.Equal("ActiveSchema", system.Hangfire.LastCreatedStorage.SqlServerOptions.SchemaName);
        }

		[Fact]
		public void ShouldPassDefaultStorageOptionsToHangfire()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration { Active = true });

			system.PublisherStarter.Start();

			var options = new SqlServerStorageOptions();
			var storage = system.Hangfire.CreatedStorages.Single();
			Assert.Equal(options.QueuePollInterval, storage.SqlServerOptions.QueuePollInterval);
			Assert.Equal(options.SlidingInvisibilityTimeout, storage.SqlServerOptions.SlidingInvisibilityTimeout);
			Assert.Equal(options.JobExpirationCheckInterval, storage.SqlServerOptions.JobExpirationCheckInterval);
			Assert.Equal(options.CountersAggregateInterval, storage.SqlServerOptions.CountersAggregateInterval);
			Assert.Equal(options.PrepareSchemaIfNecessary, storage.SqlServerOptions.PrepareSchemaIfNecessary);
			Assert.Equal(options.DashboardJobListLimit, storage.SqlServerOptions.DashboardJobListLimit);
			Assert.Equal(options.TransactionTimeout, storage.SqlServerOptions.TransactionTimeout);
			Assert.Equal(options.DisableGlobalLocks, storage.SqlServerOptions.DisableGlobalLocks);
			Assert.Equal(options.UsePageLocksOnDequeue, storage.SqlServerOptions.UsePageLocksOnDequeue);
		}

		[Fact]
		public void ShouldUseStorageOptions()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration { Active = true });
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

			system.PublisherStarter.Start(null, options);

			var storage = system.Hangfire.CreatedStorages.Single();
			Assert.Equal(options.QueuePollInterval, storage.SqlServerOptions.QueuePollInterval);
			Assert.Equal(options.SlidingInvisibilityTimeout, storage.SqlServerOptions.SlidingInvisibilityTimeout);
			Assert.Equal(options.JobExpirationCheckInterval, storage.SqlServerOptions.JobExpirationCheckInterval);
			Assert.Equal(options.CountersAggregateInterval, storage.SqlServerOptions.CountersAggregateInterval);
			Assert.Equal(options.PrepareSchemaIfNecessary, storage.SqlServerOptions.PrepareSchemaIfNecessary);
			Assert.Equal(options.DashboardJobListLimit, storage.SqlServerOptions.DashboardJobListLimit);
			Assert.Equal(options.TransactionTimeout, storage.SqlServerOptions.TransactionTimeout);
			Assert.Equal(options.DisableGlobalLocks, storage.SqlServerOptions.DisableGlobalLocks);
			Assert.Equal(options.UsePageLocksOnDequeue, storage.SqlServerOptions.UsePageLocksOnDequeue);
		}

		[Fact]
        public void ShouldUseSchemaNameFromConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, SchemaName = "SchemaName"});

            system.PublisherStarter.Start();

            Assert.Equal("SchemaName", system.Hangfire.CreatedStorages.Single().SqlServerOptions.SchemaName);
        }

        [Fact]
        public void ShouldUseSchemaNameFromTwoConfigurations()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, SchemaName = "SchemaName1"});
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, SchemaName = "SchemaName2"});

            system.PublisherStarter.Start();

            var storages = system.Hangfire.CreatedStorages;
            Assert.Equal("SchemaName1", storages.First().SqlServerOptions.SchemaName);
            Assert.Equal("SchemaName2", storages.Last().SqlServerOptions.SchemaName);
        }

        [Fact]
        public void ShouldNotCreateInactiveStorages()
        {
            var system = new SystemUnderTest();

            var connectionString = "Data Source=fake;Integrated Security=SSPI;Initial Catalog=db;";
			system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = connectionString });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false});

            system.PublisherStarter.Start();

            Assert.Equal(connectionString, system.Hangfire.CreatedStorages.Single().ConnectionString);
        }
    }
}