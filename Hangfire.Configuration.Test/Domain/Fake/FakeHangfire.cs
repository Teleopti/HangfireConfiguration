using System.Collections.Generic;
using System.Linq;
using Hangfire.PostgreSql;
using Hangfire.Server;
using Hangfire.SqlServer;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class FakeHangfire : IHangfire
    {
        private readonly object _appBuilder;
        private readonly FakeMonitoringApi _monitoringApi;

        public FakeJobStorage LastCreatedStorage => CreatedStorages.LastOrDefault();
        public IEnumerable<FakeJobStorage> CreatedStorages = Enumerable.Empty<FakeJobStorage>();

        public FakeHangfire(object appBuilder, FakeMonitoringApi monitoringApi)
        {
            _appBuilder = appBuilder;
            _monitoringApi = monitoringApi;
        }

        public IEnumerable<(object builder, FakeJobStorage storage, BackgroundJobServerOptions options, IBackgroundProcess[] backgroundProcesses)> StartedServers { get; set; } =
            new (object builder, FakeJobStorage storage, BackgroundJobServerOptions options, IBackgroundProcess[] backgroundProcesses)[0];

        public void UseHangfireServer(
            JobStorage storage,
            BackgroundJobServerOptions options,
            params IBackgroundProcess[] additionalProcesses)
        {
            StartedServers = StartedServers.Append((_appBuilder, storage as FakeJobStorage, options, additionalProcesses)).ToArray();
        }

        public JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options)
        {
            var storage = new FakeJobStorage(connectionString, options, _monitoringApi);
            CreatedStorages = CreatedStorages.Append(storage).ToArray();
            return storage;
        }

        public JobStorage MakeSqlJobStorage(string connectionString, PostgreSqlStorageOptions options)
        {
			var storage = new FakeJobStorage(connectionString, options, _monitoringApi);
			CreatedStorages = CreatedStorages.Append(storage).ToArray();
			return storage;
		}
    }
}