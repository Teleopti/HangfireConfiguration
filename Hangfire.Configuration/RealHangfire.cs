using Hangfire.Configuration.Providers;
using Hangfire.Server;

namespace Hangfire.Configuration;

public class RealHangfire : IHangfire
{
	public BackgroundJobServer UseHangfireServer(JobStorage storage,
		BackgroundJobServerOptions options,
		params IBackgroundProcess[] additionalProcesses)
	{
		return new BackgroundJobServer(options, storage, additionalProcesses);
	}

	public JobStorage MakeJobStorage(string connectionString, object options)
	{
		return connectionString.GetProvider()
			.NewStorage(connectionString, options);
	}
}