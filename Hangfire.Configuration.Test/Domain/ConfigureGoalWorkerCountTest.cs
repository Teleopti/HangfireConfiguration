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
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                GoalWorkerCount = workers
            });

            Assert.Equal(workers, system.Configuration.ReadGoalWorkerCount());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void ShouldWriteGoalWorkerCount(int workers)
        {
            var system = new SystemUnderTest();
            
            system.Configuration.WriteGoalWorkerCount(workers);

            Assert.Equal(workers, system.Repository.Workers);
        }

        [Fact]
        public void ShouldWriteNullableGoalWorkerCount()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                GoalWorkerCount = 1
            });

            system.Configuration.WriteGoalWorkerCount(null);

            Assert.Null(system.Repository.Workers);
        }

    }
}