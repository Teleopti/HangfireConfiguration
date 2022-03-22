using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
    public class ActivateMultipleConfigurationTest
    {
        [Test]
        public void ShouldActivateMultipleConfigurations()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 1,
                Active = false
            });
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 2,
                Active = false
            });

            system.ConfigurationApi.ActivateServer(1);
            system.ConfigurationApi.ActivateServer(2);

            var configurations = system.ConfigurationStorage.Data;
            Assert.AreEqual(true, configurations.ElementAt(0).Active);
            Assert.AreEqual(true, configurations.ElementAt(1).Active);
        }

        [Test]
        public void ShouldActivateConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 1,
                Active = false
            });
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 2,
                Active = false
            });

            system.ConfigurationApi.ActivateServer(2);

            var configurations = system.ConfigurationStorage.Data;
            Assert.AreEqual(false, configurations.Single(x => x.Id == 1).Active);
            Assert.AreEqual(true, configurations.Single(x => x.Id == 2).Active);
        }

        [Test]
        public void ShouldInactivateConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 1,
                Active = true
            });

            system.ConfigurationApi.InactivateServer(1);

            var configuration = system.ConfigurationStorage.Data.Single();
            Assert.AreEqual(false, configuration.Active);
        }

        [Test]
        public void ShouldInactivateGivenConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 1,
                Active = true
            });
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 2,
                Active = true
            });

            system.ConfigurationApi.InactivateServer(2);

            var configurations = system.ConfigurationStorage.Data;
            Assert.AreEqual(true, configurations.Single(x => x.Id == 1).Active);
            Assert.AreEqual(false, configurations.Single(x => x.Id == 2).Active);
        }
    }
}