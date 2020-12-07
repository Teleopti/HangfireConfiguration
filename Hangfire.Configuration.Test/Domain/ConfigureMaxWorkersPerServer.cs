using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ConfigureMaxWorkersPerServerTest
    {
        [Theory]
        [InlineData(5)]
        [InlineData(6)]
        public void ShouldWriteMaxWorkersPerServer(int expected)
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi.WriteMaxWorkersPerServer(new MaxWorkersPerServer {MaxWorkers = expected});

	        Assert.Equal(expected, system.ConfigurationStorage.MaxWorkersPerServer);
        }
        
        [Fact]
        public void ShouldWriteMaxWorkersPerServerForSpecificConfiguration()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        Id = 1
	        }, new StoredConfiguration
	        {
		        Id = 2
	        });

	        system.ConfigurationApi.WriteMaxWorkersPerServer(new MaxWorkersPerServer
	        {
		        ConfigurationId = 2,
		        MaxWorkers = 7
	        });

	        Assert.Equal(7, system.ConfigurationStorage.Data.Single(x => x.Id == 2).MaxWorkersPerServer);
        }
    }
}