using System.Collections.Generic;
using System.Linq;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class WorkerServerQueries
    {
        private readonly StorageCreator _storageCreator;
        private readonly State _state;

        public WorkerServerQueries(StorageCreator storageCreator, State state)
        {
            _storageCreator = storageCreator;
            _state = state;
        }

        public IEnumerable<JobStorage> QueryAllWorkerServers(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            _storageCreator.Refresh(options, storageOptions);
            return _state.Configurations
                .Select(s => s.CreateJobStorage())
                .ToArray();
        }
    }
}