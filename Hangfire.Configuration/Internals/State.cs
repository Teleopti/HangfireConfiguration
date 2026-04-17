using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Internals;

internal class State
{
	public State(INow now)
	{
		QueryPublishersCache = new TimedCache<IEnumerable<ConfigurationInfo>>(now);
		Lock = new object();
	}

	public object Lock { get; set; }

	public ConfigurationOptions Options { private get; set; }

	public List<object> StorageOptions { get; } = new();
	public BackgroundJobServerOptions ServerOptions { private get; set; }

	public IEnumerable<ConfigurationState> Configurations = Enumerable.Empty<ConfigurationState>();
	public bool ConfigurationUpdaterRan { get; set; }

	public ConfigurationOptions ReadOptions() =>
		Options ?? new ConfigurationOptions();

	public BackgroundJobServerOptions ReadServerOptions() =>
		ServerOptions ?? new BackgroundJobServerOptions();

	public TimedCache<IEnumerable<ConfigurationInfo>> QueryPublishersCache { get; }
}