using System;
using System.Linq;
using Hangfire.Common;
using Hangfire.Configuration.Internals;
using Hangfire.Server;

namespace Hangfire.Configuration
{
    public class ServerCountSampleRecorder : IBackgroundProcess
    {
        private readonly IKeyValueStore _store;
        private readonly State _state;
        private readonly StateMaintainer _stateMaintainer;
        private readonly INow _now;
        private readonly TimeSpan _samplingInterval = TimeSpan.FromMinutes(10);
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
            var noRecentSample = samples.Samples.Count(isRecent) == 0;

            if (noRecentSample)
            {
                var serverCount = _state.Configurations.First().MonitoringApi.Servers().Count;

                samples.Samples = samples
                    .Samples
                    .OrderByDescending(x => x.Timestamp)
                    .Take(_sampleLimit - 1)
                    .OrderBy(x => x.Timestamp)
                    .Append(new ServerCountSample {Timestamp = _now.UtcDateTime(), Count = serverCount})
                    .ToArray();

                _store.ServerCountSamples(samples);
            }
        }

        private bool isRecent(ServerCountSample sample)
        {
            var recentFrom = _now.UtcDateTime().Subtract(_samplingInterval);
            return sample.Timestamp > recentFrom;
        }
    }
}