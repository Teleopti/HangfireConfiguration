using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ReadConfigurationsTest
    {
        [Fact]
        public void ShouldRead()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                Id = 13,
                ConnectionString = "connection",
                SchemaName = "schema",
                Active = true,
                GoalWorkerCount = 11
            });

            var result = system.ConfigurationApi.ReadConfigurations() as StoredConfiguration[];

            Assert.Equal(13, result.Single().Id);
            Assert.Equal("connection", result.Single().ConnectionString);
            Assert.Equal("schema", result.Single().SchemaName);
            Assert.Equal(true, result.Single().Active);
            Assert.Equal(11, result.Single().GoalWorkerCount);
        }
        
        [Fact]
        public void ShouldWrite()
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi.WriteConfigurations(new StoredConfiguration
            {
                Id = 22,
                ConnectionString = "connection",
                SchemaName = "SchemaName",
                Active = true,
                GoalWorkerCount = 44
            });

            var result = system.ConfigurationApi.ReadConfigurations().Single();
            Assert.Equal(22, result.Id);
            Assert.Equal("connection", result.ConnectionString);
            Assert.Equal("SchemaName", result.SchemaName);
            Assert.True(result.Active);
            Assert.Equal(44, result.GoalWorkerCount);
        }
    }
}