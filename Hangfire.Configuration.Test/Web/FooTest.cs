﻿using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Xunit;

namespace Hangfire.Configuration.Test.Web
{
	public class FooTest
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
			TestLog.WriteLine("ShouldSaveEmpty/1");

			var system = new SystemUnderTest();
			system.ConfigurationStorage.Has(new StoredConfiguration
			{
				Id = 1,
				MaxWorkersPerServer = 4
			});
			
			TestLog.WriteLine("ShouldSaveEmpty/2");

			using (var s = new ServerUnderTest(system, null, "ShouldSaveEmpty"))
			{
				TestLog.WriteLine("ShouldSaveEmpty/3");

				var response = s.TestClient.PostAsync(
						"/config/saveMaxWorkersPerServer",
						new StringContent(JsonConvert.SerializeObject(new
						{
							configurationId = 1,
							maxWorkers = ""
						})))
					.Result;

				TestLog.WriteLine("ShouldSaveEmpty/4");

				Assert.Null(system.ConfigurationStorage.Data.Single().MaxWorkersPerServer);
			}
		}
	}
}