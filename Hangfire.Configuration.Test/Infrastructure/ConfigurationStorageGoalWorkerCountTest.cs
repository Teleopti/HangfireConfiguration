using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("NotParallel")]
    public class ConfigurationStorageGoalWorkerCountTest
    {
        [Fact, CleanDatabase]
        public void ShouldReadEmptyGoalWorkerCount()
        {
            var storage = new ConfigurationStorage(ConnectionUtils.GetConnectionString());

            Assert.Empty(storage.ReadConfigurations());
        }

        [Fact, CleanDatabase]
        public void ShouldWriteGoalWorkerCount()
        {
            var storage = new ConfigurationStorage(ConnectionUtils.GetConnectionString());

            storage.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

            Assert.Equal(1, storage.ReadConfigurations().Single().GoalWorkerCount);
        }

        [Fact, CleanDatabase]
        public void ShouldReadGoalWorkerCount()
        {
            var storage = new ConfigurationStorage(ConnectionUtils.GetConnectionString());
            storage.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

            var actual = storage.ReadConfigurations();
            
            Assert.Equal(1, actual.Single().GoalWorkerCount);
        }

        [Fact, CleanDatabase]
        public void ShouldWriteNullGoalWorkerCount()
        {
            var storage = new ConfigurationStorage(ConnectionUtils.GetConnectionString());
            storage.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

            var configuration = storage.ReadConfigurations().Single();
            configuration.GoalWorkerCount = null;
            storage.WriteConfiguration(configuration);

            Assert.Null(storage.ReadConfigurations().Single().GoalWorkerCount);
        }
    }
}