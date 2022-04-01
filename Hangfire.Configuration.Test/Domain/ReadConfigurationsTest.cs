using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
    public class ReadConfigurationsTest
    {
        [Test]
        public void ShouldRead()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 13,
                ConnectionString = "connection",
                SchemaName = "schema",
                Active = true,
                GoalWorkerCount = 11
            });

            var result = system.ConfigurationApi().ReadConfigurations() as StoredConfiguration[];

            Assert.AreEqual(13, result.Single().Id);
            Assert.AreEqual("connection", result.Single().ConnectionString);
            Assert.AreEqual("schema", result.Single().SchemaName);
            Assert.AreEqual(true, result.Single().Active);
            Assert.AreEqual(11, result.Single().GoalWorkerCount);
        }
        
        [Test]
        public void ShouldWrite()
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi().WriteConfiguration(new StoredConfiguration
            {
                Id = 22,
                ConnectionString = "connection",
                SchemaName = "SchemaName",
                Active = true,
                GoalWorkerCount = 44
            });

            var result = system.ConfigurationApi().ReadConfigurations().Single();
            Assert.AreEqual(22, result.Id);
            Assert.AreEqual("connection", result.ConnectionString);
            Assert.AreEqual("SchemaName", result.SchemaName);
            Assert.True(result.Active);
            Assert.AreEqual(44, result.GoalWorkerCount);
        }
    }
}