using System;
using System.Linq;
using Hangfire.PostgreSql;
using Npgsql;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
	public class StartPublishersPostgresTest
    {
        [Test]
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
			                ConnectionString = new NpgsqlConnectionStringBuilder
			                {
				                Host = "Hangfire"
			                }.ToString(),
			                Name = DefaultConfigurationName.Name()
		                }
	                }
                });

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
            system.ConfigurationStorage.Has(new StoredConfiguration { Active = false, ConnectionString = @"Host=localhost;Database=fakedb;" });
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
	            Active = true, 
	            SchemaName = "ActiveSchema", 
	            ConnectionString = @"Host=localhost;Database=fakedb;"
            });

            system.PublisherStarter.Start();

            Assert.AreEqual("ActiveSchema", system.Hangfire.LastCreatedStorage.PostgresOptions.SchemaName);
        }

		[Test]
		public void ShouldPassDefaultStorageOptionsToHangfire()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration { Active = true, ConnectionString = @"Host=localhost;Database=active;" });

			system.PublisherStarter.Start();

			var options = new PostgreSqlStorageOptions();
			var storage = system.Hangfire.CreatedStorages.Single();
			Assert.AreEqual(options.QueuePollInterval, storage.PostgresOptions.QueuePollInterval);
			Assert.AreEqual(options.DeleteExpiredBatchSize, storage.PostgresOptions.DeleteExpiredBatchSize);
			Assert.AreEqual(options.JobExpirationCheckInterval, storage.PostgresOptions.JobExpirationCheckInterval);
			Assert.AreEqual(options.DistributedLockTimeout, storage.PostgresOptions.DistributedLockTimeout);
			Assert.AreEqual(options.PrepareSchemaIfNecessary, storage.PostgresOptions.PrepareSchemaIfNecessary);
			Assert.AreEqual(options.EnableTransactionScopeEnlistment, storage.PostgresOptions.EnableTransactionScopeEnlistment);
			Assert.AreEqual(options.InvisibilityTimeout, storage.PostgresOptions.InvisibilityTimeout);
			Assert.AreEqual(options.SchemaName, storage.PostgresOptions.SchemaName);
			Assert.AreEqual(options.TransactionSynchronisationTimeout, storage.PostgresOptions.TransactionSynchronisationTimeout);
			Assert.AreEqual(options.UseNativeDatabaseTransactions, storage.PostgresOptions.UseNativeDatabaseTransactions);
		}

		[Test]
		public void ShouldUseStorageOptions()
		{
		    var system = new SystemUnderTest();
		    system.ConfigurationStorage.Has(new StoredConfiguration
		    {
			    Active = true, 
			    ConnectionString = @"Host=localhost;Database=fakedb;"
		    });
		    var options = new PostgreSqlStorageOptions
		    {
		        QueuePollInterval = TimeSpan.FromSeconds(1.0),
		        JobExpirationCheckInterval = TimeSpan.FromMinutes(4),
		        PrepareSchemaIfNecessary = !new PostgreSqlStorageOptions().PrepareSchemaIfNecessary,
		    };

		    system.Options.UseStorageOptions(options);
		    system.PublisherStarter.Start();

		    var storage = system.Hangfire.CreatedStorages.Single();
		    Assert.AreEqual(options.QueuePollInterval, storage.PostgresOptions.QueuePollInterval);
		    Assert.AreEqual(options.JobExpirationCheckInterval, storage.PostgresOptions.JobExpirationCheckInterval);
		    Assert.AreEqual(options.PrepareSchemaIfNecessary, storage.PostgresOptions.PrepareSchemaIfNecessary);
		}

		[Test]
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

            Assert.AreEqual("SchemaName", system.Hangfire.CreatedStorages.Single().PostgresOptions.SchemaName);
        }

        [Test]
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
            Assert.AreEqual("SchemaName1", storages.First().PostgresOptions.SchemaName);
            Assert.AreEqual("SchemaName2", storages.Last().PostgresOptions.SchemaName);
        }

        [Test]
        public void ShouldNotCreateInactiveStorages()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false, ConnectionString = @"Host=localhost;Database=fakedb;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = true, ConnectionString = @"Host=localhost;Database=active;" });
            system.ConfigurationStorage.Has(new StoredConfiguration {Active = false, ConnectionString = @"Host=localhost;Database=fakedb;" });

            system.PublisherStarter.Start();

            Assert.AreEqual(@"Host=localhost;Database=active;", system.Hangfire.CreatedStorages.Single().ConnectionString);
        }
    }
}