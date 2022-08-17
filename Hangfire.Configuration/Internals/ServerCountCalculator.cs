using System;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.Storage;
using Polly;

namespace Hangfire.Configuration.Internals;

internal class ServerCountCalculator
{
	private readonly IKeyValueStore _store;
        
	private static readonly Policy _retry = Policy.Handle<SqlException>(DetectTransientSqlException.IsTransient)
		.OrInner<SqlException>(DetectTransientSqlException.IsTransient)
		.WaitAndRetry(6, i => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(i, 2))));

	public ServerCountCalculator(IKeyValueStore store)
	{
		_store = store;
	}
        
	public int ServerCount(IMonitoringApi monitor, WorkerBalancerOptions options)
	{
		var serverCount = readServerCount(monitor, options);

		if (options.MinimumServerCount.HasValue)
		{
			if (serverCount < options.MinimumServerCount)
				return options.MinimumServerCount.Value;
		}
            
		return serverCount;
	}

	private int readServerCount(IMonitoringApi monitor, WorkerBalancerOptions options)
	{
		if (options.UseServerCountSampling)
		{
			var serverCount = serverCountFromSamples();
			if (serverCount.HasValue)
				return serverCount.Value;
		}
		return serverCountFromHangfire(monitor);
	}

	private static int serverCountFromHangfire(IMonitoringApi monitor)
	{
		var runningServerCount = _retry.Execute(() => monitor.Servers().Count);
		const int startingServer = 1;
		return runningServerCount + startingServer;
	}

	private int? serverCountFromSamples()
	{
		var samples = _store
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