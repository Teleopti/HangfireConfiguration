using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration;

public class PublisherQueries
{
	private readonly State _state;
	private readonly StateMaintainer _stateMaintainer;

	internal PublisherQueries(State state, StateMaintainer stateMaintainer)
	{
		_state = state;
		_stateMaintainer = stateMaintainer;
	}

	internal IEnumerable<ConfigurationInfo> QueryPublishers() =>
		_state.QueryPublishersCache.Get(queryPublishers);

	internal ConfigurationInfo GetPublisher(string connectionString, string schemaName) =>
		getPublisher(connectionString, schemaName);
	
	private ConfigurationInfo[] queryPublishers()
	{
		_stateMaintainer.Refresh();
		return _state.Configurations
			.Where(x => x.IsPublisher())
			.Select(x => new ConfigurationInfo(x))
			.ToArray();
	}
	
	private ConfigurationInfo getPublisher(string connectionString, string schemaName)
	{
		_stateMaintainer.EnsureLoaded(connectionString, schemaName);
		return _state.Configurations
			.Where(x => x.ConnectionString == connectionString)
			.Where(x => x.SchemaName == schemaName)
			.Select(x => new ConfigurationInfo(x))
			.First();
	}
}