﻿using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Xunit;

namespace Hangfire.Configuration.Test.Web
{
	[Collection("TryThis")]
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

			var response = system.TestClient.PostAsync(
					"/config/saveMaxWorkersPerServer",
					new StringContent(JsonConvert.SerializeObject(new
					{
						configurationId = 1,
						maxWorkers = 5
					})))
				.Result;

			Assert.Equal(5, system.ConfigurationStorage.Data.Single().MaxWorkersPerServer);
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

			var response = system.TestClient.PostAsync(
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