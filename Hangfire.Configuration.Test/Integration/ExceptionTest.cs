using System;
using Hangfire.Configuration.Test.Infrastructure;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Integration;

[Parallelizable(ParallelScope.None)]
public class ExceptionTest(string connectionString) : 
	DatabaseTest(connectionString)
{
	[Test]
	public void ShouldThrowWhenBadConnectionString()
	{
		var system = new SystemUnderInfraTest();
		system.UseOptions(new ConfigurationOptions
		{
			ConnectionString = "$(invalid)"
		});
		var exception = Assert.Catch(() =>
		{
			using var _ = system.StartBackgroundJobServers();
		});
		
		exception.Message.Should().Contain("Invalid connection string");
	}
}