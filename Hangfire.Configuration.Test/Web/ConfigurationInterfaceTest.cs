using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire.Configuration.Test.Domain.Fake;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Web
{
	[Parallelizable(ParallelScope.None)]
	public class ConfigurationInterfaceTest
	{
		[Test]
		public async Task ShouldFindConfigurationInterface()
		{
			using var s = new WebServerUnderTest(new SystemUnderTest(), "/config");
			var response = await s.TestClient.GetAsync("/config");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public async Task ShouldNotFindConfigurationInterface()
		{
			using var s = new WebServerUnderTest(new SystemUnderTest(), "/config");
			var response = await s.TestClient.GetAsync("/configIncorrect");
			Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Test]
		public async Task ShouldSaveWorkerGoalCount()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 1,
				GoalWorkerCount = 3
			});

			using var s = new WebServerUnderTest(system);
			var response = await s.TestClient.PostAsync(
				"/config/saveWorkerGoalCount",
				new StringContent(JsonConvert.SerializeObject(new
				{
					configurationId = 1,
					workers = 10
				})));

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual(1, system.ConfigurationStorage.Data.Single().Id);
			Assert.AreEqual(10, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
		}

		[Test]
		public async Task ShouldReturn500WithErrorMessageWhenSaveTooManyWorkerGoalCount()
		{
			var system = new SystemUnderTest();
			system.UseOptions(new ConfigurationOptionsForTest {MaximumGoalWorkerCount = 10});
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 1,
				GoalWorkerCount = 3
			});

			using var s = new WebServerUnderTest(system);
			var response = await s.TestClient.PostAsync(
				"/config/saveWorkerGoalCount",
				new StringContent(JsonConvert.SerializeObject(new
				{
					configurationId = 1,
					workers = 11
				})));

			Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
			var message = await response.Content.ReadAsStringAsync();
			message.Should().Not.Be.Empty();
			message.Should().Not.Contain("<");
		}

		[Test]
		public async Task ShouldSaveWorkerGoalCountWithEmptyDatabase()
		{
			var system = new SystemUnderTest();

			using var s = new WebServerUnderTest(system);
			await s.TestClient.PostAsync(
				"/config/saveWorkerGoalCount",
				new StringContent(JsonConvert.SerializeObject(new
				{
					workers = 10
				})));

			Assert.AreEqual(1, system.ConfigurationStorage.Data.Single().Id);
			Assert.AreEqual(10, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
		}

		[Test]
		public async Task ShouldActivateServer()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 2
			});

			using var s = new WebServerUnderTest(system);
			await s.TestClient.PostAsync(
				"/config/activateServer",
				new StringContent(JsonConvert.SerializeObject(new
				{
					configurationId = 2
				})));

			Assert.True(system.ConfigurationStorage.Data.Single().Active);
		}

		[Test]
		public async Task ShouldCreateNewServerConfiguration()
		{
			var system = new SystemUnderTest();

			using var s = new WebServerUnderTest(system);
			await s.TestClient.PostAsync(
				"/config/createNewServerConfiguration",
				new StringContent(JsonConvert.SerializeObject(
					new
					{
						server = ".",
						database = "database",
						user = "user",
						password = "password",
						schemaName = "TestSchema",
						schemaCreatorUser = "schemaCreatorUser",
						schemaCreatorPassword = "schemaCreatorPassword"
					})));

			var storedConfiguration = system.ConfigurationStorage.Data.Single();
			Assert.AreEqual(1, storedConfiguration.Id);
			storedConfiguration.ConnectionString.Should().Contain("Data Source=.;Initial Catalog=database");
			storedConfiguration.SchemaName.Should().Be("TestSchema");
		}

		[Test]
		public async Task ShouldCreateNewServerConfigurationWithName()
		{
			var system = new SystemUnderTest();

			using var s = new WebServerUnderTest(system);
			await s.TestClient.PostAsync(
				"/config/createNewServerConfiguration",
				new StringContent(JsonConvert.SerializeObject(
					new
					{
						server = ".",
						name = "name",
						database = "database",
						user = "user",
						password = "password",
						schemaName = "TestSchema",
						schemaCreatorUser = "schemaCreatorUser",
						schemaCreatorPassword = "schemaCreatorPassword"
					})));

			Assert.AreEqual("name", system.ConfigurationStorage.Data.Single().Name);
		}

		[Test]
		public async Task ShouldNotFindUnknownAction()
		{
			using var s = new WebServerUnderTest(new SystemUnderTest(), "/config");
			var response = await s.TestClient.GetAsync("/config/unknownAction");
			Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Test]
		public async Task ShouldInactivateServer()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 17,
				Active = true
			});
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 3,
				Active = true
			});

			using var s = new WebServerUnderTest(system);
			await s.TestClient.PostAsync(
				"/config/inactivateServer",
				new StringContent(JsonConvert.SerializeObject(new
				{
					configurationId = 3
				})));

			Assert.False(system.ConfigurationStorage.Data.Single(x => x.Id == 3).Active);
		}
		
		[Test]
		public async Task ShouldCreateNewServerConfigurationForPostgres()
		{
			var system = new SystemUnderTest();

			using var s = new WebServerUnderTest(system);
			await s.TestClient.PostAsync(
				"/config/createNewServerConfiguration",
				new StringContent(JsonConvert.SerializeObject(
					new
					{
						server = "localhost",
						database = "database",
						user = "user",
						password = "password",
						schemaName = "TestSchema",
						schemaCreatorUser = "schemaCreatorUser",
						schemaCreatorPassword = "schemaCreatorPassword",
						databaseProvider = "PostgreSql"
					})));

			var storedConfiguration = system.ConfigurationStorage.Data.Single();
			Assert.AreEqual(1, storedConfiguration.Id);
			storedConfiguration.ConnectionString.Should().Contain("Host=localhost;Database=database");
			storedConfiguration.SchemaName.Should().Be("TestSchema");
		}
		
#if Redis
		[Test]
		public async Task ShouldCreateNewServerConfigurationForRedis()
		{
			var system = new SystemUnderTest();

			using var s = new WebServerUnderTest(system);
			await s.TestClient.PostAsync(
				"/config/createNewServerConfiguration",
				new StringContent(JsonConvert.SerializeObject(
					new
					{
						server = "gurka",
						schemaName = "gurka:",
						databaseProvider = "redis"
					})));

			var storedConfiguration = system.ConfigurationStorage.Data.Single();
			Assert.AreEqual(1, storedConfiguration.Id);
			storedConfiguration.ConnectionString.Should().Be("gurka");
			storedConfiguration.SchemaName.Should().Be("gurka:");
		}
#endif
		
	}
}