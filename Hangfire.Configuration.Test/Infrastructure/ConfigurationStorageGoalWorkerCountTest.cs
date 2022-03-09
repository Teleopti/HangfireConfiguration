using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

public class ConfigurationStorageGoalWorkerCountTest : DatabaseTestBase
{
	public ConfigurationStorageGoalWorkerCountTest(string connectionString) : base(connectionString)
	{
	}

	[Test]
	public void ShouldReadEmptyGoalWorkerCount()
	{
		var storage = new ConfigurationStorage(ConnectionString);

		Assert.IsEmpty(storage.ReadConfigurations());
	}

	[Test]
	public void ShouldWriteGoalWorkerCount()
	{
		var storage = new ConfigurationStorage(ConnectionString);

		storage.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

		Assert.AreEqual(1, storage.ReadConfigurations().Single().GoalWorkerCount);
	}

	[Test]
	public void ShouldReadGoalWorkerCount()
	{
		var storage = new ConfigurationStorage(ConnectionString);
		storage.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

		var actual = storage.ReadConfigurations();

		Assert.AreEqual(1, actual.Single().GoalWorkerCount);
	}

	[Test]
	public void ShouldWriteNullGoalWorkerCount()
	{
		var storage = new ConfigurationStorage(ConnectionString);
		storage.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

		var configuration = storage.ReadConfigurations().Single();
		configuration.GoalWorkerCount = null;
		storage.WriteConfiguration(configuration);

		Assert.Null(storage.ReadConfigurations().Single().GoalWorkerCount);
	}
}