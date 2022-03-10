using System.Collections.Generic;
using Hangfire.SqlServer;

namespace Hangfire.Configuration.Test
{
    public class WorkerServerQueriesUnderTest
    {
        private readonly WorkerServerQueries _instance;
        private readonly Options _options;

        public WorkerServerQueriesUnderTest(WorkerServerQueries instance, Options options)
        {
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ConfigurationInfo> QueryAllWorkerServers() =>
            _instance.QueryAllWorkerServers();

        public IEnumerable<ConfigurationInfo> QueryAllWorkerServers(
            ConfigurationOptions options,
            SqlServerStorageOptions storageOptions)
        {
            if (options != null)
                _options.UseOptions(options);
            if (storageOptions != null)
                _options.UseStorageOptions(storageOptions);
            return _instance.QueryAllWorkerServers();
        }
    }
}