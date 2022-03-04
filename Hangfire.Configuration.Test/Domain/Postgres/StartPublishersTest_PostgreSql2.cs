using System.Data.SqlClient;
using System.Linq;
using Hangfire.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace Hangfire.Configuration.Test.Domain.Postgres
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
			                ConnectionString = new SqlConnectionStringBuilder
			                {
				                DataSource = "Hangfire"
			                }.ToString(),
			                Name = DefaultConfigurationName.Name()
		                }
	                }
                }, (PostgreSqlStorageOptions)null);

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
            system.ConfigurationStorage.Has(new StoredConfiguration { ConnectionString = @"Host=localhost;Database=fakedb;" });
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true, 
	            SchemaName = "ActiveSchema", 
	            ConnectionString = @"Host=localhost;Database=fakedb;"
            });

            system.PublisherStarter.Start();

            Assert.Equal("ActiveSchema", system.Hangfire.LastCreatedStorage.PostgresOptions.SchemaName);
        }

		[Fact]
		public void ShouldPassDefaultStorageOptionsToHangfire()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration { Active = true, ConnectionString = @"Host=localhost;Database=active;" });

			system.PublisherStarter.Start();

			var options = new PostgreSqlStorageOptions();
			var storage = system.Hangfire.CreatedStorages.Single();
			Assert.Equal(options.QueuePollInterval, storage.PostgresOptions.QueuePollInterval);
			Assert.Equal(options.DeleteExpiredBatchSize, storage.PostgresOptions.DeleteExpiredBatchSize);
			Assert.Equal(options.JobExpirationCheckInterval, storage.PostgresOptions.JobExpirationCheckInterval);
			Assert.Equal(options.DistributedLockTimeout, storage.PostgresOptions.DistributedLockTimeout);
			Assert.Equal(options.PrepareSchemaIfNecessary, storage.PostgresOptions.PrepareSchemaIfNecessary);
			Assert.Equal(options.EnableTransactionScopeEnlistment, storage.PostgresOptions.EnableTransactionScopeEnlistment);
			Assert.Equal(options.InvisibilityTimeout, storage.PostgresOptions.InvisibilityTimeout);
			Assert.Equal(options.SchemaName, storage.PostgresOptions.SchemaName);
			Assert.Equal(options.TransactionSynchronisationTimeout, storage.PostgresOptions.TransactionSynchronisationTimeout);
			Assert.Equal(options.UseNativeDatabaseTransactions, storage.PostgresOptions.UseNativeDatabaseTransactions);
		}

		//[Fact]
		//public void ShouldUseStorageOptions()
		//{
		//    var system = new SystemUnderTest();
		//    system.ConfigurationStorage.Has(new StoredConfiguration {Active = true});
		//    var options = new SqlServerStorageOptions
		//    {
		//        QueuePollInterval = TimeSpan.FromSeconds(1.0),
		//        SlidingInvisibilityTimeout = TimeSpan.FromSeconds(2),
		//        JobExpirationCheckInterval = TimeSpan.FromMinutes(4),
		//        CountersAggregateInterval = TimeSpan.FromMinutes(5.0),
		//        PrepareSchemaIfNecessary = !new SqlServerStorageOptions().PrepareSchemaIfNecessary,
		//        DashboardJobListLimit = 6,
		//        TransactionTimeout = TimeSpan.FromMinutes(7.0),
		//        DisableGlobalLocks = !new SqlServerStorageOptions().DisableGlobalLocks,
		//        UsePageLocksOnDequeue = !new SqlServerStorageOptions().UsePageLocksOnDequeue
		//    };

		//    system.PublisherStarter.Start(null, options);

		//    var storage = system.Hangfire.CreatedStorages.Single();
		//    Assert.Equal(options.QueuePollInterval, storage.Options.QueuePollInterval);
		//    Assert.Equal(options.SlidingInvisibilityTimeout, storage.Options.SlidingInvisibilityTimeout);
		//    Assert.Equal(options.JobExpirationCheckInterval, storage.Options.JobExpirationCheckInterval);
		//    Assert.Equal(options.CountersAggregateInterval, storage.Options.CountersAggregateInterval);
		//    Assert.Equal(options.PrepareSchemaIfNecessary, storage.Options.PrepareSchemaIfNecessary);
		//    Assert.Equal(options.DashboardJobListLimit, storage.Options.DashboardJobListLimit);
		//    Assert.Equal(options.TransactionTimeout, storage.Options.TransactionTimeout);
		//    Assert.Equal(options.DisableGlobalLocks, storage.Options.DisableGlobalLocks);
		//    Assert.Equal(options.UsePageLocksOnDequeue, storage.Options.UsePageLocksOnDequeue);
		//}

		[Fact]
        public void ShouldUseSchemaNameFromConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true, 
	            SchemaName = "SchemaName",
	            ConnectionString = @"Host=localhost;Database=fakedb;"
			});

            system.PublisherStarter.Start();

            Assert.Equal("SchemaName", system.Hangfire.CreatedStorages.Single().PostgresOptions.SchemaName);
        }

        [Fact]
        public void ShouldUseSchemaNameFromTwoConfigurations()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true, 
	            SchemaName = "SchemaName1",
	            ConnectionString = @"Host=localhost;Database=fakedb;"
			});
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true, 
	            SchemaName = "SchemaName2",
	            ConnectionString = @"Host=localhost;Database=fakedb;"
			});

            system.PublisherStarter.Start();

            var storages = system.Hangfire.CreatedStorages;
            Assert.Equal("SchemaName1", storages.First().PostgresOptions.SchemaName);
            Assert.Equal("SchemaName2", storages.Last().PostgresOptions.SchemaName);
        }

        [Fact]
        public void ShouldNotCreateInactiveStorages()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false, ConnectionString = @"Host=localhost;Database=fakedb;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = @"Host=localhost;Database=active;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false, ConnectionString = @"Host=localhost;Database=fakedb;" });

            system.PublisherStarter.Start();

            Assert.Equal(@"Host=localhost;Database=active;", system.Hangfire.CreatedStorages.Single().ConnectionString);
        }
    }
}