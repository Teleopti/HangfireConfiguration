using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
using Hangfire.Storage;

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
    }

    public class FakeJobStorage : JobStorage
    {
        public string ConnectionString { get; }
        public SqlServerStorageOptions Options { get; }
        private readonly IMonitoringApi _monitoringApi;

        public FakeJobStorage(string connectionString, SqlServerStorageOptions options, FakeMonitoringApi monitoringApi)
        {
            ConnectionString = connectionString;
            Options = options;
            _monitoringApi = monitoringApi;
        }

        public override IMonitoringApi GetMonitoringApi() => _monitoringApi;

        public override IStorageConnection GetConnection()
        {
            throw new System.NotImplementedException();
        }
    }
}