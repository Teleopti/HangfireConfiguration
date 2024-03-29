using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration;

public class Options
{
	private readonly State _state;

	internal Options(State state)
	{
		_state = state;
	}

	public void UseOptions(ConfigurationOptions options) =>
		_state.Options = options;

	public void UseStorageOptions(object storageOptions) =>
		_state.StorageOptions.Add(storageOptions);

	public void UseServerOptions(BackgroundJobServerOptions serverOptions) =>
		_state.ServerOptions = serverOptions;

	public ConfigurationOptions ConfigurationOptions() =>
		_state.ReadOptions();
}