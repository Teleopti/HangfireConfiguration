using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
    public class ActivateServerConfigurationTest
    {
        [Test]
        public void ShouldBeInactiveWhenCreated()
        {
            var system = new SystemUnderTest();
            
            system.ConfigurationApi.CreateServerConfiguration(new CreateSqlServerWorkerServer
            {
                Server = "AwesomeServer",
                Database = "TestDatabase",
                User = "testUser",
                Password = "awesomePassword",
                SchemaCreatorUser = "createUser",
                SchemaCreatorPassword = "createPassword",
                SchemaName = "awesomeSchema"
            });

            var storedConfiguration = system.ConfigurationStorage.Data.Last();
            Assert.AreEqual(false, storedConfiguration.Active);
        }

        [Test]
        public void ShouldActivate()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 1,
                ConnectionString = "connectionString",
                SchemaName = "awesomeSchema",
                Active = false
            });

            system.ConfigurationApi.ActivateServer(1);

            var storedConfiguration = system.ConfigurationStorage.Data.Single();
            Assert.AreEqual(true, storedConfiguration.Active);
        }

        [Test]
        public void ShouldDeactivatePreviouslyActive()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(
                new StoredConfiguration {Id = 1, Active = true, ConnectionString = "connectionString", SchemaName = "awesomeSchema"},
                new StoredConfiguration {Id = 2, ConnectionString = "connectionString2", SchemaName = "awesomeSchema2"}
            );

            system.ConfigurationApi.ActivateServer(2);

            Assert.AreEqual(false, system.ConfigurationStorage.Data.Single(x => x.Id == 1).Active);
        }
    }
}