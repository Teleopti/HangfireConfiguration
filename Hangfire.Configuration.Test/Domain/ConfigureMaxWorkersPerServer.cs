using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
    public class ConfigureMaxWorkersPerServerTest
    {
        [Test]
        [TestCase(5)]
        [TestCase(6)]
        public void ShouldWriteMaxWorkersPerServer(int expected)
        {
            var system = new SystemUnderTest();

            system.ConfigurationApi().WriteMaxWorkersPerServer(new WriteMaxWorkersPerServer {MaxWorkers = expected});

	        Assert.AreEqual(expected, system.ConfigurationStorage.MaxWorkersPerServer);
        }
        
        [Test]
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

	        system.ConfigurationApi().WriteMaxWorkersPerServer(new WriteMaxWorkersPerServer
	        {
		        ConfigurationId = 2,
		        MaxWorkers = 7
	        });

	        Assert.AreEqual(7, system.ConfigurationStorage.Data.Single(x => x.Id == 2).MaxWorkersPerServer);
        }
    }
}