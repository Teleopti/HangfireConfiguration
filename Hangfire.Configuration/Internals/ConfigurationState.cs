using System;
using System.Threading;

namespace Hangfire.Configuration.Internals;

internal class ConfigurationState
{
	private readonly Lazy<IBackgroundJobClient> _backgroundJobClient;
	private readonly Lazy<JobStorage> _jobStorage;

	public ConfigurationState(
		StoredConfiguration configuration,
		Func<JobStorage> jobStorageCreator
		)
	{
		Configuration = configuration;
		_jobStorage = new Lazy<JobStorage>(jobStorageCreator, LazyThreadSafetyMode.ExecutionAndPublication);
		_backgroundJobClient = new Lazy<IBackgroundJobClient>(() => new BackgroundJobClient(JobStorage), LazyThreadSafetyMode.ExecutionAndPublication);
	}
	
	public void EnsureJobStorageInitialized()
	{
		var _ = _jobStorage.Value;
	}

	internal StoredConfiguration Configuration { get; set; }
	internal JobStorage JobStorage => _jobStorage.Value;
	internal IBackgroundJobClient BackgroundJobClient => _backgroundJobClient.Value;
}