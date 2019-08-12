using System;
using System.Collections.Generic;
using Hangfire.Server;
using Hangfire.SqlServer;
using Owin;

namespace Hangfire.Configuration
{
    public class HangfireConfiguration
    {
        private static IEnumerable<RunningServer> _runningServers;

        public static IEnumerable<RunningServer> RunningServers() => _runningServers;

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

        public HangfireConfiguration StartServers(BackgroundJobServerOptions serverOptions, SqlServerStorageOptions storageOptions, IBackgroundProcess[] additionalProcesses)
        {
            _runningServers = new ServerStarter(_builder, new RealHangfire(), new ConfigurationRepository(_options.ConnectionString))
                .StartServers(_options, serverOptions, storageOptions, additionalProcesses);
            return this;
        }

        private static Configuration BuildConfiguration(string connectionString)
        {
            var repository = new ConfigurationRepository(connectionString);
            var creator = new HangfireSchemaCreator();
            var configuration = new Configuration(repository, creator);
            return configuration;
        }
    }
}