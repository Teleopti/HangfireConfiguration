using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Owin;

namespace Hangfire.Configuration.Test.Domain
{
    public class FakeHangfire : IHangfire
    {
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
            new FakeJobStorage(connectionString, options);
    }
    
    public class FakeJobStorage : JobStorage
    {
        public string ConnectionString { get; }
        public SqlServerStorageOptions Options { get; }

        public FakeJobStorage(string connectionString, SqlServerStorageOptions options)
        {
            ConnectionString = connectionString;
            Options = options;
        }

        public override IMonitoringApi GetMonitoringApi()
        {
            throw new System.NotImplementedException();
        }

        public override IStorageConnection GetConnection()
        {
            throw new System.NotImplementedException();
        }
    }
}