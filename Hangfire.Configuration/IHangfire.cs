using Hangfire.Server;

namespace Hangfire.Configuration;

public interface IHangfire
{
	IBackgroundProcessingServer StartBackgroundJobServer(
		JobStorage storage,
		BackgroundJobServerOptions options,
		IBackgroundProcess[] additionalProcesses);

	IBackgroundProcessingServer StartBackgroundProcesses(
		JobStorage storage, 
		IBackgroundProcess[] processes);

	JobStorage MakeJobStorage(string connectionString, object options);
}