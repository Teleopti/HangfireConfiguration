using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Owin;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class FakeHangfire : IHangfire
    {
        private readonly FakeMonitoringApi _monitor;

        public FakeHangfire(FakeMonitoringApi monitor)
        {
            _monitor = monitor;
        }
        
        public IEnumerable<(IAppBuilder builder, JobStorage storage, BackgroundJobServerOptions options, IBackgroundProcess[] backgroundProcesses)> StartedServers { get; set; } = 
            new (IAppBuilder builder, JobStorage storage, BackgroundJobServerOptions options, IBackgroundProcess[] backgroundProcesses)[0];

        public IAppBuilder UseHangfireServer(
            IAppBuilder builder,
            JobStorage storage,
            BackgroundJobServerOptions options,
            params IBackgroundProcess[] additionalProcesses)
        {
            StartedServers = StartedServers.Append((builder, storage, options, additionalProcesses));
            return builder;
        }

        public JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options) =>
            new FakeJobStorage(connectionString, options, _monitor );
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