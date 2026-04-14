using System.Collections.Generic;
using System.Linq;
using Hangfire.Storage;

namespace Hangfire.Configuration.Internals;

internal class ServerCountCalculator(IKeyValueStore store)
{
	public int ServerCount(IMonitoringApi api, string[] containerQueues, WorkerBalancerOptions options)
	{
		var serverCount = readServerCount(api, containerQueues);

		if (options.MinimumServerCount.HasValue)
		{
			if (serverCount < options.MinimumServerCount)
				return options.MinimumServerCount.Value;
		}

		return serverCount;
	}

	private int readServerCount(IMonitoringApi api, string[] containerQueues)
	{
		var serverCount = serverCountFromSamples(containerQueues);
		if (serverCount.HasValue)
			return serverCount.Value;
		return serverCountFromHangfire(api, containerQueues);
	}

	private static int serverCountFromHangfire(IMonitoringApi api, string[] containerQueues)
	{
		var servers = api.Servers();
		var runningServers = servers
			.Where(s => s.WorkersCount > 0)
			.Where(s => matchesQueues(s.Queues, containerQueues))
			.Count();
		const int startingServer = 1;
		return runningServers + startingServer;
	}

	private int? serverCountFromSamples(string[] containerQueues)
	{
		var samples = store
			.ServerCountSamples()
			.Samples
			.Where(s => s.Count != 0)
			.Where(s => matchesQueues(s.Queues, containerQueues))
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

	private static bool matchesQueues(IEnumerable<string> queues, string[] containerQueues)
	{
		if (containerQueues == null)
			return true;
		if (queues == null)
			return true;
		return queues.Intersect(containerQueues).Any();
	}
}