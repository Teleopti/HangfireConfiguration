using System.Collections.Generic;
using System.Linq;
using Hangfire.Storage;

namespace Hangfire.Configuration.Internals;

internal class ServerCountCalculator(IKeyValueStore store, INow now)
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
		var samples = store.ServerCountSamples().Samples
			.Where(s => s.Count != 0)
			.ToArray();

		var recentCutoff = now.UtcDateTime().Subtract(ServerCountSamplingPolicy.RecentWindow);

		var tiers = new[]
		{
			samples.Where(s => isExactMatch(s.Queues, containerQueues) && s.Timestamp > recentCutoff),
			samples.Where(s => isExactMatch(s.Queues, containerQueues)),
			samples.Where(s => matchesQueues(s.Queues, containerQueues) && s.Timestamp > recentCutoff),
			samples.Where(s => matchesQueues(s.Queues, containerQueues))
		};

		var tier = tiers.FirstOrDefault(t => t.Any());
		if (tier == null)
			return null;

		return tier
			.OrderBy(s => s.Timestamp)
			.GroupBy(s => s.Count)
			.OrderByDescending(g => g.Count())
			.First().Key;
	}

	private static bool isExactMatch(IEnumerable<string> queues, string[] containerQueues)
	{
		if (containerQueues == null) return queues == null;
		if (queues == null) return false;
		return queues.OrderBy(q => q).SequenceEqual(containerQueues.OrderBy(q => q));
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
