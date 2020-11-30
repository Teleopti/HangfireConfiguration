using System;
using System.Linq;
using Hangfire.Common;
using Hangfire.Server;

namespace Hangfire.Configuration
{
    public class ServerCountSampleRecorder : IBackgroundProcess
    {
        private readonly IServerCountSampleRepository _repository;
        private readonly State _state;
        private readonly StateMaintainer _stateMaintainer;

        internal ServerCountSampleRecorder(
            IServerCountSampleRepository repository, 
            State state,
            StateMaintainer stateMaintainer)
        {
            _repository = repository;
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
            _repository.Write(new ServerCountSample {Count = serverCount});
        }
    }
}