using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire.Configuration.Test.Domain.Fake;
using Newtonsoft.Json;
using Xunit;

namespace Hangfire.Configuration.Test.Web
{
	[Collection("NotParallel")]
	public class ConfigurationInterfaceTest
	{
		[Fact]
		public async Task ShouldFindConfigurationInterface()
		{
			using (var s = new ServerUnderTest(new SystemUnderTest(), "/config"))
			{
				var response = await s.TestClient.GetAsync("/config");
				Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			}
		}

		[Fact]
		public async Task ShouldNotFindConfigurationInterface()
		{
			using (var s = new ServerUnderTest(new SystemUnderTest(), "/config"))
			{
				var response = await s.TestClient.GetAsync("/configIncorrect");
				Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			}
		}

		[Fact]
		public async Task ShouldSaveWorkerGoalCount()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 1,
				GoalWorkerCount = 3
			});

			using (var s = new ServerUnderTest(system))
			{
				var response = await s.TestClient.PostAsync(
						"/config/saveWorkerGoalCount",
						new StringContent(JsonConvert.SerializeObject(new
						{
							configurationId = 1,
							workers = 10
						})));

				Assert.Equal(HttpStatusCode.OK, response.StatusCode);
				Assert.Equal(1, system.ConfigurationStorage.Data.Single().Id);
				Assert.Equal(10, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
			}
		}

		[Fact]
		public async Task ShouldReturn500WithErrorMessageWhenSaveTooManyWorkerGoalCount()
		{
			var system = new SystemUnderTest();
			system.Options.UseOptions(new ConfigurationOptionsForTest {MaximumGoalWorkerCount = 10});
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 1,
				GoalWorkerCount = 3
			});

			using (var s = new ServerUnderTest(system))
			{
				var response = await s.TestClient.PostAsync(
						"/config/saveWorkerGoalCount",
						new StringContent(JsonConvert.SerializeObject(new
						{
							configurationId = 1,
							workers = 11
						})));

				Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
				var message = await response.Content.ReadAsStringAsync();
				Assert.NotEmpty(message);
				Assert.DoesNotContain("<", message);
			}
		}

		[Fact]
		public async Task ShouldSaveWorkerGoalCountWithEmptyDatabase()
		{
			var system = new SystemUnderTest();

			using (var s = new ServerUnderTest(system))
			{
				await s.TestClient.PostAsync(
						"/config/saveWorkerGoalCount",
						new StringContent(JsonConvert.SerializeObject(new
						{
							workers = 10
						})));

				Assert.Equal(1, system.ConfigurationStorage.Data.Single().Id);
				Assert.Equal(10, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
			}
		}

		[Fact]
		public async Task ShouldActivateServer()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 2
			});

			using (var s = new ServerUnderTest(system))
			{
				await s.TestClient.PostAsync(
						"/config/activateServer",
						new StringContent(JsonConvert.SerializeObject(new
						{
							configurationId = 2
						})));

				Assert.True(system.ConfigurationStorage.Data.Single().Active);
			}
		}

		[Fact]
		public async Task ShouldCreateNewServerConfiguration()
		{
			var system = new SystemUnderTest();

			using (var s = new ServerUnderTest(system))
			{
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

				Assert.Equal(1, system.ConfigurationStorage.Data.Single().Id);
				Assert.Contains("database", system.ConfigurationStorage.Data.Single().ConnectionString);
			}
		}

		[Fact]
		public async Task ShouldCreateNewServerConfigurationWithName()
		{
			var system = new SystemUnderTest();

			using (var s = new ServerUnderTest(system))
			{
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

				Assert.Equal("name", system.ConfigurationStorage.Data.Single().Name);
			}
		}

		[Fact]
		public async Task ShouldNotFindUnknownAction()
		{
			using (var s = new ServerUnderTest(new SystemUnderTest(), "/config"))
			{
				var response = await s.TestClient.GetAsync("/config/unknownAction");
				Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			}
		}

		[Fact]
		public async Task ShouldInactivateServer()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 3,
				Active = true
			});

			using (var s = new ServerUnderTest(system, null))
			{
				await s.TestClient.PostAsync(
						"/config/inactivateServer",
						new StringContent(JsonConvert.SerializeObject(new
						{
							configurationId = 3
						})));

				Assert.False(system.ConfigurationStorage.Data.Single().Active);
			}
		}
	}
}