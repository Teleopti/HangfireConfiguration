using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Xunit;

namespace Hangfire.Configuration.Test.Web
{
	public class MaxWorkersPerServerTest
	{
		[Fact]
		public void ShouldSave()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 1,
			});

			using (var s = new ServerUnderTest(system))
			{
				var response = s.TestClient.PostAsync(
						"/config/saveMaxWorkersPerServer",
						new StringContent(JsonConvert.SerializeObject(new
						{
							configurationId = 1,
							maxWorkers = 5
						})))
					.Result;

				Assert.Equal(5, system.ConfigurationStorage.Data.Single().MaxWorkersPerServer);
			}
		}

		[Fact]
		public void ShouldSaveEmpty()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 1,
				MaxWorkersPerServer = 4
			});

			using (var s = new ServerUnderTest(system, null, "ShouldSaveEmpty"))
			{
				var response = s.TestClient.PostAsync(
						"/config/saveMaxWorkersPerServer",
						new StringContent(JsonConvert.SerializeObject(new
						{
							configurationId = 1,
							maxWorkers = ""
						})))
					.Result;

				Assert.Null(system.ConfigurationStorage.Data.Single().MaxWorkersPerServer);
			}
		}
	}
}