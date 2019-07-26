using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    // delete?
    public class ReadActiveConfigurationTest
    {
        [Fact]
        public void ShouldReadEmptyActiveConnectionString()
        {
            var system = new SystemUnderTest();

            var connectionString = system.Configuration.ReadActiveConfigurationConnectionString();

            Assert.Null(connectionString);
        }

        [Fact]
        public void ShouldReadActiveConnectionString()
        {
            var system = new SystemUnderTest();
            
            system.Repository.Has(new StoredConfiguration
            {
                ConnectionString = "connectionString",
                Active = true
            });

            var connectionString = system.Configuration.ReadActiveConfigurationConnectionString();

            Assert.Equal("connectionString", connectionString);
        }

    }
}