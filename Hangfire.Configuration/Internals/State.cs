using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Internals;

internal class State
{
	public State(INow now)
	{
		QueryPublishersCache = new TimedCache<IEnumerable<ConfigurationInfo>>(now);
	}
	
	public ConfigurationOptions Options { private get; set; }

	public List<object> StorageOptions { get; } = new();
	public BackgroundJobServerOptions ServerOptions { get; set; }

	public IEnumerable<ConfigurationState> Configurations = Enumerable.Empty<ConfigurationState>();
	public bool ConfigurationUpdaterRan { get; set; }

	public ConfigurationOptions ReadOptions() =>
		Options ?? new ConfigurationOptions();

	public TimedCache<IEnumerable<ConfigurationInfo>> QueryPublishersCache { get; }
}