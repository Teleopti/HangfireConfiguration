using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ActivateMultipleConfigurationTest
    {
        [Fact]
        public void ShouldActivateMultipleConfigurations()
        {
            var system = new SystemUnderTest(new ConfigurationOptions
            {
                AllowMultipleActive = true
            });
            system.ConfigurationRepository.Has(new StoredConfiguration
            {
                Id = 1,
                Active = false
            });
            system.ConfigurationRepository.Has(new StoredConfiguration
            {
                Id = 2,
                Active = false
            });

            system.ConfigurationApi.ActivateServer(1);
            system.ConfigurationApi.ActivateServer(2);

            var configurations = system.ConfigurationRepository.Data;
            Assert.Equal(true, configurations.ElementAt(0).Active);
            Assert.Equal(true, configurations.ElementAt(1).Active);
        }

        [Fact]
        public void ShouldActivateConfiguration()
        {
            var system = new SystemUnderTest(new ConfigurationOptions
            {
                AllowMultipleActive = true
            });
            system.ConfigurationRepository.Has(new StoredConfiguration
            {
                Id = 1,
                Active = false
            });
            system.ConfigurationRepository.Has(new StoredConfiguration
            {
                Id = 2,
                Active = false
            });

            system.ConfigurationApi.ActivateServer(2);

            var configurations = system.ConfigurationRepository.Data;
            Assert.Equal(false, configurations.Single(x => x.Id == 1).Active);
            Assert.Equal(true, configurations.Single(x => x.Id == 2).Active);
        }

        [Fact]
        public void ShouldInactivateConfiguration()
        {
            var system = new SystemUnderTest(new ConfigurationOptions
            {
                AllowMultipleActive = true
            });
            system.ConfigurationRepository.Has(new StoredConfiguration
            {
                Id = 1,
                Active = true
            });

            system.ConfigurationApi.InactivateServer(1);

            var configuration = system.ConfigurationRepository.Data.Single();
            Assert.Equal(false, configuration.Active);
        }


        [Fact]
        public void ShouldInactivateGivenConfiguration()
        {
            var system = new SystemUnderTest(new ConfigurationOptions
            {
                AllowMultipleActive = true
            });
            system.ConfigurationRepository.Has(new StoredConfiguration
            {
                Id = 1,
                Active = true
            });
            system.ConfigurationRepository.Has(new StoredConfiguration
            {
                Id = 2,
                Active = true
            });

            system.ConfigurationApi.InactivateServer(2);

            var configurations = system.ConfigurationRepository.Data;
            Assert.Equal(true, configurations.Single(x => x.Id == 1).Active);
            Assert.Equal(false, configurations.Single(x => x.Id == 2).Active);
        }
    }
}