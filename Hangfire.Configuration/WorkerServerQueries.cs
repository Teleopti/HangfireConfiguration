using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration
{
    public class WorkerServerQueries
    {
        private readonly StateMaintainer _stateMaintainer;
        private readonly State _state;

        internal WorkerServerQueries(StateMaintainer stateMaintainer, State state)
        {
            _stateMaintainer = stateMaintainer;
            _state = state;
        }

        public IEnumerable<ConfigurationInfo> QueryAllWorkerServers()
        {
            _stateMaintainer.Refresh();
            return _state.Configurations
                .Select(x => new ConfigurationInfo(x))
                .ToArray();
        }
    }
}