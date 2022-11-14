using System;
using Hangfire.Configuration.Test.Infrastructure;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Integration;

[Parallelizable(ParallelScope.None)]
public class IntegrationTest : DatabaseTest
{
	public IntegrationTest(string connectionString) : base(connectionString)
	{
	}

	[Test]
	public void ShouldStartServerWithWorkers()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = ConnectionString,
			UpdateConfigurations = new[]
			{
				new UpdateStorageConfiguration
				{
					ConnectionString = ConnectionString,
					Name = DefaultConfigurationName.Name()
				}
			}
		});
		system.StartWorkerServers();
	}
	
	[Test]
	public void ShouldGetPublisherWithoutConfigurationConnection()
	{
		var system = new SystemUnderInfraTest();
		system.UseRealHangfire();
		system.UseOptions(new ConfigurationOptions {ConnectionString = null});

		var result = system.GetPublisher(ConnectionString, "Hangfire");

		result.BackgroundJobClient.Enqueue(() => Console.WriteLine("test"));
	}
}