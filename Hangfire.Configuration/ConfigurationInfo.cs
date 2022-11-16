using Hangfire.Configuration.Internals;
using Hangfire.Storage;

namespace Hangfire.Configuration;

public class ConfigurationInfo
{
	private readonly ConfigurationState _state;

	internal ConfigurationInfo(ConfigurationState state)
	{
		_state = state;
		// storage should be created when querying
		// because... compatibility?
		// well, there's red tests without it
		_state.EnsureJobStorageInitialized();
	}

	public int? ConfigurationId => _state.Configuration?.Id;
	public string Name => _state.Configuration?.Name;
	public string ConnectionString => _state.Configuration?.ConnectionString;
	public string SchemaName => _state.Configuration?.AppliedSchemaName();

	public JobStorage JobStorage => _state.JobStorage;
	public IBackgroundJobClient BackgroundJobClient => _state.BackgroundJobClient;
	public IRecurringJobManager RecurringJobManager => _state.RecurringJobManager;
	public IMonitoringApi MonitoringApi => _state.MonitoringApi;
	
	public bool Publisher => _state.IsPublisher();
}