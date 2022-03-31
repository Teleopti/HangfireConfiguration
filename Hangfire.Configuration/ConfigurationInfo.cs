using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration;

public class ConfigurationInfo
{
	private readonly ConfigurationState _state;

	internal ConfigurationInfo(ConfigurationState state)
	{
		_state = state;
		// storage should be created when querying
		// because... compatibility?
		_state.EnsureJobStorageInitialized();
	}

	public int ConfigurationId => _state.Configuration.Id.Value;
	public string Name => _state.Configuration.Name;
	public JobStorage JobStorage => _state.JobStorage;
	public IBackgroundJobClient BackgroundJobClient => _state.BackgroundJobClient;
}