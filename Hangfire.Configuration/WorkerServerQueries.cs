using System.Collections.Generic;
using System.Linq;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class WorkerServerQueries
    {
        private readonly StateMaintainer _stateMaintainer;
        private readonly State _state;

        public WorkerServerQueries(StateMaintainer stateMaintainer, State state)
        {
            _stateMaintainer = stateMaintainer;
            _state = state;
        }

        public IEnumerable<JobStorage> QueryAllWorkerServers(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            _stateMaintainer.Refresh(options, storageOptions);
            return _state.Configurations.Values
                .Select(s => s.CreateJobStorage())
                .ToArray();
        }
    }
}