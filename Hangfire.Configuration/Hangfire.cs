using Hangfire.Server;
using Hangfire.SqlServer;
using Owin;

namespace Hangfire.Configuration
{
    public interface IHangfire
    {
        IAppBuilder UseHangfireServer(
            IAppBuilder builder,
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
        public IAppBuilder UseHangfireServer(
            IAppBuilder builder,
            JobStorage storage,
            BackgroundJobServerOptions options,
            params IBackgroundProcess[] additionalProcesses) =>
            builder.UseHangfireServer(storage, options, additionalProcesses);
    }

    public class RealHangfireStorage : IHangfireStorage
    {
        public JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options) =>
            new SqlServerStorage(connectionString, options);

        public void UseStorage(JobStorage jobStorage) => GlobalConfiguration.Configuration.UseStorage(jobStorage);
    }
}