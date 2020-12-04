using System;
using System.Linq;
using Hangfire.Common;
using Hangfire.Server;

namespace Hangfire.Configuration
{
    public class ServerCountSampleRecorder : IBackgroundProcess
    {
        private readonly IKeyValueStore _storage;
        private readonly State _state;
        private readonly StateMaintainer _stateMaintainer;
        private readonly INow _now;
        private readonly TimeSpan _samplingInterval = TimeSpan.FromMinutes(10);
        private readonly int _sampleLimit = 6;

        internal ServerCountSampleRecorder(
            IKeyValueStore storage,
            State state,
            StateMaintainer stateMaintainer,
            INow now)
        {
            _storage = storage;
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

            var samples = _storage.Read();
            var noRecentSample = samples.Samples.Count(isRecent) == 0;

            if (noRecentSample)
            {
                var serverCount = _state.Configurations.First().CreateJobStorage().GetMonitoringApi().Servers().Count;

                samples.Samples = samples
                    .Samples
                    .OrderByDescending(x => x.Timestamp)
                    .Take(_sampleLimit - 1)
                    .OrderBy(x => x.Timestamp)
                    .Append(new ServerCountSample {Timestamp = _now.UtcDateTime(), Count = serverCount})
                    .ToArray();

                _storage.Write(samples);
            }
        }

        private bool isRecent(ServerCountSample sample)
        {
            var recentFrom = _now.UtcDateTime().Subtract(_samplingInterval);
            return sample.Timestamp > recentFrom;
        }
    }
}