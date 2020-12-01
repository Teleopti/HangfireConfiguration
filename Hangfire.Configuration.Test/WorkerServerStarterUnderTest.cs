using Hangfire.Server;
using Hangfire.SqlServer;

namespace Hangfire.Configuration.Test
{
    public class WorkerServerStarterUnderTest
    {
        private readonly WorkerServerStarter _instance;
        private readonly Options _options;

        public WorkerServerStarterUnderTest(WorkerServerStarter instance, Options options)
        {
            _instance = instance;
            _options = options;
        }

        public void Start() => Start(null, null, null, null);

        public void Start(
            ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            SqlServerStorageOptions storageOptions) =>
            Start(options, serverOptions, storageOptions, null);

        public void Start(
            ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            SqlServerStorageOptions storageOptions,
            IBackgroundProcess backgroundProcess)
        {
            if (options != null)
                _options.UseOptions(options);
            if (serverOptions != null)
                _options.UseServerOptions(serverOptions);
            if (storageOptions != null)
                _options.UseStorageOptions(storageOptions);
            _instance.Start(backgroundProcess?.AsArray());
        }
    }
}