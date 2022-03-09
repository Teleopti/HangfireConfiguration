using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Web
{
	[Parallelizable(ParallelScope.None)]
	public class MaxWorkersPerServerTest
	{
		[Test]
		public void ShouldSave()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 1,
			});

			using (var s = new ServerUnderTest(system))
			{
				_ = s.TestClient.PostAsync(
						"/config/saveMaxWorkersPerServer",
						new StringContent(JsonConvert.SerializeObject(new
						{
							configurationId = 1,
							maxWorkers = 5
						})))
					.Result;

				Assert.AreEqual(5, system.ConfigurationStorage.Data.Single().MaxWorkersPerServer);
			}
		}

		[Test]
		public void ShouldSaveEmpty()
		{
			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 1,
				MaxWorkersPerServer = 4
			});

			using (var s = new ServerUnderTest(system))
			{
				_ = s.TestClient.PostAsync(
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