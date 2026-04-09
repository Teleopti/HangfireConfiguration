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
		Func<JobStorage> jobStorageCreator,
		INow now
	)
	{
		Configuration = configuration;
		ConnectionString = connectionString;
		SchemaName = schemaName;
		_now = now;
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
	private readonly INow _now;
	internal JobStorage JobStorage => _jobStorage.Value;
	internal IBackgroundJobClient BackgroundJobClient => _backgroundJobClient.Value;
	internal IRecurringJobManager RecurringJobManager => _recurringJobManager.Value;
	internal IMonitoringApi MonitoringApi => _monitoringApi.Value;

	internal bool IsActive() =>
		Configuration.Active.GetValueOrDefault();

	internal bool IsPublisher()
	{
		// state can contain publishers without configuration
		// for example when client get by connection string
		if (Configuration == null) 
			return true;
		return IsActive();
	}

	internal bool WorkerBalancerIsEnabled() =>
		Configuration.DefaultContainer().WorkerBalancerIsEnabled();

	internal bool IsShutdown()
	{
		if (IsActive())
			return false;
		var shutdownAt = Configuration.ShutdownAt;
		if (shutdownAt == null)
			return false;
		return _now.UtcDateTime() >= shutdownAt.Value;
	}
}