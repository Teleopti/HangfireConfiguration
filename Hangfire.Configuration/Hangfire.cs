using System;
using Hangfire.Server;
using Hangfire.SqlServer;
#if NET472
using Owin;
#else
using Microsoft.AspNetCore.Builder;
#endif

namespace Hangfire.Configuration
{
    public interface IHangfire
    {
        void UseHangfireServer(
            JobStorage storage,
            BackgroundJobServerOptions options,
            params IBackgroundProcess[] additionalProcesses);
    }

    public interface IHangfireStorage
    {
        JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options);
        void UseStorage(JobStorage jobStorage);
    }

    public class RealHangfire : IHangfire
    {
        private readonly object _applicationBuilder;

        public RealHangfire(object applicationBuilder)
        {
            _applicationBuilder = applicationBuilder;
        }
        
        public void UseHangfireServer(
            JobStorage storage,
            BackgroundJobServerOptions options,
            params IBackgroundProcess[] additionalProcesses)
        {
#if !NET472
            ((IApplicationBuilder) _applicationBuilder).UseHangfireServer(options, additionalProcesses, storage);
#else
            ((IAppBuilder) _applicationBuilder).UseHangfireServer(storage, options, additionalProcesses);
#endif
        }
    }

    public class RealHangfireStorage : IHangfireStorage
    {
        public JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options) =>
            new SqlServerStorage(connectionString, options);

        public void UseStorage(JobStorage jobStorage) => GlobalConfiguration.Configuration.UseStorage(jobStorage);
    }
}