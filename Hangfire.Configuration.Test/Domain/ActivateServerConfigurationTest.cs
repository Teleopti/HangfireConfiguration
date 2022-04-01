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

			system.ConfigurationApi().CreateServerConfiguration(new CreateSqlServerWorkerServer
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

			system.ConfigurationApi().ActivateServer(1);

			var storedConfiguration = system.ConfigurationStorage.Data.Single();
			Assert.AreEqual(true, storedConfiguration.Active);
		}
	}
}