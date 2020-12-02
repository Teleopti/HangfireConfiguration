using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Common;
using Hangfire.Server;

namespace Hangfire.Configuration
{
    public class ServerCountSampleRecorder : IBackgroundProcess
    {
        private readonly IServerCountSampleStorage _storage;
        private readonly State _state;
        private readonly StateMaintainer _stateMaintainer;
        private readonly INow _now;
        private readonly TimeSpan _samplingInterval = TimeSpan.FromMinutes(10);
        private readonly int _sampleLimit = 6;

        internal ServerCountSampleRecorder(
            IServerCountSampleStorage storage,
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
            context.StoppingToken.Wait(TimeSpan.FromMinutes(10));
        }

        public void Record()
        {
            _stateMaintainer.Refresh();
            if (!_state.Configurations.Any())
                return;
            
            var samples = _storage.Samples().ToArray();
            var noRecentSample = samples.Count(isRecent) == 0;

            if (noRecentSample)
            {
                var makeRoomForRecent = 1;
                var keepSamples = samples
                    .OrderByDescending(x => x.Timestamp)
                    .Take(_sampleLimit - makeRoomForRecent);

                samples
                    .Where(sample => isRemovable(sample, keepSamples))
                    .ForEach(sample => _storage.Remove(sample));
                
                var serverCount = _state.Configurations.First().CreateJobStorage().GetMonitoringApi().Servers().Count;
                _storage.Write(new ServerCountSample {Timestamp = _now.UtcDateTime(), Count = serverCount});
            }
        }

        private bool isRecent(ServerCountSample sample)
        {
            var recentFrom = _now.UtcDateTime().Subtract(_samplingInterval);
            return sample.Timestamp > recentFrom;
        }
        
        private static bool isRemovable(ServerCountSample sample, IEnumerable<ServerCountSample> keepSamples) 
            => keepSamples.All(keepSample => sample.Timestamp != keepSample.Timestamp);
    }
}