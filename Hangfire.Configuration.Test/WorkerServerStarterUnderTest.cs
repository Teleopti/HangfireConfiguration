using Hangfire.Server;

namespace Hangfire.Configuration.Test;

public class WorkerServerStarterUnderTest(WorkerServerStarter instance, Options options)
{
	public void Start() => 
		instance.Start();

	public void Start(IBackgroundProcess additionalProcess) => 
		instance.Start(additionalProcess.AsArray());

	public void Start(IBackgroundProcess[] additionalProcesses) => 
		instance.Start(additionalProcesses);

	public void Start(ConfigurationOptions options1)
	{
		if (string.IsNullOrEmpty(options1.ConnectionString))
			options1.ConnectionString = new ConfigurationOptionsForTest().ConnectionString;
		options.UseOptions(options1);
		instance.Start();
	}
}