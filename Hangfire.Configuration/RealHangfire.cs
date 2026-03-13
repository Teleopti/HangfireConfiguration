using Hangfire.Configuration.Providers;
using Hangfire.Server;

namespace Hangfire.Configuration;

public class RealHangfire : IHangfire
{
	public IBackgroundProcessingServer StartBackgroundJobServer(
		JobStorage storage, 
		BackgroundJobServerOptions options, 
		IBackgroundProcess[] additionalProcesses)
	{
		return new BackgroundJobServer(options, storage, additionalProcesses);
	}

	public IBackgroundProcessingServer StartBackgroundProcesses(
		JobStorage storage, 
		IBackgroundProcess[] processes)
	{
		return new BackgroundProcessingServer(storage, processes);
	}

	public JobStorage MakeJobStorage(string connectionString, object options)
	{
		return connectionString.GetProvider()
			.NewStorage(connectionString, options);
	}
}