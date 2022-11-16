using System;
using System.Threading;
using Hangfire.Storage;

namespace Hangfire.Configuration.Internals;

internal class ConfigurationState
{
	private readonly Lazy<JobStorage> _jobStorage;
	private readonly Lazy<IBackgroundJobClient> _backgroundJobClient;
	private readonly Lazy<IRecurringJobManager> _recurringJobManager;
	private readonly Lazy<IMonitoringApi> _monitoringApi;

	public ConfigurationState(
		StoredConfiguration configuration,
		string connectionString,
		string schemaName,
		Func<JobStorage> jobStorageCreator
	)
	{
		Configuration = configuration;
		ConnectionString = connectionString;
		SchemaName = schemaName;
		_jobStorage = new Lazy<JobStorage>(jobStorageCreator, LazyThreadSafetyMode.ExecutionAndPublication);
		_backgroundJobClient = new Lazy<IBackgroundJobClient>(() => new BackgroundJobClient(JobStorage), LazyThreadSafetyMode.ExecutionAndPublication);
		_recurringJobManager = new Lazy<IRecurringJobManager>(() => new RecurringJobManager(JobStorage), LazyThreadSafetyMode.ExecutionAndPublication);
		_monitoringApi = new Lazy<IMonitoringApi>(() => JobStorage.GetMonitoringApi(), LazyThreadSafetyMode.ExecutionAndPublication);
	}

	public void EnsureJobStorageInitialized()
	{
		var _ = _jobStorage.Value;
	}

	internal StoredConfiguration Configuration;
	internal readonly string ConnectionString;
	internal readonly string SchemaName;
	internal JobStorage JobStorage => _jobStorage.Value;
	internal IBackgroundJobClient BackgroundJobClient => _backgroundJobClient.Value;
	internal IRecurringJobManager RecurringJobManager => _recurringJobManager.Value;
	internal IMonitoringApi MonitoringApi => _monitoringApi.Value;

	internal bool IsPublisher() =>
		Configuration == null || Configuration.IsActive();

	internal bool WorkerBalancerIsEnabled() => 
		Configuration.WorkerBalancerIsEnabled();
}