using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ConfigureGoalWorkerCountTest
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(null)]
        public void ShouldReadGoalWorkerCount(int? workers)
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration
            {
                GoalWorkerCount = workers
            });
            var configuration = new Configuration(repository);

            Assert.Equal(workers, configuration.ReadGoalWorkerCount());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void ShouldWriteGoalWorkerCount(int workers)
        {
            var repository = new FakeConfigurationRepository();
            var configuration = new Configuration(repository);

            configuration.WriteGoalWorkerCount(workers);

            Assert.Equal(workers, repository.Workers);
        }

        [Fact]
        public void ShouldWriteNullableGoalWorkerCount()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration
            {
                GoalWorkerCount = 1
            });
            var configuration = new Configuration(repository);

            configuration.WriteGoalWorkerCount(null);

            Assert.Null(repository.Workers);
        }

    }
}