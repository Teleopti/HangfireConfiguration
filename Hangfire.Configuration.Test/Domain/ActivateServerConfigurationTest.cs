using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ActivateServerConfigurationTest
    {
        [Fact]
        public void ShouldBeInactiveWhenCreated()
        {
            var system = new SystemUnderTest();
            
            system.ConfigurationApi.CreateServerConfiguration(new CreateServerConfiguration()
            {
                Server = "AwesomeServer",
                Database = "TestDatabase",
                User = "testUser",
                Password = "awesomePassword",
                SchemaCreatorUser = "createUser",
                SchemaCreatorPassword = "createPassword",
                SchemaName = "awesomeSchema"
            });

            var storedConfiguration = system.ConfigurationRepository.Data.Last();
            Assert.Equal(false, storedConfiguration.Active);
        }

        [Fact]
        public void ShouldActivate()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.Has(new StoredConfiguration
            {
                Id = 1,
                ConnectionString = "connectionString",
                SchemaName = "awesomeSchema",
                Active = false
            });

            system.ConfigurationApi.ActivateServer(1);

            var storedConfiguration = system.ConfigurationRepository.Data.Single();
            Assert.Equal(true, storedConfiguration.Active);
        }

        [Fact]
        public void ShouldDeactivatePreviouslyActive()
        {
            var system = new SystemUnderTest();
            system.ConfigurationRepository.Has(
                new StoredConfiguration {Id = 1, Active = true, ConnectionString = "connectionString", SchemaName = "awesomeSchema"},
                new StoredConfiguration {Id = 2, ConnectionString = "connectionString2", SchemaName = "awesomeSchema2"}
            );

            system.ConfigurationApi.ActivateServer(2);

            Assert.Equal(false, system.ConfigurationRepository.Data.Single(x => x.Id == 1).Active);
        }
    }
}