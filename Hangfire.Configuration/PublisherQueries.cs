using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration;

public class PublisherQueries
{
	private readonly Options _options;
	private readonly State _state;
	private readonly StateMaintainer _stateMaintainer;

	internal PublisherQueries(Options options, State state, StateMaintainer stateMaintainer)
	{
		_options = options;
		_state = state;
		_stateMaintainer = stateMaintainer;
	}

	public IEnumerable<ConfigurationInfo> QueryPublishers() =>
		_state.PublisherQueryCache.Get(queryPublishers);

	private ConfigurationInfo[] queryPublishers()
	{
		_stateMaintainer.Refresh();
		return _state.Configurations
			.Where(x => x.Configuration.IsActive())
			.Select(x => new ConfigurationInfo(x))
			.ToArray();
	}
}