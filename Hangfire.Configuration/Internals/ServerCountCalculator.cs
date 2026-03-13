using System.Linq;
using Hangfire.Storage;

namespace Hangfire.Configuration.Internals;

internal class ServerCountCalculator(IKeyValueStore store)
{
	public int ServerCount(IMonitoringApi api, WorkerBalancerOptions options)
	{
		var serverCount = readServerCount(api, options);

		if (options.MinimumServerCount.HasValue)
		{
			if (serverCount < options.MinimumServerCount)
				return options.MinimumServerCount.Value;
		}

		return serverCount;
	}

	private int readServerCount(IMonitoringApi api, WorkerBalancerOptions options)
	{
		var serverCount = serverCountFromSamples();
		if (serverCount.HasValue)
			return serverCount.Value;
		return serverCountFromHangfire(api);
	}

	private static int serverCountFromHangfire(IMonitoringApi api)
	{
		var servers = api.Servers();
		var runningWorkerServers = servers.Count(s => s.WorkersCount > 0);
		const int startingServer = 1;
		return runningWorkerServers + startingServer;
	}

	private int? serverCountFromSamples()
	{
		var samples = store
			.ServerCountSamples()
			.Samples
			.Where(s => s.Count != 0)
			.ToArray();
		if (samples.Any())
		{
			return samples
				.OrderBy(s => s.Timestamp)
				.GroupBy(s => new {count = s.Count})
				.OrderByDescending(g => g.Count())
				.First().Key.count;
		}

		return null;
	}
}