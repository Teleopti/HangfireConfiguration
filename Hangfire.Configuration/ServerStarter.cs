using System;
using System.Collections.Generic;
using System.Linq;
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

        JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options);
    }

    public class RealHangfire : IHangfire
    {
        public IAppBuilder UseHangfireServer(
            IAppBuilder builder,
            JobStorage storage,
            BackgroundJobServerOptions options,
            params IBackgroundProcess[] additionalProcesses) =>
            builder.UseHangfireServer(storage, options, additionalProcesses);

        public JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options) =>
            new SqlServerStorage(connectionString, options);
    }

    public class ServerStarter
    {
        private readonly IAppBuilder _builder;
        private readonly Configuration _configuration;
        private readonly IHangfire _hangfire;

        public ServerStarter(IAppBuilder builder, Configuration configuration, IHangfire hangfire)
        {
            _builder = builder;
            _configuration = configuration;
            _hangfire = hangfire;
        }

        public IEnumerable<RunningServer> StartServers(
            ConfigurationOptions options, 
            BackgroundJobServerOptions serverOptions,
            SqlServerStorageOptions storageOptions,
            params IBackgroundProcess[] additionalProcesses)
        {
            var runningServers = new List<RunningServer>();
            var serverNumber = 1;

            if (options?.DefaultHangfireConnectionString != null)
                _configuration.ConfigureDefaultStorage(options?.DefaultHangfireConnectionString, options?.DefaultSchemaName);
            
            if (serverOptions != null)
                serverOptions.ServerName = null;
            
            var storedConfigs = _configuration.ReadConfigurations().ToArray();

            foreach (var storedConfig in storedConfigs)
            {
                var appliedStorageOptions = new SqlServerStorageOptions
                {
                    SchemaName = storedConfig.SchemaName
                };

                if (storageOptions != null)
                {
                    appliedStorageOptions.PrepareSchemaIfNecessary = storageOptions.PrepareSchemaIfNecessary;
                    appliedStorageOptions.QueuePollInterval = storageOptions.QueuePollInterval;
                    appliedStorageOptions.SlidingInvisibilityTimeout = storageOptions.SlidingInvisibilityTimeout;
                    appliedStorageOptions.InvisibilityTimeout = storageOptions.InvisibilityTimeout;
                    appliedStorageOptions.JobExpirationCheckInterval = storageOptions.JobExpirationCheckInterval;
                    appliedStorageOptions.CountersAggregateInterval = storageOptions.CountersAggregateInterval;
                    appliedStorageOptions.DashboardJobListLimit = storageOptions.DashboardJobListLimit;
                    appliedStorageOptions.TransactionTimeout = storageOptions.TransactionTimeout;
                    appliedStorageOptions.DisableGlobalLocks = storageOptions.DisableGlobalLocks;
                    appliedStorageOptions.UsePageLocksOnDequeue = storageOptions.UsePageLocksOnDequeue;
                }

                var sqlJobStorage = _hangfire.MakeSqlJobStorage(storedConfig.ConnectionString, appliedStorageOptions);
                _hangfire.UseHangfireServer(_builder, sqlJobStorage, serverOptions, additionalProcesses);
                
                runningServers.Add(new RunningServer() {Number = serverNumber, Storage = sqlJobStorage});
                serverNumber++;
                
                additionalProcesses = new IBackgroundProcess[] { };
            }

            return runningServers;
        }
    }
}