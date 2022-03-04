using System.Linq;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace Hangfire.Configuration.Test.Domain.Postgres
{
	public class DefaultConfigurationNameTest : XunitContextBase
	{
		public DefaultConfigurationNameTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public void ShouldUpdateLegacyWithDefaultName()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 3});

			var result = system.WorkerServerQueries.QueryAllWorkerServers();

			Assert.Equal(DefaultConfigurationName.Name(), result.Single().Name);
		}

		[Fact]
		public void ShouldNotUpdateNamed()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration {Name = "name"});

			var result = system.WorkerServerQueries.QueryAllWorkerServers();

			Assert.Equal("name", result.Single().Name);
		}

		[Fact]
		public void ShouldUpdateFirstLegacyWithDefaultName()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration {Id = 2, GoalWorkerCount = 3});
			system.ConfigurationStorage.Has(new StoredConfiguration {Id = 1, GoalWorkerCount = 1});

			var result = system.WorkerServerQueries.QueryAllWorkerServers();

			Assert.Equal(DefaultConfigurationName.Name(), result.Single(x => x.ConfigurationId == 1).Name);
		}

		[Fact]
		public void ShouldUpdateAutoUpdateMarkedWithDefaultName()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = new NpgsqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString()});

			var result = system.WorkerServerQueries.QueryAllWorkerServers();

			Assert.Equal(DefaultConfigurationName.Name(), result.Single().Name);
		}

		[Fact]
		public void ShouldUpdateLegacyOverAutoUpdateMarkedWithDefaultName()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 1,
				ConnectionString = new NpgsqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString()
			});
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 2,
				GoalWorkerCount = 3
			});

			var result = system.WorkerServerQueries.QueryAllWorkerServers();

			Assert.Equal(DefaultConfigurationName.Name(), result.Single(x => x.ConfigurationId == 2).Name);
			Assert.Null(result.Single(x => x.ConfigurationId == 1).Name);
		}
	}
}