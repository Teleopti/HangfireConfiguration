using System;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.Storage;
using Polly;

namespace Hangfire.Configuration
{
    internal class ServerCountDeterminer
    {
        private readonly IKeyValueStore _keyValueStore;
        
        private static readonly Policy _retry = Policy.Handle<SqlException>(DetectTransientSqlException.IsTransient)
            .OrInner<SqlException>(DetectTransientSqlException.IsTransient)
            .WaitAndRetry(6, i => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(i, 2))));

        public ServerCountDeterminer(IKeyValueStore keyValueStore)
        {
            _keyValueStore = keyValueStore;
        }
        
        public int DetermineServerCount(IMonitoringApi monitor, WorkerDeterminerOptions options)
        {
            var serverCount = readServerCount(monitor, options);

            if (options.MinimumServerCount.HasValue)
            {
                if (serverCount < options.MinimumServerCount)
                    return options.MinimumServerCount.Value;
            }
            
            return serverCount;
        }

        private int readServerCount(IMonitoringApi monitor, WorkerDeterminerOptions options)
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
            var startingServer = 1;
            return runningServerCount + startingServer;
        }

        private int? serverCountFromSamples()
        {
            var samples = _keyValueStore
                .Read()
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
}