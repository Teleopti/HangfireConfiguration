using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Common;
using Hangfire.Configuration.Internals;
using Hangfire.Server;

namespace Hangfire.Configuration;

public class ServerCountSampleRecorder : IBackgroundProcess
{
	private readonly IKeyValueStore _store;
	private readonly State _state;
	private readonly StateMaintainer _stateMaintainer;
	private readonly INow _now;
	private readonly TimeSpan _samplingInterval = TimeSpan.FromMinutes(10);
	private readonly TimeSpan _sampleMaxAge = TimeSpan.FromHours(24);
	private const int _sampleLimit = 6;

	internal ServerCountSampleRecorder(
		IKeyValueStore store,
		State state,
		StateMaintainer stateMaintainer,
		INow now)
	{
		_store = store;
		_state = state;
		_stateMaintainer = stateMaintainer;
		_now = now;
	}

	public void Execute(BackgroundProcessContext context)
	{
		Record();
		context.StoppingToken.Wait(TimeSpan.FromMinutes(10));
	}

	public void Record()
	{
		_stateMaintainer.Refresh();
		if (!_state.Configurations.Any())
			return;

		var samples = _store.ServerCountSamples();
		if (samples.Samples.Any(isRecent))
			return;

		var api = _state.Configurations.First().MonitoringApi;
		var servers = api.Servers().Where(s => s.WorkersCount > 0).ToArray();
		var now = _now.UtcDateTime();

		var newSamples = servers.Length == 0
			? [new ServerCountSample {Timestamp = now, Count = 0}]
			: servers
				.GroupBy(s => queuesKey(s.Queues))
				.Select(g => new ServerCountSample
				{
					Timestamp = now,
					Count = g.Count(),
					Queues = g.First().Queues?.ToArray()
				})
				.ToArray();

		var result = samples.Samples.Concat(newSamples);

		var cutoff = now.Subtract(_sampleMaxAge);
		result = result.Where(s => s.Timestamp > cutoff);

		result = result
			.GroupBy(s => queuesKey(s.Queues))
			.SelectMany(g => g
				.OrderByDescending(x => x.Timestamp)
				.Take(_sampleLimit)
			);

		samples.Samples = result.OrderBy(x => x.Timestamp).ToArray();

		_store.ServerCountSamples(samples);
	}

	private bool isRecent(ServerCountSample sample)
	{
		var recentFrom = _now.UtcDateTime().Subtract(_samplingInterval);
		return sample.Timestamp > recentFrom;
	}

	private static string queuesKey(IEnumerable<string> queues) =>
		queues != null ? string.Join(",", queues.OrderBy(q => q)) : "";
}