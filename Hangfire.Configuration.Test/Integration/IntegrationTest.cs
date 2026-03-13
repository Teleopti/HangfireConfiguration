using System;
using Hangfire.Configuration.Test.Infrastructure;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Integration;

[Parallelizable(ParallelScope.None)]
public class IntegrationTest(string connectionString) : 
	DatabaseTest(connectionString)
{
	[Test]
	public void ShouldStartServerWithWorkers()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = ConnectionString,
			ExternalConfigurations = new[]
			{
				new ExternalConfiguration
				{
					ConnectionString = ConnectionString,
					Name = DefaultConfigurationName.Name()
				}
			}
		});
		using var _ = system.StartWorkerServers();
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