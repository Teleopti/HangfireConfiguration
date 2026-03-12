using Hangfire.Server;

namespace Hangfire.Configuration;

public interface IHangfire
{
	BackgroundJobServer UseHangfireServer(JobStorage storage,
		BackgroundJobServerOptions options,
		params IBackgroundProcess[] additionalProcesses);

	JobStorage MakeJobStorage(string connectionString, object options);
}