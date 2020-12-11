using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Hangfire.Configuration.Test.Domain.Fake;
using Newtonsoft.Json;
using Xunit;

namespace Hangfire.Configuration.Test.Web
{
	public class ConfigurationInterfaceTest
	{
		[Fact]
		public void ShouldFindConfigurationInterface()
		{
			using (var s = new ServerUnderTest(new SystemUnderTest(), "/config"))
			{
				var response = s.TestClient.GetAsync("/config").Result;
				Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			}
		}

		[Fact]
		public void ShouldNotFindConfigurationInterface()
		{
			using (var s = new ServerUnderTest(new SystemUnderTest(), "/config"))
			{
				var response = s.TestClient.GetAsync("/configIncorrect").Result;
				Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			}
		}

		[Fact]
		public void ShouldSaveWorkerGoalCount()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 1,
				GoalWorkerCount = 3
			});

			using (var s = new ServerUnderTest(system))
			{
				var response = s.TestClient.PostAsync(
						"/config/saveWorkerGoalCount",
						new StringContent(JsonConvert.SerializeObject(new
						{
							configurationId = 1,
							workers = 10
						})))
					.Result;

				Assert.Equal(1, system.ConfigurationStorage.Data.Single().Id);
				Assert.Equal(10, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
			}
		}

		[Fact]
		public void ShouldReturn500WithErrorMessageWhenSaveTooManyWorkerGoalCount()
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
				var response = s.TestClient.PostAsync(
						"/config/saveWorkerGoalCount",
						new StringContent(JsonConvert.SerializeObject(new
						{
							configurationId = 1,
							workers = 11
						})))
					.Result;

				Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
				var message = response.Content.ReadAsStringAsync().Result;
				Assert.NotEmpty(message);
				Assert.DoesNotContain("<", message);
			}
		}

		[Fact]
		public void ShouldSaveWorkerGoalCountWithEmptyDatabase()
		{
			var system = new SystemUnderTest();

			using (var s = new ServerUnderTest(system))
			{
				var response = s.TestClient.PostAsync(
						"/config/saveWorkerGoalCount",
						new StringContent(JsonConvert.SerializeObject(new
						{
							workers = 10
						})))
					.Result;

				Assert.Equal(1, system.ConfigurationStorage.Data.Single().Id);
				Assert.Equal(10, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
			}
		}

		[Fact]
		public void ShouldActivateServer()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 2
			});

			using (var s = new ServerUnderTest(system))
			{
				var response = s.TestClient.PostAsync(
						"/config/activateServer",
						new StringContent(JsonConvert.SerializeObject(new
						{
							configurationId = 2
						})))
					.Result;

				Assert.True(system.ConfigurationStorage.Data.Single().Active);
			}
		}

		[Fact]
		public void ShouldCreateNewServerConfiguration()
		{
			var system = new SystemUnderTest();

			using (var s = new ServerUnderTest(system))
			{
				var response = s.TestClient.PostAsync(
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
							})))
					.Result;

				Assert.Equal(1, system.ConfigurationStorage.Data.Single().Id);
				Assert.Contains("database", system.ConfigurationStorage.Data.Single().ConnectionString);
			}
		}

		[Fact]
		public void ShouldCreateNewServerConfigurationWithName()
		{
			var system = new SystemUnderTest();

			using (var s = new ServerUnderTest(system))
			{
				var response = s.TestClient.PostAsync(
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
							})))
					.Result;

				Assert.Equal("name", system.ConfigurationStorage.Data.Single().Name);
			}
		}

		[Fact]
		public void ShouldNotFindUnknownAction()
		{
			using (var s = new ServerUnderTest(new SystemUnderTest(), "/config"))
			{
				var response = s.TestClient.GetAsync("/config/unknownAction").Result;
				Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			}
		}

		[Fact]
		public void ShouldInactivateServer()
		{
			var test = new ConcurrencyRunner();
			test.InParallel(() =>
			{

				TestLog.WriteLine("ShouldInactivateServer/1");
			
				var system = new SystemUnderTest();
				system.ConfigurationStorage.Has(new StoredConfiguration
				{
					Id = 3,
					Active = true
				});

				TestLog.WriteLine("ShouldInactivateServer/2");

				using (var s = new ServerUnderTest(system, null, "ShouldInactivateServer"))
				{
					TestLog.WriteLine("ShouldInactivateServer/3");
					var response = s.TestClient.PostAsync(
							"/config/inactivateServer",
							new StringContent(JsonConvert.SerializeObject(new
							{
								configurationId = 3
							})))
						.Result;

					TestLog.WriteLine("ShouldInactivateServer/4");
					Assert.False(system.ConfigurationStorage.Data.Single().Active);
				}
			});

			test.Wait(TimeSpan.FromSeconds(15));
		}
	}
}