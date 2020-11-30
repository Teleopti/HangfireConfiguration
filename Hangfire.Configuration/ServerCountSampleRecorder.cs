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

        internal ServerCountSampleRecorder(
            IServerCountSampleStorage storage, 
            State state,
            StateMaintainer stateMaintainer)
        {
            _storage = storage;
            _state = state;
            _stateMaintainer = stateMaintainer;
        }
        
        public void Execute(BackgroundProcessContext context)
        {
            // TakeFoo();
            context.StoppingToken.Wait(TimeSpan.FromMinutes(10));
        }

        public void Record()
        {
            _stateMaintainer.Refresh(null, null);
            var serverCount = _state.Configurations.Single().CreateJobStorage().GetMonitoringApi().Servers().Count;
            _storage.Write(new ServerCountSample {Count = serverCount});
        }
    }
}