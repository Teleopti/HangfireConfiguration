using System;
using Hangfire.Server;
using Hangfire.SqlServer;
using Owin;

namespace Hangfire.Configuration
{
    public class HangfireConfiguration
    {
        private readonly IAppBuilder _builder;
        private readonly ConfigurationOptions _options;

        public HangfireConfiguration(IAppBuilder builder, ConfigurationOptions options)
        {
            _builder = builder;
            _options = options;
        }

        [Obsolete("Dont use directly, will be removed")]
        public static WorkerDeterminer GetWorkerDeterminer(string connectionString)
        {
            var configuration = BuildConfiguration(connectionString);
            return new WorkerDeterminer(configuration, JobStorage.Current.GetMonitoringApi());
        }
        
        public HangfireConfiguration StartServers(BackgroundJobServerOptions serverOptions, SqlServerStorageOptions storageOptions, params IBackgroundProcess[] additionalProcesses)
        {
            var configuration = BuildConfiguration(_options.ConnectionString);
            new ServerStarter(_builder, configuration, new RealHangfire())
                .StartServers(serverOptions, storageOptions, additionalProcesses);
            return this;
        }
		
        private static Configuration BuildConfiguration(string connectionString)
        {
            var repository = new ConfigurationRepository(connectionString);
            var configuration = new Configuration(repository);
            return configuration;
        }
    }

}