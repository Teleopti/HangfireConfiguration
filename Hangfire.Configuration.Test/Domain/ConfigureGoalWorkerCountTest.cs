using System;
using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ConfigureGoalWorkerCountTest
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void ShouldWriteGoalWorkerCount(int workers)
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi.WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = workers});

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

            system.ConfigurationApi.WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = null});

            Assert.Null(system.Repository.Workers);
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

            system.ConfigurationApi.WriteGoalWorkerCount(new WriteGoalWorkerCount
            {
                ConfigurationId = 2,
                Workers = 5
            });

            Assert.Equal(5, system.Repository.Data.Single(x => x.Id == 2).GoalWorkerCount);
        }

        [Fact]
        public void ShouldThrowIfGoalWorkerCountHigherThan100()
        {
            var system = new SystemUnderTest();

            var e = Assert.Throws<Exception>(() => system.ConfigurationApi.WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = 101}));
            Assert.Equal("Invalid goal worker count.", e.Message);
        }

        [Fact]
        public void ShouldNotWriteIfGoalWorkerCountHigherThan100()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                Id = 1,
                GoalWorkerCount = 10
            });

            try
            {
                system.ConfigurationApi.WriteGoalWorkerCount(new WriteGoalWorkerCount {ConfigurationId = 1, Workers = 101});
            }
            catch (Exception e)
            {
            }

            Assert.Equal(10, system.Repository.Data.Single().GoalWorkerCount);
        }

        [Fact]
        public void ShouldThrowIfGoalWorkerCountHigherThanOptions()
        {
            var system = new SystemUnderTest(new ConfigurationOptions {MaximumGoalWorkerCount = 5});

            Assert.Throws<Exception>(() => system.ConfigurationApi.WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = 6}));
        }
    }
}