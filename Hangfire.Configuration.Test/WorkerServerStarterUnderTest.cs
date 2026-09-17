using System;
using Hangfire.Server;

namespace Hangfire.Configuration.Test;

public class BackgroundJobServerStarterUnderTest(BackgroundJobServerStarter instance, Options options)
{
	public IDisposable Start() => 
		instance.Start();

	public IDisposable Start(IBackgroundProcess additionalProcess) => 
		instance.Start(additionalProcess.AsArray());

	public IDisposable Start(IBackgroundProcess[] additionalProcesses) => 
		instance.Start(additionalProcesses);

	public IDisposable Start(ConfigurationOptions options1)
	{
		if (string.IsNullOrEmpty(options1.ConnectionString))
			options1.ConnectionString = new ConfigurationOptionsForTest().ConnectionString;
		options.UseOptions(options1);
		return instance.Start();
	}
}
