using System;
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
        private TimeSpan _samplingInterval = TimeSpan.FromMinutes(10);

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
            // TakeFoo();
            context.StoppingToken.Wait(TimeSpan.FromMinutes(10));
        }

        public void Record()
        {
            _stateMaintainer.Refresh();
            if (!_state.Configurations.Any())
                return;

            bool isRecent(ServerCountSample sample)
            {
                var recentFrom = _now.UtcDateTime().Subtract(_samplingInterval);
                return sample.Timestamp > recentFrom;
            }

            var noRecentSample = _storage.Samples().Count(isRecent) == 0;

            if (noRecentSample)
            {
                var serverCount = _state.Configurations.First().CreateJobStorage().GetMonitoringApi().Servers().Count;
                _storage.Write(new ServerCountSample {Timestamp = _now.UtcDateTime(), Count = serverCount});
            }
        }
    }
}