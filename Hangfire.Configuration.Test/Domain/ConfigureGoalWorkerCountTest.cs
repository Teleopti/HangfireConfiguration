using System;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
    public class ConfigureGoalWorkerCountTest
    {
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public void ShouldWriteGoalWorkerCount(int workers)
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi().WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = workers});

            Assert.AreEqual(workers, system.Configurations().Single().GoalWorkerCount);
        }

        [Test]
        public void ShouldWriteNullableGoalWorkerCount()
        {
            var system = new SystemUnderTest();
            system.WithConfiguration(new StoredConfiguration
            {
                GoalWorkerCount = 1
            });

            system.ConfigurationApi().WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = null});

            Assert.Null(system.Configurations().Single().GoalWorkerCount);
        }

        [Test]
        public void ShouldWriteGoalWorkerCountForSpecificConfiguration()
        {
            var system = new SystemUnderTest();
            system.WithConfiguration(new StoredConfiguration {Id = 1});
            system.WithConfiguration(new StoredConfiguration {Id = 2});

            system.ConfigurationApi().WriteGoalWorkerCount(new WriteGoalWorkerCount
            {
                ConfigurationId = 2,
                Workers = 5
            });

            Assert.AreEqual(5, system.Configurations().Single(x => x.Id == 2).GoalWorkerCount);
        }

        [Test]
        public void ShouldThrowIfGoalWorkerCountHigherThan100()
        {
            var system = new SystemUnderTest();

            var e = Assert.Throws<Exception>(() => system.ConfigurationApi().WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = 101}));
            Assert.AreEqual("Invalid goal worker count.", e.Message);
        }

        [Test]
        public void ShouldNotWriteIfGoalWorkerCountHigherThan100()
        {
            var system = new SystemUnderTest();
            system.WithConfiguration(new StoredConfiguration
            {
                Id = 1,
                GoalWorkerCount = 10
            });

            try
            {
                system.ConfigurationApi().WriteGoalWorkerCount(new WriteGoalWorkerCount {ConfigurationId = 1, Workers = 101});
            }
            catch (Exception)
            {
            }

            Assert.AreEqual(10, system.Configurations().Single().GoalWorkerCount);
        }

        [Test]
        public void ShouldThrowIfGoalWorkerCountHigherThanOptions()
        {
            var system = new SystemUnderTest();
            system.UseOptions(new ConfigurationOptionsForTest {MaximumGoalWorkerCount = 5});
            Assert.Throws<Exception>(() => system.ConfigurationApi().WriteGoalWorkerCount(new WriteGoalWorkerCount {Workers = 6}));
        }
    }
}