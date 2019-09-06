using System.Collections.Generic;
using System.Linq;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class WorkerServerQueries
    {
        private readonly StorageCreator _storageCreator;

        public WorkerServerQueries(StorageCreator storageCreator)
        {
            _storageCreator = storageCreator;
        }

        public IEnumerable<WorkerServer> QueryAllWorkerServers(ConfigurationOptions options, SqlServerStorageOptions storageOptions) =>
            _storageCreator
                .Create(options, storageOptions)
                .Select(s => new WorkerServer
                {
                    JobStorage = s.JobStorage
                });
    }

    public class WorkerServer
    {
        public JobStorage JobStorage { get; set; }
    }
}