using System.Linq;
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

            system.Configuration.WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = workers});

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

            system.Configuration.WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = null});

            Assert.Null(system.Repository.Workers);
        }

        [Fact]
        public void ShouldReadGoalWorkerCountWithId()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                Id = 1,
                GoalWorkerCount = 8
            }, new StoredConfiguration
            {
                Id = 2,
                GoalWorkerCount = 40
            });

            Assert.Equal(40, system.Configuration.ReadGoalWorkerCount(2));
        }

        [Fact]
        public void ShouldReadNullWhenNoConfiguration()
        {
            var system = new SystemUnderTest();

            Assert.Null(system.Configuration.ReadGoalWorkerCount());
            Assert.Null(system.Configuration.ReadGoalWorkerCount(1));
        }

        [Fact]
        public void ShouldWriteGoalWorkerCountForSpecificConfiguration()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                Id = 1
            }, new StoredConfiguration
            {
                Id = 2
            });

            system.Configuration.WriteGoalWorkerCount(new WriteGoalWorkerCount
            {
                ConfigurationId = 2,
                Workers = 5
            });

            Assert.Equal(5, system.Repository.Data.Single(x => x.Id == 2).GoalWorkerCount);
        }
    }
}