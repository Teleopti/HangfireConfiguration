using System;
using System.Linq;
using Hangfire.PostgreSql;
using Hangfire.Pro.Redis;
using Hangfire.SqlServer;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class StorageOptionsTest
{
	[Test]
	public void ShouldApplyExternalStorageOptions()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "redis"
		});

		system.Options.UseOptions(new ConfigurationOptions
		{
			StorageOptionsFactory = new testFactory(() => new RedisStorageOptions {MultiplexerPoolSize = 42})
		});
		system.WorkerServerStarter.Start();

		var storage = system.Hangfire.StartedServers.Single().storage;
		storage.RedisOptions.MultiplexerPoolSize.Should().Be(42);
	}

	[Test]
	public void ShouldApplySqlServerOptions()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Data Source=srv;Initial Catalog=db"
		});

		system.Options.UseOptions(new ConfigurationOptions
		{
			StorageOptionsFactory = new testFactory(() => new SqlServerStorageOptions {QueuePollInterval = new TimeSpan(42)})
		});
		system.WorkerServerStarter.Start();

		var storage = system.Hangfire.StartedServers.Single().storage;
		storage.SqlServerOptions.QueuePollInterval.Should().Be(TimeSpan.FromTicks(42));
	}

	[Test]
	public void ShouldApplyPostgresOptions()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Host=local;Database=db"
		});

		system.Options.UseOptions(new ConfigurationOptions
		{
			StorageOptionsFactory = new testFactory(() => new PostgreSqlStorageOptions {DistributedLockTimeout = TimeSpan.FromTicks(42)})
		});
		system.WorkerServerStarter.Start();

		var storage = system.Hangfire.StartedServers.Single().storage;
		storage.PostgresOptions.DistributedLockTimeout.Should().Be(TimeSpan.FromTicks(42));
	}

	[Test]
	public void ShouldApplyWithDifferentSchemaNames()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "redis",
			SchemaName = "schema1"
		}, new StoredConfiguration
		{
			ConnectionString = "redis",
			SchemaName = "schema2"
		});

		var options = new RedisStorageOptions {MultiplexerPoolSize = 5};
		system.Options.UseOptions(new ConfigurationOptions
		{
			StorageOptionsFactory = new testFactory(() => options)
		});
		system.WorkerServerStarter.Start();

		var storage1 = system.Hangfire.StartedServers.First().storage;
		storage1.RedisOptions.Prefix.Should().Be("schema1");
		storage1.RedisOptions.MultiplexerPoolSize.Should().Be(5);
		var storage2 = system.Hangfire.StartedServers.Last().storage;
		storage2.RedisOptions.Prefix.Should().Be("schema2");
		storage2.RedisOptions.MultiplexerPoolSize.Should().Be(5);
	}

	[Test]
	public void ShouldApplySqlServerWithDifferentSchemaNames()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Data Source=.",
			SchemaName = "schema1"
		}, new StoredConfiguration
		{
			ConnectionString = "Data Source=.",
			SchemaName = "schema2"
		});

		var options = new SqlServerStorageOptions {CommandTimeout = TimeSpan.FromMinutes(1)};
		system.Options.UseOptions(new ConfigurationOptions
		{
			StorageOptionsFactory = new testFactory(() => options)
		});
		system.WorkerServerStarter.Start();

		var storage1 = system.Hangfire.StartedServers.First().storage;
		storage1.SqlServerOptions.SchemaName.Should().Be("schema1");
		storage1.SqlServerOptions.CommandTimeout.Should().Be(TimeSpan.FromMinutes(1));
		var storage2 = system.Hangfire.StartedServers.Last().storage;
		storage2.SqlServerOptions.SchemaName.Should().Be("schema2");
		storage2.SqlServerOptions.CommandTimeout.Should().Be(TimeSpan.FromMinutes(1));
	}

	[Test]
	public void ShouldApplyPostgresWithDifferentSchemaNames()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			ConnectionString = "Host=local",
			SchemaName = "schema1"
		}, new StoredConfiguration
		{
			ConnectionString = "Host=local",
			SchemaName = "schema2"
		});

		var options = new PostgreSqlStorageOptions() {DistributedLockTimeout = TimeSpan.FromMinutes(1)};
		system.Options.UseOptions(new ConfigurationOptions
		{
			StorageOptionsFactory = new testFactory(() => options)
		});
		system.WorkerServerStarter.Start();

		var storage1 = system.Hangfire.StartedServers.First().storage;
		storage1.PostgresOptions.SchemaName.Should().Be("schema1");
		storage1.PostgresOptions.DistributedLockTimeout.Should().Be(TimeSpan.FromMinutes(1));
		var storage2 = system.Hangfire.StartedServers.Last().storage;
		storage2.PostgresOptions.SchemaName.Should().Be("schema2");
		storage2.PostgresOptions.DistributedLockTimeout.Should().Be(TimeSpan.FromMinutes(1));
	}

	[Test]
	public void ShouldApplyForDifferentConfigurations()
	{
		var system = new SystemUnderTest();
		system.ConfigurationStorage.Has(new StoredConfiguration
		{
			Name = "one"
		}, new StoredConfiguration
		{
			Name = "two"
		});

		system.Options.UseOptions(new ConfigurationOptions
		{
			StorageOptionsFactory = new testFactory(c =>
			{
				return c.Name switch
				{
					"one" => new RedisStorageOptions {MultiplexerPoolSize = 1},
					"two" => new RedisStorageOptions {MultiplexerPoolSize = 2},
					_ => null
				};
			})
		});
		system.WorkerServerStarter.Start();

		var storage1 = system.Hangfire.StartedServers.First().storage;
		storage1.RedisOptions.MultiplexerPoolSize.Should().Be(1);
		var storage2 = system.Hangfire.StartedServers.Last().storage;
		storage2.RedisOptions.MultiplexerPoolSize.Should().Be(2);
	}

	private class testFactory : IStorageOptionsFactory
	{
		private readonly Func<StoredConfiguration, object> _factoryMethod;

		public testFactory(Func<StoredConfiguration, object> factoryMethod)
		{
			_factoryMethod = factoryMethod;
		}

		public testFactory(Func<object> factoryMethod)
		{
			_factoryMethod = _ => factoryMethod.Invoke();
		}

		public object Make(StoredConfiguration configuration) =>
			_factoryMethod.Invoke(configuration);
	}
}