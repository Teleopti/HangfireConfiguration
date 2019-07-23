using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("Infrastructure")]
    public class ConfigurationRepositoryGoalWorkerCountTest
    {
        [Fact, CleanDatabase]
        public void ShouldReadEmptyGoalWorkerCount()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());

            Assert.Empty(repository.ReadConfigurations());
        }

        [Fact, CleanDatabase]
        public void ShouldWriteGoalWorkerCount()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());

            repository.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

            Assert.Equal(1, repository.ReadConfigurations().Single().GoalWorkerCount);
        }

        [Fact, CleanDatabase]
        public void ShouldReadGoalWorkerCount()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

            var actual = repository.ReadConfigurations();
            
            Assert.Equal(1, actual.Single().GoalWorkerCount);
        }

        [Fact, CleanDatabase]
        public void ShouldWriteNullGoalWorkerCount()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteConfiguration(new StoredConfiguration {GoalWorkerCount = 1});

            var configuration = repository.ReadConfigurations().Single();
            configuration.GoalWorkerCount = null;
            repository.WriteConfiguration(configuration);

            Assert.Null(repository.ReadConfigurations().Single().GoalWorkerCount);
        }
    }
}