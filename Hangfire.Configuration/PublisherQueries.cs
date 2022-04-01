using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration;

public class PublisherQueries
{
	private readonly Options _options;
	private readonly State _state;
	private readonly StateMaintainer _stateMaintainer;
	private readonly INow _now;

	internal PublisherQueries(Options options, State state, StateMaintainer stateMaintainer, INow now)
	{
		_options = options;
		_state = state;
		_stateMaintainer = stateMaintainer;
		_now = now;
	}

	public IEnumerable<ConfigurationInfo> QueryPublishers()
	{
		if (_options.ConfigurationOptions().CachePublisherQuery_Experimental)
		{
			if (_state.PublisherQueryCache == null || _state.PublisherQueryCacheTimeout <= _now.UtcDateTime())
			{
				_stateMaintainer.Refresh();
				_state.PublisherQueryCacheTimeout = _now.UtcDateTime().AddMinutes(1);
				_state.PublisherQueryCache = _state.Configurations
					.Where(x => x.Configuration.Active.GetValueOrDefault())
					.Select(x => new ConfigurationInfo(x))
					.ToArray();
			}

			return _state.PublisherQueryCache;
		}

		_stateMaintainer.Refresh();
		return _state.Configurations
			.Where(x => x.Configuration.Active.GetValueOrDefault())
			.Select(x => new ConfigurationInfo(x))
			.ToArray();
	}
}