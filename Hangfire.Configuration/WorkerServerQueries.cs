using System.Collections.Generic;
using System.Linq;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class WorkerServerQueries
    {
        private readonly StorageCreator _storageCreator;
        private readonly StorageState _state;

        public WorkerServerQueries(StorageCreator storageCreator, StorageState state)
        {
            _storageCreator = storageCreator;
            _state = state;
        }

        public IEnumerable<JobStorage> QueryAllWorkerServers(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            _storageCreator.Create(options, storageOptions);
            return _state.State
                .Select(s => s.JobStorage)
                .ToArray();
        }
    }
}