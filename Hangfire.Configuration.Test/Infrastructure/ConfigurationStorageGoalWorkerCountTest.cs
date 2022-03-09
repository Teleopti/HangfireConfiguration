using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Parallelizable(ParallelScope.None)]
    [CleanDatabase]
    [CleanDatabasePostgres]
    [TestFixture(ConnectionUtils.DefaultConnectionStringTemplate)]
    [TestFixture(ConnectionUtilsPostgres.DefaultConnectionStringTemplate)]
    public class ConfigurationStorageGoalWorkerCountTest
    {
	    private readonly string _connectionString;

	    public ConfigurationStorageGoalWorkerCountTest(string connectionString)
	    {
		    _connectionString = connectionString;
	    }
	    
        [Test]
        public void ShouldReadEmptyGoalWorkerCount()
        {
            var storage = new ConfigurationStorage(_connectionString);

            Assert.IsEmpty(storage.ReadConfigurations());
        }

        [Test]
        public void ShouldWriteGoalWorkerCount()
        {
            var storage = new ConfigurationStorage(_connectionString);

            storage.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

            Assert.AreEqual(1, storage.ReadConfigurations().Single().GoalWorkerCount);
        }

        [Test]
        public void ShouldReadGoalWorkerCount()
        {
            var storage = new ConfigurationStorage(_connectionString);
            storage.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

            var actual = storage.ReadConfigurations();
            
            Assert.AreEqual(1, actual.Single().GoalWorkerCount);
        }

        [Test]
        public void ShouldWriteNullGoalWorkerCount()
        {
            var storage = new ConfigurationStorage(_connectionString);
            storage.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

            var configuration = storage.ReadConfigurations().Single();
            configuration.GoalWorkerCount = null;
            storage.WriteConfiguration(configuration);

            Assert.Null(storage.ReadConfigurations().Single().GoalWorkerCount);
        }
    }
}